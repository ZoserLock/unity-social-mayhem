using System;
using System.Collections.Generic;

namespace Zen.Core
{
    public interface IBusMessage
    {}

    public interface IBusMessageSubscription
    {
        bool Unsubscribe();
    }

    public class BusMessageSubscription : IBusMessageSubscription
    {
        private readonly IMessageBus _messageBus;
        private readonly Type _type;
        private readonly Action<IBusMessage> _action;
        
        public BusMessageSubscription(IMessageBus messageBus, Type type, Action<IBusMessage> action)
        {
            _messageBus = messageBus;
            _type = type;
            _action = action;
        }
        
        public bool Unsubscribe()
        {
            return _messageBus.Unsubscribe(_type,_action);
        }
    }
    
    public class DisabledBusSubscription : IBusMessageSubscription
    {
        public bool Unsubscribe()
        {
            return true;
        }
    }
    
    public interface IMessageBus: ILifeTimeListener
    {
        void Reset();
        
        void Publish<TMessage>(TMessage message) where TMessage : class, IBusMessage;
        
        IBusMessageSubscription Subscribe<TMessage>(Action<IBusMessage> action) where TMessage: class, IBusMessage;
        bool Unsubscribe(Type type, Action<IBusMessage> action);
    }
 
    // Simple Message Bus
    public class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, List<Action<IBusMessage>>> _listeners = new Dictionary<Type, List<Action<IBusMessage>>>();
        
        private event Action<IBusMessage> OnMessageUnhandled;
        
        public IBusMessageSubscription Subscribe<TMessage>(Action<IBusMessage> action) where TMessage: class, IBusMessage
        {
            var type = typeof(TMessage);
            
            if (!_listeners.ContainsKey(type))
            {
                _listeners[type] = new List<Action<IBusMessage>>();
            }
            
            _listeners[type].Add(action);

            return new BusMessageSubscription(this,type,action);
        }

        public bool Unsubscribe(Type type, Action<IBusMessage> action)
        {
            if (!_listeners.ContainsKey(type))
            {
                return false;
            }

            var listeners = _listeners[type];

            return listeners.Remove(action);
        }

        public void Reset()
        {
            _listeners.Clear();
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class, IBusMessage
        {
            var listeners = GetListeners(typeof(TMessage));

            if (listeners.Count == 0)
            {
                OnMessageUnhandled?.Invoke(message);
                return;
            }
            
            foreach (var listener in listeners)
            {
                listener.Invoke(message);
            }
        }
        
        private List<Action<IBusMessage>> GetListeners(Type type)
        {
            if (!_listeners.ContainsKey(type))
            {
                _listeners[type] = new List<Action<IBusMessage>>();
            }
            
            return _listeners[type];
        }

        public void OnLifeEnd()
        {
            Reset();
        }
    }

    // This is a disabled bus that does nothing. Use to disable a bus without need to change code or make a null check
    public class DisabledMessageBus : IMessageBus
    {
        public void Reset()
        {
            // Do Nothing
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class, IBusMessage
        {
            // Do Nothing
        }
        
        public IBusMessageSubscription Subscribe<TMessage>(Action<IBusMessage> action) where TMessage : class, IBusMessage
        {
            return new DisabledBusSubscription();
        }

        public bool Unsubscribe(Type type, Action<IBusMessage> action)
        {
            return true;
        }

        public void OnLifeEnd()
        {
            Reset();
        }
    }
}