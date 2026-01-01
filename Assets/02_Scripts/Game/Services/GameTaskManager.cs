using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using shortid;
using Utils;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public interface IGameTaskOwner
    {
        string Name { get; }
    }
    
    public class GameTaskData
    {
        public string Name { get; set; }
        public string SubName { get; set; }
        
        public long ExecutionTime { get; set; }
        public long ScheduledTime { get; set; }
        
    }
    
    public interface IGameTask
    {
        string TaskId { get; }
        string Name { get; }
        string SubName { get; }
        string FullName { get; }

        long ExecutionTime { get; }
        long ScheduledTime { get; }
        
        bool IsCompleted { get; }
        bool IsDiscarded { get; }
        
        // Task that spawned this task if was spawned as copy. Null for newly spawned Tasks.
        IGameTask Original { get; }
        
        object Owner { get; }
        Action<GameTask> Callback { get; }
        
        void Discard();
        
        float GetNormalizedTime();
        float GetPendingTimeSecs();

        void SetOriginal(IGameTask original);

        GameTaskData GetSaveData();
    }
    
    public class GameTask : IGameTask
    {
        private IGameTaskManager _manager;
        private string _taskId;
   
        private string _name;
        private string _subName;
        
        private long _executionTime;
        private long _scheduledTime;
        private long _taskTime;
        
        private bool _isCompleted;
        private bool _discarded;
        private int  _taskDepth;
        private object _owner;
        private string _ownerString;
        
        private Action<GameTask> _callback;
        private IGameTask _original;
        private readonly ITimeManager _timeManager;


        // Get / Set
        public string TaskId => _taskId;
        public string Name => _name;
        public string SubName => _subName;
        public string FullName => $"{_ownerString} - {_name}.{_subName} - [{_taskDepth}]";

        public long ExecutionTime => _executionTime;
        public long ScheduledTime => _scheduledTime;
        public long TaskTime => _taskTime;
        
        public bool IsCompleted => _isCompleted;
        public bool IsDiscarded => _discarded;
        
        public object Owner => _owner;
        public IGameTask Original => _original;
        public Action<GameTask> Callback => _callback;
        
        public GameTask(ITimeManager timeManager, object owner, string name, string subName, long executionTime,long scheduledTime, Action<GameTask> callback)
        {
            _timeManager = timeManager;
            
            _name = name;
            _subName = subName;
            _owner = owner;
            _ownerString = GetOwnerName();
            _taskId = $"{_ownerString}.{_name}.{_subName}.{ShortId.Generate()}";
            
            _executionTime = executionTime;
            _scheduledTime = scheduledTime;
    
            _taskTime = _executionTime - _scheduledTime;
            
            _isCompleted = false;
            _callback = callback;
        }

        private string GetOwnerName()
        {
            IGameTaskOwner owned = _owner as IGameTaskOwner;
            
            switch (_owner)
            {
                case IGameTaskOwner owner:
                    return owner.Name;
                default:
                    return _owner.GetHashCode().ToString("X"); ;
            }
        }

        public void Execute()
        {
            if (!_isCompleted)
            {
                _isCompleted = true;

                _callback?.Invoke(this);
            }
        }

        public void Discard()
        {
            if (!_isCompleted)
            {
                _isCompleted = true;
                _discarded = true;
            }
        }
        
        public float GetNormalizedTime()
        {
            if (_executionTime == _scheduledTime)
            {
                return 0;
            }

            var progress =_timeManager.CurrentTicks() - _scheduledTime;
            
            
            var totalTicks = _executionTime - _scheduledTime;
            
            
            var result = (float)(progress) / (float)(totalTicks);

            return result;
        }

        public float GetPendingTimeSecs()
        {
            if (_executionTime == _scheduledTime)
            {
                return 0;
            }
            
            var pendingTimeTicks = _executionTime - _timeManager.CurrentTicks();
            
            return (float)((double) pendingTimeTicks / (double)TimeSpan.TicksPerSecond);

        }

        public void SetOriginal(IGameTask original)
        {
            #if UNITY_EDITOR

                _taskDepth = 0;
                    
                if (original is GameTask gameTask)
                {
                    _taskDepth = gameTask._taskDepth + 1;  
                }
                
                _original = original;     
                
            #endif
        }

        public GameTaskData GetSaveData()
        {
            return new GameTaskData()
            {
                Name = _name,
                SubName = _subName,

                ExecutionTime = _executionTime,
                ScheduledTime = _scheduledTime,
            };
        }
    }
    
    public interface IGameTaskManager
    {
        public IGameTask ScheduleSeconds(object owner, string name, string subName, float secondsFromNow, Action<GameTask> callback);
        IGameTask Schedule(object owner, string name,string subName, long executionTime, long scheduleTime, Action<GameTask> callback);
        IGameTask ScheduleCopy(IGameTask task, string subName, long executionTime);
        IGameTask ScheduleCopySeconds(IGameTask task,string subName, float secondsFromNow);
        
        IGameTask ScheduleFromData(object owener, GameTaskData taskData, Action<IGameTask> callback);
        
        bool DiscardTask(string handleId);
        
        void Tick();
        void Clear();
        List<IGameTask> Tasks { get; }
    }

    public class GameTaskManager : IGameTaskManager
    {
        private readonly ITimeManager _timeManager;
        private readonly ActionQueue _actionQueue = new ActionQueue();
        
        private readonly Dictionary<string, GameTask> _idToTask = new();
        
        private readonly PriorityQueue<GameTask, long> _taskQueue = new();
        private readonly HashSet<GameTask> _pendingTasks = new();
        private readonly HashSet<GameTask> _finishedTasks = new();
        
        private readonly Queue<GameTask> _finishedTaskQueue = new();
        
        private Watch _tickWatch = new Watch();
        private Watch _catchUpWatch = new Watch();
        
        private bool _processingTasks;
        private bool _catchingUp;


        // Get / Set
        

        
        public List<IGameTask> Tasks 
        {
            get
            {
                var tasks = new List<IGameTask>();
                tasks.AddRange(_pendingTasks);
                tasks.AddRange(_finishedTasks);
                
                tasks.Sort((a, b) => a.ExecutionTime.CompareTo(b.ExecutionTime));
                
                return tasks;
            }
         
        }

        public GameTaskManager(ITimeManager timeManager)
        {
            _timeManager = timeManager;
        }
        
        public IGameTask ScheduleSeconds(object owner, string name, string subName, float secondsFromNow, Action<GameTask> callback)
        {
            long currentTicks = _timeManager.CurrentTicks();
            long ticksToAdd = (long)(secondsFromNow * TimeSpan.TicksPerSecond);
            long executionTime = currentTicks + ticksToAdd;
        
            return Schedule(owner, name, subName, executionTime, _timeManager.CurrentTicks(), callback);
        }

        public IGameTask Schedule(object owner, string name, string subName, long executionTime,long scheduleTime, Action<GameTask> callback)
        {
            if (owner is IGameTask task2)
            {
                Debug.LogError("sd");
            }
            
            var task = new GameTask(_timeManager,owner, name,subName, executionTime, scheduleTime, callback);
            
            _actionQueue.EnqueueAction(() =>
            {
                _idToTask.Add(task.TaskId, task);
                _taskQueue.Enqueue(task, task.ExecutionTime);
                _pendingTasks.Add(task);
            });
            
            return task;
        }
        
        public IGameTask ScheduleCopySeconds(IGameTask original, string subName, float secondsFromNow)
        {
            long currentTime = _timeManager.CurrentTicks();
            long ticksToAdd = (long)(secondsFromNow * TimeSpan.TicksPerSecond);
            long executionTime = currentTime + ticksToAdd;

            return ScheduleCopy(original, subName, executionTime);
        }

        public IGameTask ScheduleFromData(object owner, GameTaskData dataTask, Action<IGameTask> callback)
        {
            if (dataTask == null)
            {
                return null;
            }
            
            var task = Schedule(owner, dataTask.Name, dataTask.SubName, dataTask.ExecutionTime,dataTask.ScheduledTime, callback);
            return task;
        }

        public IGameTask ScheduleCopy(IGameTask original, string subName, long executionTime)
        {
            var task = Schedule(original.Owner, original.Name, subName, executionTime, _timeManager.CurrentTicks(), original.Callback);
            task.SetOriginal(original);
            return task;
        }
        
        public bool DiscardTask(string taskId)
        {
            if (_idToTask.TryGetValue(taskId, out var task))
            {
                if (_pendingTasks.Contains(task))
                {
                    task.Discard();
                    return true;
                }
            }
            return false;
        }

        public void Tick()
        {

            if (_timeManager.IsPaused)
            {
                return;
            }

            _tickWatch.Reset();

            var currentRealTime = _timeManager.CurrentTicks();

            if ((currentRealTime - _timeManager.ProcessingTime) > TimeSpan.TicksPerSecond)
            {
                ZenLog.Warning($"Catching up! {_timeManager.ProcessingTime / TimeSpan.TicksPerSecond} -> {currentRealTime / TimeSpan.TicksPerSecond} ");
                _catchingUp = true;
                _catchUpWatch.Reset();
            }
            else
            {
                if (_catchingUp)
                {
                    ZenLog.Warning($"Catch up took {_catchUpWatch.Time()} s");
                }

                _catchingUp = false;
            }


            _actionQueue.ProcessQueuedActions();

            _timeManager.BeginProcessing(currentRealTime);
            
            _processingTasks = true;
            
            var loopGuard = 500;
            var runAgain = true;
            
            while (runAgain && loopGuard > 0)
            {
                loopGuard--;
                runAgain = false;

                while (_taskQueue.Count > 0)
                {
                    var task = _taskQueue.Peek();

                    if (_timeManager.ProcessingTime < task.ExecutionTime)
                    {
                        if (currentRealTime - task.ExecutionTime > 0)
                        {
                            _timeManager.SetProcessingTime(task.ExecutionTime);
                        }
                        else
                        {
                            _timeManager.SetProcessingTime(currentRealTime);
                        }
                    }

                    if (task.ExecutionTime <= _timeManager.ProcessingTime)
                    {
                        _pendingTasks.Remove(task);
                        _taskQueue.Dequeue();

                        if (!task.IsDiscarded)
                        {
                            task.Execute();
                            _finishedTasks.Add(task);
                            _finishedTaskQueue.Enqueue(task);
                        }

                        runAgain = true;
                    }
                    else
                    {
                        break;
                    }
                }

                _actionQueue.ProcessQueuedActions();
            }
            
            // this was added after all the testing. check again
            // This was for a bug that did not update the time manager if no tasks.
            if (_taskQueue.Count == 0)
            {
                _timeManager.SetProcessingTime(currentRealTime);
            }

            _timeManager.EndProcessing();


            var time = _tickWatch.Time();

            if (time > 0.1f)
            {
                ZenLog.Warning($"[Game Task Manager] -> Tick over threshold. [{time}]");
            }
        }

        public void Clear()
        {
            _idToTask.Clear();
            _taskQueue.Clear(); 
            _actionQueue.Clear();
        }

    }
}