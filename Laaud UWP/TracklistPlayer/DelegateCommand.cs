using System;
using System.Windows.Input;

namespace Laaud_UWP
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> execute;

        public DelegateCommand(Action execute)
        {
            this.execute = (parameter) => execute();
        }

        public DelegateCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}