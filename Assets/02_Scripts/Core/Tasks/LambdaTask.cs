using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StrangeSpace
{
    public class LambdaTask : ZenTask
    {
        private readonly Func<UniTask<ETaskResult>> _function;

        public LambdaTask(Func<UniTask<ETaskResult>> function)
        {
            _function = function;
        }

        protected override async UniTask<ETaskResult> OnExecute()
        {
            await UniTask.Yield();
            return await _function();
        }

        public override string GetTaskName()
        {
            return $"Lambda";
        }
    }
}