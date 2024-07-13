using System.Collections.Generic;

namespace TD.DesignPattern.Command
{
    public class CommandExecuter
    {
        private Queue<ICommand> commandQueue;
        private Stack<ICommand> commandStack;
        public CommandExecuter()
        {
            commandQueue = new Queue<ICommand>();
            commandStack = new Stack<ICommand>();
        }

        #region ____QUEUE METHODS____
        public void QueueCommand(ICommand command)
        {
            commandQueue.Enqueue(command);
        }
        public void DequeueCommand()
        {
            commandQueue.Dequeue();
        }
        public void UndoLastQueuedCommand()
        {
            if (commandQueue.Count == 0)
                return;

            var cmd = commandQueue.Dequeue();
            cmd.Undo();
        }
        public void ExecuteQueuedCommands()
        {
            while (commandQueue.Count > 0)
            {
                ICommand command = commandQueue.Dequeue();
                command.Execute();
            }
        }
        #endregion

        #region ____STACK METHODS____
        public void StackCommand(ICommand command)
        {
            commandStack.Push(command);
        }
        public void PopCommand()
        {
            commandStack.Pop();
        }

        public void UndoLastStackedCommand()
        {
            if (commandStack.Count == 0)
                return;

            var cmd = commandStack.Pop();
            cmd.Undo();
        }
        public void ExecuteStackedCommands()
        {
            while (commandStack.Count > 0)
            {
                ICommand command = commandStack.Pop();
                command.Execute();
            }
        }
        #endregion
    }
}
