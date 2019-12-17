using System;
using System.Windows.Input;

namespace UTA.Interactivity
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _predicate;

        public RelayCommand(Action<object> action) : this(action, null)
        {
        }

        public RelayCommand(Action<object> action, Predicate<object> predicate)
        {
            this._action = action ?? throw new ArgumentNullException(nameof(action), @"You must specify an Action<T>.");
            this._predicate = predicate;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _predicate == null || _predicate(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}