using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StrangeSpace
{
    public class DelayTask : ZenTask
    {
        private readonly float _timeSeconds;
        
        public DelayTask(float timeMs)
        {
            _timeSeconds = timeMs;
        }
        
        protected override async UniTask<ETaskResult> OnExecute()
        {
            await UniTask.Delay(Mathf.FloorToInt(_timeSeconds * 1000));
            
            return await UniTask.FromResult(ETaskResult.Success);
        }

        public override string GetTaskName()
        {
            return $"Delay [{_timeSeconds}] seconds";
        }
    }
}