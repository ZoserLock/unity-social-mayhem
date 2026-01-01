using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class ParallelTask : ZenTask
    {
        private readonly ZenTask _task;
        private volatile bool _running;

        public ParallelTask(ZenTask task) : base()
        {
            _task = task;
        }

        protected override void OnPrepare()
        {
            
        }

        protected override async UniTask<ETaskResult> OnExecute()
        {
            ZenLog.Info(LogCategory.System, $"[Parrallel ZenTask] -> [{GetTaskName()}]: Starting execution");

            await UniTask.RunOnThreadPool(async () =>
            {
                await _task.Execute();
                
                _running = false;
                
            },false);

            ZenLog.Info(LogCategory.System, $"[Parrallel ZenTask] -> [{GetTaskName()}]: Waiting for task to finish");
            while (_running)
            {
                await UniTask.Yield();
            }
            
            ZenLog.Info(LogCategory.System, $"[Parrallel ZenTask] -> [{GetTaskName()}]: Execution Finished with result: [{ETaskResult.Success}]");
            return ETaskResult.Success;
        }

        public override string GetTaskName()
        {
            return $"Parallel";
        }
    }
}