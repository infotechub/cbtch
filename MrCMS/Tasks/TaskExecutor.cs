using System.Collections.Generic;
using System.Linq;

namespace MrCMS.Tasks
{
    public class TaskExecutor : ITaskExecutor
    {
        private readonly IEnumerable<ITaskExecutionHandler> _executionHandlers;

        public TaskExecutor(IEnumerable<ITaskExecutionHandler> executionHandlers)
        {
            _executionHandlers = executionHandlers;
        }

        public BatchExecutionResult Execute(IList<IExecutableTask> tasksToExecute)
        {
            List<TaskExecutionResult> results = new List<TaskExecutionResult>();
            foreach (ITaskExecutionHandler handler in _executionHandlers.OrderByDescending(handler => handler.Priority))
            {
                IList<IExecutableTask> tasks = handler.ExtractTasksToHandle(ref tasksToExecute);
                results.AddRange(handler.ExecuteTasks(tasks));
            }
            return new BatchExecutionResult { Results = results };
        }
    }
}