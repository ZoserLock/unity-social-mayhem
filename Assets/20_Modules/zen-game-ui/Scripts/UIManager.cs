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
    public interface UILayer
    {
        public bool DetachPanel(UIPanel panel);
    }
    
    public sealed class UIManager : PlainSingleton<UIManager>
    {
        private UIManagerRoot _root;
        private UIPanelPool _panelPool;
        
        private IEventService _eventService;
        private TrackerService _trackerService;

        private Dictionary<Type, UIPanel> _panelDatabase = new Dictionary<Type, UIPanel>();

        // Get / Set
        public Camera CanvasCamera => _root.CanvasCamera;
        
        public void SetDependencies(IEventService eventService, TrackerService trackerService)
        {
            _eventService = eventService;
            _trackerService = trackerService;
        }

        protected override void OnInitialize()
        {
            _root = Root.Get<UIManagerRoot>();
            
            _root.FullOverlayObject.Hide();
            
            _panelPool = _root.PanelPool;
            
            // Generate list of available UIPanels.
            PreloadPanelList();
            
            // Initialize Layers
            _root.AreaLayer.Initialize(this, _panelPool, _panelDatabase);
            _root.PopupLayer.Initialize(this, _panelPool, _panelDatabase);
        }

        private void PreloadPanelList()
        {
            for (var a = 0; a < _root.Configuration.PanelLists.Length; ++a)
            {
                var panelList = _root.Configuration.PanelLists[a].Panels;

                for (int b = 0; b < panelList.Length; ++b)
                {
                    var panel = panelList[b];

                    if (panel == null)
                    {
                        ZenLog.Error($"Panel is null in list {a} at index {b}");
                        continue;
                    }
                    PreloadPanel(panel);
                }
            }
        }

        private void PreloadPanel(UIPanel panel)
        {
            Type panelType = panel.GetType();
            if (!_panelDatabase.ContainsKey(panelType))
            {
                _panelDatabase.Add(panelType, panel);
                _panelPool.PreloadInstance(panel);
            }
            else
            {
                ZenLog.Warning("Trying to load a panel twice: " + panelType);
            }
        }
        
        // POPUPS
        public T AttachPopup<T>(Action<UIPanel> prepare = null) where T : UIPanel
        {
            return _root.PopupLayer.AttachPanel<T>(prepare);
        }
        
        public T AttachAreaPanel<T>(Action<UIPanel> prepare = null) where T : UIPanel
        {
            return _root.AreaLayer.AttachPanel<T>(prepare);
        }

        public void EnableFullOverlay(bool enable)
        {
            if (enable)
            {
                _root.FullOverlayObject.Show();
            }
            else
            {
                _root.FullOverlayObject.Hide();
            }
        }
    }
}
