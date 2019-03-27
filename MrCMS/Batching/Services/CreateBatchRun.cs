using System.Collections.Generic;
using MrCMS.Batching.Entities;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Batching.Services
{
    public class CreateBatchRun : ICreateBatchRun
    {
        private readonly IStatelessSession _statelessSession;

        public CreateBatchRun(IStatelessSession statelessSession)
        {
            _statelessSession = statelessSession;
        }

        public BatchRun Create(Batch batch)
        {
            if (batch == null)
                return null;
            BatchJob jobAlias = null;

            QueryOver<BatchRunResult, BatchRunResult> subQuery = QueryOver.Of<BatchRunResult>()
                .Where(result => result.Status != JobExecutionStatus.Failed && result.BatchJob.Id == jobAlias.Id)
                .Select(result => result.Id);

            IList<BatchJob> jobs = _statelessSession.QueryOver(() => jobAlias)
                .Where(job => job.Batch.Id == batch.Id)
                .WithSubquery.WhereNotExists(subQuery)
                .List();

            // we need to make sure that the site is loaded from the correct session
            Site site = _statelessSession.Get<Site>(batch.Site.Id);
            return _statelessSession.Transact(session =>
            {
                System.DateTime now = CurrentRequestData.Now;
                BatchRun batchRun = new BatchRun
                {
                    Batch = batch,
                    BatchRunResults = new List<BatchRunResult>(),
                    Site = site,
                    CreatedOn = now,
                    UpdatedOn = now
                };
                session.Insert(batchRun);
                for (int index = 0; index < jobs.Count; index++)
                {
                    BatchJob batchJob = jobs[index];
                    BatchRunResult batchRunResult = new BatchRunResult
                    {
                        BatchJob = batchJob,
                        Status = JobExecutionStatus.Pending,
                        ExecutionOrder = index,
                        BatchRun = batchRun,
                        Site = site,
                        CreatedOn = now,
                        UpdatedOn = now
                    };
                    batchRun.BatchRunResults.Add(batchRunResult);
                    session.Insert(batchRunResult);
                }
                return batchRun;
            });
        }
    }
}