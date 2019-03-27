﻿using System.Collections.Generic;

namespace MrCMS.Tasks
{
    public class TaskRunner : ITaskRunner
    {
        private readonly ITaskResetter _taskResetter;
        private readonly ITaskQueuer _taskQueuer;
        private readonly ITaskBuilder _taskBuilder;
        private readonly ITaskExecutor _taskExecutor;

        public TaskRunner(ITaskResetter taskResetter, ITaskQueuer taskQueuer, ITaskBuilder taskBuilder, ITaskExecutor taskExecutor)
        {
            _taskResetter = taskResetter;
            _taskQueuer = taskQueuer;
            _taskBuilder = taskBuilder;
            _taskExecutor = taskExecutor;
        }

        public BatchExecutionResult ExecutePendingTasks()
        {
            _taskResetter.ResetHungTasks();

            IList<QueuedTask> pendingQueuedTasks = _taskQueuer.GetPendingQueuedTasks();
            IList<ScheduledTask> pendingScheduledTasks = _taskQueuer.GetPendingScheduledTasks();

            IList<IExecutableTask> tasksToExecute = _taskBuilder.GetTasksToExecute(pendingQueuedTasks, pendingScheduledTasks);

            return _taskExecutor.Execute(tasksToExecute);
        }

        public BatchExecutionResult ExecuteLuceneTasks()
        {
            IList<QueuedTask> pendingQueuedTasks = _taskQueuer.GetPendingLuceneTasks();

            IList<IExecutableTask> tasksToExecute = _taskBuilder.GetTasksToExecute(pendingQueuedTasks, new List<ScheduledTask>());

            return _taskExecutor.Execute(tasksToExecute);

        }
    }
}