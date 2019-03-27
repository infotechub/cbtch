using MrCMS.Batching.Entities;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Web.Areas.Admin.Services.Batching
{
    public class GetBatchStatus : IGetBatchStatus
    {
        private readonly ISession _session;

        public GetBatchStatus(ISession session)
        {
            _session = session;
        }

        public BatchStatus Get(Batch batch)
        {
            if (batch == null)
                return new BatchStatus();
            bool anyRuns = _session.QueryOver<BatchRun>().Where(job => job.Batch.Id == batch.Id).Any();
            if (!anyRuns)
                return BatchStatus.Pending;
            bool anyPaused = _session.QueryOver<BatchRun>().Where(job => job.Batch.Id == batch.Id && job.Status == BatchRunStatus.Paused).Any();
            if (anyPaused)
                return BatchStatus.Paused;
            bool anyExecuting = _session.QueryOver<BatchRun>().Where(job => job.Batch.Id == batch.Id && job.Status == BatchRunStatus.Executing).Any();
            if (anyExecuting)
                return BatchStatus.Executing;
            if (_session.QueryOver<BatchRun>().Where(job => job.Batch.Id == batch.Id && job.Status != BatchRunStatus.Complete).Any())
                return BatchStatus.Pending;
            return BatchStatus.Complete;
        }
    }
}