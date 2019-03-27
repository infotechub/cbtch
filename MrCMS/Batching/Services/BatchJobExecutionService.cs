using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Batching.Entities;
using MrCMS.Helpers;
using Ninject;

namespace MrCMS.Batching.Services
{
    public class BatchJobExecutionService : IBatchJobExecutionService
    {
        private readonly IKernel _kernel;

        public BatchJobExecutionService(IKernel kernel)
        {
            _kernel = kernel;
        }

        private static readonly Dictionary<Type, Type> _executorTypeList = new Dictionary<Type, Type>();

        static BatchJobExecutionService()
        {
            HashSet<Type> batchJobTypes = TypeHelper.GetAllConcreteTypesAssignableFrom<BatchJob>();

            foreach (Type batchJobType in batchJobTypes)
            {
                Type type = typeof(BaseBatchJobExecutor<>).MakeGenericType(batchJobType);
                HashSet<Type> executorTypes = TypeHelper.GetAllTypesAssignableFrom(type);
                if (executorTypes.Any())
                {
                    _executorTypeList[batchJobType] = executorTypes.First();
                }
            }
        }

        public async Task<BatchJobExecutionResult> Execute(BatchJob batchJob)
        {
            Type type = batchJob.GetType();
            bool hasExecutorType = _executorTypeList.ContainsKey(type);
            if (hasExecutorType)
            {
                IBatchJobExecutor batchJobExecutor = _kernel.Get(_executorTypeList[type]) as IBatchJobExecutor;
                if (batchJobExecutor != null)
                    return await batchJobExecutor.Execute(batchJob);
            }

            return await _kernel.Get<DefaultBatchJobExecutor>().Execute(batchJob);
        }
    }
}