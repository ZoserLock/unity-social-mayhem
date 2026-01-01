using System;
using System.Collections.Generic;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public interface IEventService
    {
        event Action<EventService.Event> OnEvent; 
        
        void RegisteEvent(EventService.EventType type, string message, object data = null);
    }
    
    public class EventService : IEventService, IEditorInspectorProvider
    {
        public enum EventType
        {
            Global = 0,
            GameLifeCycle = 1,
            GameData = 2,
            UI = 3,
        }
        
        public class Event
        {
            public string Message;
            public EventType Type;
            public object Data;
        }
        
        private readonly List<Event> _events = new List<Event>();
        
        private readonly bool _printToLog = true;
        
        public event Action<Event> OnEvent;
        
        // Get / Set
        public List<Event> Events => _events;
        
        public void RegisteEvent(EventType type, string message, object data)
        {
            var evt = new Event
            {
                Type = type,
                Message = message,
                Data = data
            };
            
            _events.Add(evt);
            
            OnEvent?.Invoke(evt);

            if (_printToLog)
            {
                // Log the event
                ZenLog.Info(LogCategory.Gameplay, $"[EventService]: Event: {evt.Type} - {evt.Message}");
            }
        }
        
        public InspectorInfo GetInspectorInfo()
        {
            return new InspectorInfo
            {
                Name = "Event Service",
                Description = "Manages game events and logging",
            };
        }

        public IEditorInspector GetInspector()
        {
            #if UNITY_EDITOR
                return new EventServiceInspector(this);
            #endif
            return null;
        }

    }
}