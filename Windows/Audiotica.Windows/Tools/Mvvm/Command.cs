using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Audiotica.Windows.Tools.Mvvm
{
    // http://codepaste.net/jgxazh

    public class Command : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public Command(Action execute)
            : this(execute, () => true)
        {
            /* empty */
        }

        public Command(Action execute, Func<bool> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canexecute;
        }

        public event EventHandler CanExecuteChanged;

        [DebuggerStepThrough]
        public bool CanExecute(object p)
        {
            try
            {
                return _canExecute == null || _canExecute();
            }
            catch
            {
                Debugger.Break();
                return false;
            }
        }

        public void Execute(object p)
        {
            if (!CanExecute(p)) return;
            try
            {
                _execute();
            }
            catch
            {
                Debugger.Break();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Command<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public Command(Action<T> execute)
            : this(execute, x => true)
        {
            /* empty */
        }

        public Command(Action<T> execute, Func<T, bool> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canexecute;
        }

        public event EventHandler CanExecuteChanged;

        [DebuggerStepThrough]
        public bool CanExecute(object p)
        {
            try
            {
                var value = (T) Convert.ChangeType(p, typeof (T));
                return _canExecute?.Invoke(value) ?? true;
            }
            catch
            {
                Debugger.Break();
                return false;
            }
        }

        public void Execute(object p)
        {
            if (!CanExecute(p)) return;
            try
            {
                var value = (T) Convert.ChangeType(p, typeof (T));
                _execute(value);
            }
            catch
            {
                Debugger.Break();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}