using System;
using System.Collections.Generic;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class ActionQueue
    {
        private readonly Queue<Action> _queuedActions = new Queue<Action>();
        private readonly HashSet<Action> _runningActions = new HashSet<Action>();
        private bool _isProcessing = false;

        public void EnqueueAction(Action action)
        {
            if (action == null)
            {
                ZenLog.Error(LogCategory.System, "[ActionQueue] -> EnqueueAction: Tried to enqueue a null action");
                return;
            }

            _queuedActions.Enqueue(action);
        }

        public bool ProcessQueuedActions()
        {
            if (_isProcessing)
            {
                ZenLog.Warning(LogCategory.System, "[ActionQueue] -> ProcessQueuedActions: Already processing actions");
                return false;
            }

            _isProcessing = true;
            bool processedSomething = false;
            try
            {
                while (_queuedActions.Count > 0)
                {
                    var action = _queuedActions.Dequeue();
                    ExecuteAction(action);
                    processedSomething = true;
                }
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[ActionQueue] -> ProcessQueuedActions: Exception occurred: {e}");
            }
            finally
            {
                _isProcessing = false;
            }

            return processedSomething;
        }

        private void ExecuteAction(Action action)
        {
            if (action == null)
            {
                return;
            }

            _runningActions.Add(action);

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[ActionQueue] -> ExecuteAction: Action failed with exception: {e}");
            }
            finally
            {
                _runningActions.Remove(action);
            }
        }

        public void Clear()
        {
            int count = _queuedActions.Count;
            _queuedActions.Clear();
        }

        public bool IsIdle()
        {
            return _queuedActions.Count == 0 && _runningActions.Count == 0;
        }
        
        public int QueueCount()
        {
            return _queuedActions.Count;
        }
    }
}