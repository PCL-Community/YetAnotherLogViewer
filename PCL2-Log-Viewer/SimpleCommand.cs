using System;
using System.Dynamic;
using System.Windows.Input;

namespace LogViewer;

public class SimpleCommand(Action<object?> executeCallback) : ICommand
{
    public bool CanExecute(object? parameter) => true;

    public event EventHandler? CanExecuteChanged { add {} remove {} }

    public void Execute(object? parameter = null) => executeCallback(parameter);
}
