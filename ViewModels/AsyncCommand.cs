using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace musical_journey.Commands;

/// <summary>
/// Async command implementation for commands without parameters
/// </summary>
public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public AsyncCommand(Func<Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting)
            return;

        _isExecuting = true;
        OnCanExecuteChanged();

        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    protected virtual void OnCanExecuteChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Dispatcher.UIThread.Post(() => OnCanExecuteChanged());
        }
    }
}

/// <summary>
/// Async command implementation for commands with parameters
/// </summary>
/// <typeparam name="T">The type of the command parameter</typeparam>
public class AsyncCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private bool _isExecuting;

    public AsyncCommand(Func<T?, Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting)
            return;

        _isExecuting = true;
        OnCanExecuteChanged();

        try
        {
            await _execute((T?)parameter);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    protected virtual void OnCanExecuteChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Dispatcher.UIThread.Post(() => OnCanExecuteChanged());
        }
    }
}