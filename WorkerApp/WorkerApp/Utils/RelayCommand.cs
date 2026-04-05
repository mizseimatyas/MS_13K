using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WorkerApp.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Func<object?, Task> executeAsync,
                            Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter)
            => await _executeAsync(parameter);

        public event EventHandler? CanExecuteChanged;
    }
}