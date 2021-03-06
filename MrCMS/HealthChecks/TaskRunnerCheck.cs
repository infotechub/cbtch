using System;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Tasks;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.HealthChecks
{
    public class TaskRunnerCheck : HealthCheck
    {
        private readonly ISession _session;

        public TaskRunnerCheck(ISession session)
        {
            _session = session;
        }

        public override string DisplayName
        {
            get { return "Task Runner"; }
        }

        public override HealthCheckResult PerformCheck()
        {
            System.Collections.Generic.IList<ScheduledTask> tasks = _session.QueryOver<ScheduledTask>().List();
            System.Collections.Generic.List<ScheduledTask> stalledTasks = tasks.Where(x => x.LastComplete <= CurrentRequestData.Now.AddSeconds(-(x.EveryXSeconds + 120)) || x.LastComplete == null).ToList();

            if (stalledTasks.Any())
            {
                System.Collections.Generic.List<string> messages = stalledTasks.Select(task =>
                {
                    DateTime? lastComplete = task.LastComplete;
                    return lastComplete.HasValue
                        ? string.Format("{0} has not been ran since {1}", task.TypeName, lastComplete)
                        : string.Format("{0} has never been run", task.TypeName);
                }).ToList();
                return new HealthCheckResult
                {
                    Messages = messages,
                    OK = false
                };
            }
            return HealthCheckResult.Success;
        }
    }
}