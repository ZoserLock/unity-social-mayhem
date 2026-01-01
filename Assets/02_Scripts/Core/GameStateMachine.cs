using System;
using System.Collections.Generic;
using System.Numerics;
using System.Transactions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public class GameStateMachineTransaction
    {
        private readonly GameState _nextState;
        private readonly object _data;
        // Get / Set
        public GameState NextState => _nextState;
        public object Data => _data;

        public GameStateMachineTransaction(GameState nextState, object data)
        {
            _nextState = nextState;
            _data = data;
        }
    }

    public class GameStateMachineTransactionError
    {
        public GameStateMachineTransaction Transaction;
        public string ErrorMessage;
    }

    public class GameStateMachine
    {
        private ZenApplication _application;
        
        private Dictionary<Type,GameState> _states = new Dictionary<Type, GameState>();
        
        private List<GameStateMachineTransaction> _pendingTransactions = new List<GameStateMachineTransaction>();
        
        private GameState _currentState;
        
        // Running transaction
        private GameState _originState;
        private GameState _destinationState;
        private bool _isTransitioning;
        
        // Get / Set
        public GameState CurrentState => _currentState;
        
        public GameStateMachine(ZenApplication application)
        {
            RuntimeAssert.IsNotNull(application);
            _application = application;
        }
        
        public void RegisterState<T>(T state) where T : GameState
        {
            RuntimeAssert.IsFalse(_states.ContainsKey(typeof(T)), $"State {typeof(T)} is already registered");
            
            _states.Add(typeof(T), state);
            state.Register(this);
        }
        
        public void UnregisterState<T>() where T : GameState
        {
            var state = GetState<T>();
            
            RuntimeAssert.IsNotNull(state, $"State {typeof(T)} not found");
            
            state.Unregister();
            _states.Remove(typeof(T));
        }

        private GameState GetState<T>() where T : GameState
        {
            RuntimeAssert.IsTrue(_states.ContainsKey(typeof(T)), $"State {typeof(T)} not found");
            
            return _states[typeof(T)];
        }

        public void MoveToState<T>(object data = null) where T : GameState
        {
            var state = GetState<T>();
            
            RuntimeAssert.IsNotNull(state, $"State {typeof(T)} not found");
            
            _pendingTransactions.Add(new GameStateMachineTransaction(state, data));
        }
        
        public void Update(float deltaTime)
        {
            ApplyPendingTransactions().Forget();
            
            if (_currentState != null && !_isTransitioning)
            {
                _currentState.Update(deltaTime);
            }
        }
        
        private async UniTask ApplyPendingTransactions()
        {
            if(_isTransitioning)
            {
                return;
            }
            
            _isTransitioning = true;
            
            var pendingTransations = new List<GameStateMachineTransaction>(_pendingTransactions);
            _pendingTransactions.Clear();
            
            foreach (var transaction in pendingTransations)
            {
                _originState = _currentState;
                _destinationState = transaction.NextState;
                
                if (_currentState != null)
                {
                    await _currentState.Leave();
                }

                _currentState = null;
                
                _currentState = transaction.NextState;
                
                await _currentState.Enter(transaction.Data);
            }
            
            _isTransitioning = false;
        }
    }
    
    public class GameState
    {
        private GameStateMachine _stateMachine;
        private ZenScheduler _scheduler = new ZenScheduler();
        private List<IScheduleHandle> _scheduleHandles = new List<IScheduleHandle>();
        
        public void Register(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            
            OnRegister();
        }

        public void Unregister()
        {
            OnUnregister();
            
            _stateMachine = null;
        }

        public UniTask Enter(object data)
        {
            return OnEnter(data);
        }

        public UniTask Leave()
        {
            foreach (var handle in _scheduleHandles)
            {
                handle.Cancel();
            }
            _scheduleHandles.Clear();
            
            return OnLeave();
        }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
            
            _scheduler.Tick();
        }
        
        public void MoveToState<T>(object data = null) where T : GameState
        {
            _stateMachine.MoveToState<T>(data);
        }
        
        public void EnqueueAction(Action action)
        {
      
        }

        public IScheduleHandle Schedule(TimeSpan timeSpan, Action action,int priority = 0)
        {
            var handle = _scheduler.Schedule(timeSpan, action, priority);

            return handle;
        }
        
        public void AutoSchedule(TimeSpan timeSpan, Action action,int priority = 0)
        {
            var handle = _scheduler.Schedule(timeSpan, action, priority);
            
            _scheduleHandles.Add(handle);
        }
        
        // Virtual Functions
        protected virtual void OnRegister()
        { }
        
        protected virtual void OnUnregister()
        { }

        protected virtual UniTask OnEnter(object data)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnLeave()
        {
            return UniTask.CompletedTask;
        }
        
        protected virtual void OnUpdate(float deltaTime)
        { }
    }
}