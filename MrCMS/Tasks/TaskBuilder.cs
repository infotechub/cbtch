using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Website;
using NHibernate;
using Ninject;

namespace MrCMS.Tasks
{
    public class TaskBuilder : ITaskBuilder
    {
        private readonly IKernel _kernel;
        private readonly ISession _session;

        public TaskBuilder(IKernel kernel,ISession session)
        {
            _kernel = kernel;
            _session = session;
        }

        public IList<IExecutableTask> GetTasksToExecute(IList<QueuedTask> pendingQueuedTasks, IList<ScheduledTask> pendingScheduledTasks)
        {
            List<IExecutableTask> executableTasks = new List<IExecutableTask>();
            List<QueuedTask> failedTasks = new List<QueuedTask>();
            foreach (QueuedTask queuedTask in pendingQueuedTasks)
            {
                try
                {
                    IExecutableTask task = _kernel.Get(queuedTask.GetTaskType()) as IExecutableTask;
                    task.Site = queuedTask.Site;
                    task.Entity = queuedTask;
                    task.SetData(queuedTask.Data);
                    executableTasks.Add(task);
                }
                catch (Exception exception)
                {
                    CurrentRequestData.ErrorSignal.Raise(exception);
                    failedTasks.Add(queuedTask);
                }
            }
            if (failedTasks.Any())
            {
                _session.Transact(session => failedTasks.ForEach(task =>
                {
                    task.Status = TaskExecutionStatus.Failed;
                    task.FailedAt = CurrentRequestData.Now;
                    session.Update(task);
                }));
            }
            foreach (ScheduledTask scheduledTask in pendingScheduledTasks)
            {
                Type taskType = TypeHelper.GetAllTypes().FirstOrDefault(type => type.FullName == scheduledTask.Type);
                IExecutableTask task = _kernel.Get(taskType) as IExecutableTask;
                task.Site = scheduledTask.Site;
                task.Entity = scheduledTask;
                executableTasks.Add(task);
            }
            return executableTasks;
        }
    }
}