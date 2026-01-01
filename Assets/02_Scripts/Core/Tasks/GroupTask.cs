using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class GroupTask : ZenTask
    {
        private ZenTask[] _tasks;
        
        public GroupTask(params ZenTask[] tasks)
        {
            _tasks = tasks;
        }

        protected override async UniTask<ETaskResult> OnExecute()
        {
            foreach (var task in _tasks)
            {
                if (task.State != ETaskState.Undefined)
                {
                    ZenLog.Error(LogCategory.System, $"[GroupTask] -> [{GetTaskName()}]: Task [{task.GetTaskName()}] is already running or finished");
                    return ETaskResult.Failure;
                }
                
                var result = await task.Execute();
               
                if (result != ETaskResult.Success)
                {
                     return result;
                }
            }
     
            return ETaskResult.Success;
        }

        public override string GetTaskName()
        {
            return $"GroupTask: [{_tasks.Length}] tasks";
        }
    }
}