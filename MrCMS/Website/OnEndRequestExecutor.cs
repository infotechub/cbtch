using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MrCMS.Helpers;
using MrCMS.Services.Resources;
using Ninject;

namespace MrCMS.Website
{
    public class OnEndRequestExecutor : IOnEndRequestExecutor
    {
        private readonly IKernel _kernel;
        private static readonly Dictionary<Type, Type> OnRequestExecutionTypes = new Dictionary<Type, Type>();

        static OnEndRequestExecutor()
        {
            foreach (Type type in TypeHelper.GetAllConcreteTypesAssignableFrom(typeof(EndRequestTask<>)).Where(type => !type.ContainsGenericParameters))
            {
                Type executorType = TypeHelper.GetAllConcreteTypesAssignableFrom(
                    typeof(ExecuteEndRequestBase<,>).MakeGenericType(type, type.BaseType.GenericTypeArguments[0])).FirstOrDefault();


                OnRequestExecutionTypes.Add(type, executorType);
            }
        }
        public OnEndRequestExecutor(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void ExecuteTasks(HashSet<EndRequestTask> tasks)
        {
            Dictionary<Type, HashSet<EndRequestTask>> tasksGroupedByType = tasks.GroupBy(task => task.GetType())
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToHashSet());

            foreach (Type type in tasksGroupedByType.Keys.OrderByDescending(GetExecutionPriority))
            {
                if (OnRequestExecutionTypes.ContainsKey(type) && OnRequestExecutionTypes[type] != null)
                {
                    ExecuteEndRequestBase requestBase = _kernel.Get(OnRequestExecutionTypes[type]) as ExecuteEndRequestBase;
                    if (requestBase != null)
                    {
                        HashSet<object> data = tasksGroupedByType[type].Select(task => task.BaseData).ToHashSet();
                        requestBase.Execute(data);
                        continue;

                    }
                }
                CurrentRequestData.ErrorSignal.Raise(
                    new Exception(
                        string.Format(
                            "Could not process tasks of type {0}. Please create a valid executor for the type",
                            type.FullName)));
            }
        }

        private int GetExecutionPriority(Type arg)
        {
            EndRequestExecutionPriorityAttribute attribute = arg.GetCustomAttribute<EndRequestExecutionPriorityAttribute>();
            if (attribute == null)
                return int.MinValue;
            return attribute.Priority;
        }
    }

    public class EndRequestExecutionPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public EndRequestExecutionPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}