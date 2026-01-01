using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using StrangeSpace;
using UnityEngine.Serialization;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public sealed class UIOverlay : PlainSingleton<UIOverlay>
    {
        private UIOverlayRoot _root = null;

        private IEventService _eventService;
        private TrackerService _trackerService;

        // Get / Set
        public Camera Camera => _root.Camera;

        protected override void OnInitialize()
        {
            _root = Root.Get<UIOverlayRoot>();
        }
        
        public void SetDependencies(IEventService eventService, TrackerService trackerService)
        {
            _eventService = eventService;
            _trackerService = trackerService;
        }

        public void PublishEvent(string message, UIPanelEvent panelEvent)
        {

        }
    }
}
