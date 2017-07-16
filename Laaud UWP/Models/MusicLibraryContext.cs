using Laaud_UWP.Util.SQLiteExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Models
{
    public class MusicLibraryContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(@"Data Source=laaud.db;")
                .ReplaceService<SqliteCompositeMethodCallTranslator, LaaudSqliteCompositeMethodCallTranslator>();
        }
    }
}
