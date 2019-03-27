using MrCMS.Website;
using NHibernate;

namespace MrCMS.Tasks
{
    public class DeleteOldQueuedTasks : SchedulableTask
    {
        private readonly ISessionFactory _sessionFactory;

        public DeleteOldQueuedTasks(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public override int Priority { get { return 10; } }

        protected override void OnExecute()
        {
            IStatelessSession statelessSession = _sessionFactory.OpenStatelessSession();
            System.Collections.Generic.IList<QueuedTask> logs =
                statelessSession.QueryOver<QueuedTask>().Where(data => data.CompletedAt <= CurrentRequestData.Now.AddDays(-1)).List();

            using (ITransaction transaction = statelessSession.BeginTransaction())
            {
                foreach (QueuedTask log in logs)
                {
                    statelessSession.Delete(log);
                }
                transaction.Commit();
            }
        }
    }
}