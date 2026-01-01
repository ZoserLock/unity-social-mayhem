using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ModestTree.Util;
using UnityEngine;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public enum ETaskState
    {
        Undefined,
        Enqueued,
        Running,
        Finished,
    }

    public enum ETaskResult
    {
        Undefined,          // Default value
        Success,            // ZenTask completed successfully 
        Failure,            // ZenTask failed but code execution was successful
        FailureException,   // ZenTask failed by throwing an exception
        Cancelled,          // ZenTask was cancelled by the user
    }
    
    public interface ITaskHandle
    {
        void Cancel();
    }
    
    public class TaskHandle : ITaskHandle, ILifeTimeListener
    {
        private readonly ZenTask _task;
        
        public TaskHandle(ZenTask task)
        {
            _task = task;
        }
        
        public void Cancel()
        {
            _task.MarkAsCancelled();
        }
        
        public void OnLifeEnd()
        {
            Cancel();
        }
    }
    
    public class ZenTask
    {
        private ETaskState _state = ETaskState.Undefined;
        private ETaskResult _result = ETaskResult.Undefined;
        
        private bool _cancelled;
        
        private float _startTime;
        private float _endTime;
        
        private Exception _failureException;
        
        // Get / Set
        public ETaskState State => _state;
        public ETaskResult Result => _result;
        public Exception FailureException => _failureException;
        
        internal void SetState(ETaskState state)
        {
            _state = state;
        }
        
        internal void SetResult(ETaskResult result)
        {
            _result = result;
        }
        
        internal void MarkAsCancelled()
        {
            _cancelled = true;
        }
        
        public async UniTask<ETaskResult> Execute()
        {
            ZenLog.Info(LogCategory.System, $"[ZenTask] -> [{GetTaskName()}]: Execution Started");
            
            if(_state != ETaskState.Enqueued && _state != ETaskState.Undefined)
            {
                ZenLog.Error(LogCategory.System,$"[ZenTask] -> [{GetTaskName()}]: Trying to execute a task that is already running, finished or not enqueued");
                return ETaskResult.Failure;
            }

            //  _startTime = Time.realtimeSinceStartup;
            
            OnPrepare();
            
            SetState(ETaskState.Running);
            
            Exception exception = null;
            ETaskResult result = ETaskResult.Undefined;
            
            try
            {
                result = await OnExecute();
            }
            catch (Exception e)
            {
                exception = e;
            }
            
            SetState(ETaskState.Finished);
            
            // _endTime = Time.realtimeSinceStartup;
            if (exception != null)
            {
                result = ETaskResult.FailureException;
                _failureException = exception;
            }
            else if (_cancelled)
            {
                result = ETaskResult.Cancelled;
            }
            
            SetResult(result);
            
            OnCompleted(result);
            
            ZenLog.Info(LogCategory.System, $"[ZenTask] -> [{GetTaskName()}]: Execution Finished with result: [{result}]");
         
            return result;
        }
        
        protected virtual void OnPrepare()
        {}
        
        protected virtual UniTask<ETaskResult> OnExecute()
        { 
            return UniTask.FromResult(ETaskResult.Failure);
        }
        
        protected virtual void OnCompleted(ETaskResult result)
        {}
        
        public virtual string GetTaskName()
        {
            return "Unnamed ZenTask";
        }
    }
    
    public interface IZenTaskManager
    {
        void Update();
        
        // TODO: Make them awaitables versions?
        bool RunModalTask(ZenTask task, Action<ZenTask> completitionCallback = null);
        ITaskHandle RunTask(ZenTask task, Action<ZenTask> completitionCallback = null);
        
        bool IsIdle();
    }
    
    public class ZenTaskManager : IZenTaskManager
    {
        class TaskTransaction
        {
            public ZenTask Task;
            public Action<ZenTask> CompletitionCallback;
        }
        
        // Modal Tasks
        private ZenTask _currentModalTask;
        
        private TaskTransaction _nextTransaction;
        
        private readonly Queue<TaskTransaction> _queuedTransactions = new ();
        
        private readonly HashSet<ZenTask> _runningTasks = new HashSet<ZenTask>();
        private readonly Dictionary<ZenTask,Action<ZenTask>> _taskCompletitionCallbacks = new Dictionary<ZenTask, Action<ZenTask>>();
        
        public void Update()
        {
            // Update Modal ZenTask
            if (_currentModalTask != null)
            {
                // Note: Modal tasks are not cancellable.
                if (_currentModalTask.State == ETaskState.Finished)
                {
                    _currentModalTask = null;
                }
            }
            else
            {
                if(_nextTransaction != null)
                {
                    ApplyModalTransaction(_nextTransaction);
                    _nextTransaction = null;
                }
            }
            
            // Update async tasks
            while (_queuedTransactions.Count > 0)
            {
                var transaction = _queuedTransactions.Dequeue();
                ApplyTransaction(transaction);
            }
        }

        private void ApplyModalTransaction(TaskTransaction transaction)
        {
            _currentModalTask = transaction.Task;
            
            ExecuteTask(transaction.Task, transaction.CompletitionCallback).Forget();
        }
        
        private void ApplyTransaction(TaskTransaction transaction)
        {
            ExecuteTask(transaction.Task, transaction.CompletitionCallback).Forget();
        }
        
        private async UniTask ExecuteTask(ZenTask task, Action<ZenTask> completitionCallback = null)
        {
            _runningTasks.Add(task);
            _taskCompletitionCallbacks.Add(task, completitionCallback);
            
            try
            {
                await task.Execute();
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[ZenTask] -> ExecuteTask: [Rare] ZenTask {task.GetTaskName()} failed with exception: {e}");
            }

            try
            {
                _taskCompletitionCallbacks[task]?.Invoke(task);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[ZenTask] -> CompletitionCallback: ZenTask {task.GetTaskName()} failed with exception: {e}");
            }

            _taskCompletitionCallbacks.Remove(task);
            _runningTasks.Remove(task); 
            
        }
        
        // Run a task that takes control of the application.
        // Modal tasks never timeout and should not be cancelled from the outside of the task.
        public bool RunModalTask(ZenTask task, Action<ZenTask> completitionCallback = null)
        {
            if(_currentModalTask != null || _nextTransaction != null)
            {
                ZenLog.Error(LogCategory.System,"[ZenTask] -> RunModalTask: Trying to run a modal task while another modal task is already running");
                return false;
            }
            
            if (task.State != ETaskState.Undefined)
            {
                ZenLog.Error(LogCategory.System,"[ZenTask] -> RunTask: Trying to run a task that is already running or finished");
                return false;
            }
            
            task.SetState(ETaskState.Enqueued);
            
            _nextTransaction = new TaskTransaction()
            {
                Task = task,
                CompletitionCallback = completitionCallback,
            };
            
            return true;
        }

        public ITaskHandle RunParallelTask(ZenTask task, Action<ZenTask> completitionCallback = null)
        {
            return RunTask(new ParallelTask(task), completitionCallback);
        }
        
        public ITaskHandle RunLambdaTask(Func<UniTask<ETaskResult>> function, Action<ZenTask> completitionCallback = null)
        {
            return RunTask(new LambdaTask(function), completitionCallback);
        }

        public UniTask RunTaskAsync(ZenTask task)
        {
            UniTaskCompletionSource<ZenTask> completionSource = new UniTaskCompletionSource<ZenTask>();
            
            RunTask(task, (ZenTask completedTask)=>
            {
                completionSource.TrySetResult(completedTask);
            });

            return completionSource.Task;
        }
        
        public ITaskHandle RunTask(ZenTask task, Action<ZenTask> completitionCallback = null)
        {
            if (task.State != ETaskState.Undefined)
            {
                ZenLog.Error(LogCategory.System,"[ZenTask] -> RunTask: Trying to run a task that is already running or finished");
                return null;
            }
            
            task.SetState(ETaskState.Enqueued);
            
            var handle = new TaskHandle(task);
            
            _queuedTransactions.Enqueue(new TaskTransaction()
            {
                Task = task,
                CompletitionCallback = completitionCallback,
            });
            
            return handle;
        }

        public bool IsIdle()
        {
            return _currentModalTask == null && _nextTransaction == null && 
                   _runningTasks.Count == 0 && _queuedTransactions.Count == 0;
        }
    }
}