using System;

namespace TD.DesignPattern.Command
{
    public class GenericCommand<T> : ICommand
    {
        private Action<T> action;
        private Action<T> prevAction;
        private T parameter;

        public GenericCommand(Action<T> action, T parameter)
        {
            this.action     = action;
            this.parameter  = parameter;
        }

        public void Execute()
        {
            prevAction = action;
            action?.Invoke(parameter);
        }

        public void Undo()
        {
            action = prevAction;
            Execute();
        }
    }
}
