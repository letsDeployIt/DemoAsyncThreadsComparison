using System;
using System.Windows.Input;

namespace AsyncThreadsComparison
{
    internal class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        /// <summary> Creates instance of the command handler </summary>
        /// <param name="action">Action to be executed by the command</param>
        /// <param name="canExecute">A boolean property to containing current permissions to execute the command</param>
        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Forcess checking if execute is allowed
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object? parameter)
        {
            _action();
        }
    }
}