using System;
using UnityEngine;

namespace StrangeSpace
{
    public interface IEditorPing
    {
        public void OnEditorPing();
    }

    public struct InspectorInfo
    {
        public string Name;
        public string Description;
        public int Priority;
        public Color Color;
    }
    
    public interface IEditorInspectorProvider
    {
        InspectorInfo GetInspectorInfo();
        IEditorInspector GetInspector();
    }
    
    public interface IEditorInspector
    {
        void OnGui();
    }
    
    public class ZenEditorInspector : IEditorInspector
    {
        private ActionQueue _actionQueue = new();
        
        public void OnGui()
        {
            var currentEvent = Event.current;
            
            if (currentEvent.type == EventType.Layout)
            {
                _actionQueue.ProcessQueuedActions();
            }
            
            OnDrawGui();
        }

        public void RunAction(Action action)
        {
            _actionQueue.EnqueueAction(action);
        }
        
        public virtual void OnDrawGui()
        {
            // Overridable
        }
    }
}