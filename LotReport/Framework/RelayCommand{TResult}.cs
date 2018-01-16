using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public class RelayCommand<TResult> : INotifyPropertyChanged, ICommand
{
    private readonly Func<object, TResult> execute;
    private readonly Predicate<object> canExecute;
    private TResult result;

    public RelayCommand(Func<object, TResult> execute)
        : this(execute, null)
    {
    }

    public RelayCommand(Func<object, TResult> execute, Predicate<object> canExecute)
    {
        if (execute == null)
        {
            throw new ArgumentException("execute");
        }

        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public TResult Result
    {
        get
        {
            return this.result;
        }

        set
        {
            this.result = value;
            this.OnPropertyChanged();
        }
    }

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return this.canExecute == null ? true : this.canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        this.Result = this.execute(parameter);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler handler = this.PropertyChanged;

        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
