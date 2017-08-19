using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Laaud_UWP.DBSearch
{
    abstract class DBSearcher<TLoadedItem, TSearchTerm>
    {
        private CancellationTokenSource idsLoadCancellationTokenSource = null;
        private object idsLoadLockObject = new object();

        private CancellationTokenSource itemsLoadCancellationTokenSource = null;
        private object itemsLoadLockObject = new object();
        private Task lastItemsLoadTask = null;

        private List<int> sortedItemIds = new List<int>();
        private Dictionary<int, TLoadedItem> loadedItemsCache = new Dictionary<int, TLoadedItem>();
        private int lastLoadedItemIndex = -1;

        private bool queueWatchingTaskRunning = false;

        public DBSearcher()
        {
        }

        public async Task<bool> LoadItemIdsAsync(TSearchTerm searchTerm)
        {
            lock (this.idsLoadLockObject)
            {
                if (this.idsLoadCancellationTokenSource != null)
                {
                    this.idsLoadCancellationTokenSource.Cancel();
                }
            }

            lock (this.itemsLoadLockObject)
            {
                if (this.itemsLoadCancellationTokenSource != null)
                {
                    this.itemsLoadCancellationTokenSource.Cancel();
                }
            }

            try
            {
                return await Task.Factory.StartNew(() =>
                {
                    CancellationToken cancellationToken;
                    lock (this.idsLoadLockObject)
                    {
                        this.idsLoadCancellationTokenSource = new CancellationTokenSource();
                        cancellationToken = this.idsLoadCancellationTokenSource.Token;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // load new ids
                    List<int> newIDs = this.GetItemIdsForSearchTerms(searchTerm);

                    lock (this.idsLoadLockObject)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        this.lastLoadedItemIndex = 0;
                        this.sortedItemIds = newIDs;
                        return true;
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public async Task<List<TLoadedItem>> LoadItemsAsync(int countOfItemsToLoad)
        {
            Task<List<TLoadedItem>> loadingTask;
            if (this.lastItemsLoadTask == null || this.lastItemsLoadTask.IsCompleted)
            {
                loadingTask = Task.Factory.StartNew(() => this.LoadItemsInner(countOfItemsToLoad));
            }
            else
            {
                loadingTask = this.lastItemsLoadTask.ContinueWith((task) => this.LoadItemsInner(countOfItemsToLoad));
            }

            this.lastItemsLoadTask = loadingTask;

            try
            {
                return await loadingTask;
            }
            catch (OperationCanceledException)
            {
                return new List<TLoadedItem>();
            }
        }

        public TLoadedItem GetByPrimaryKey(int primaryKey)
        {
            TLoadedItem loadedItem;
            if (!this.loadedItemsCache.TryGetValue(primaryKey, out loadedItem))
            {
                loadedItem = this.GetByPrimaryKeys(new List<int>() { primaryKey }).FirstOrDefault();
                if (loadedItem != null)
                {
                    this.loadedItemsCache.Add(primaryKey, loadedItem);
                }
            }

            return loadedItem;
        }

        protected abstract List<int> GetItemIdsForSearchTerms(TSearchTerm searchTerm);

        protected abstract List<TLoadedItem> GetByPrimaryKeys(List<int> primaryKeyCollection);

        protected abstract int GetPrimaryKey(TLoadedItem loadedItem);

        private List<TLoadedItem> LoadItemsInner(int countOfItemsToLoad)
        {
            CancellationToken cancellationToken;
            lock (this.itemsLoadLockObject)
            {
                this.itemsLoadCancellationTokenSource = new CancellationTokenSource();
                cancellationToken = this.itemsLoadCancellationTokenSource.Token;
            }

            if (countOfItemsToLoad + this.lastLoadedItemIndex >= this.sortedItemIds.Count)
            {
                countOfItemsToLoad = (this.sortedItemIds.Count - this.lastLoadedItemIndex) - 1;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (countOfItemsToLoad > 0)
            {

                TLoadedItem[] resultCollection = new TLoadedItem[countOfItemsToLoad];
                Dictionary<int, int> itemIDsToLoad = new Dictionary<int, int>();
                for (int index = this.lastLoadedItemIndex + 1; index <= this.lastLoadedItemIndex + countOfItemsToLoad; index++)
                {
                    int itemID = this.sortedItemIds[index];
                    int itemIndexInNewCollection = index - this.lastLoadedItemIndex - 1;
                    if (this.loadedItemsCache.ContainsKey(itemID))
                    {
                        resultCollection[itemIndexInNewCollection] = this.loadedItemsCache[itemID];
                    }
                    else
                    {
                        itemIDsToLoad.Add(itemID, itemIndexInNewCollection);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                List<TLoadedItem> itemsNewlyLoaded = this.GetByPrimaryKeys(itemIDsToLoad.Keys.ToList());
                foreach (TLoadedItem loadedItem in itemsNewlyLoaded)
                {
                    int primaryKey = this.GetPrimaryKey(loadedItem);
                    this.loadedItemsCache.Add(primaryKey, loadedItem);
                    resultCollection[itemIDsToLoad[primaryKey]] = loadedItem;
                    cancellationToken.ThrowIfCancellationRequested();
                }

                this.lastLoadedItemIndex += countOfItemsToLoad;

                return resultCollection.ToList();
            }
            else
            {
                return new List<TLoadedItem>();
            }
        }
    }
}
