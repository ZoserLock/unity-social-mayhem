using System;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    public class UIStackLayer : MonoBehaviour, UILayer
    {
        private UIManager _uiManager;
        private UIPanelPool _panelPool;
        private Dictionary<Type, UIPanel> _panelDatabase;
        
        private List<UIPanel> _panels = new List<UIPanel>();
        private UIPanel _focusedPanel = null;
        
        public void Initialize(UIManager uiManager, UIPanelPool pool, Dictionary<Type, UIPanel> database)
        {
            _uiManager = uiManager;
            _panelPool = pool;
            _panelDatabase = database;
        }

        public T AttachPanel<T>(Action<UIPanel> prepare = null) where T : UIPanel
        {
            if (!_panelDatabase.TryGetValue(typeof(T), out UIPanel panelModel))
            {
                Debug.LogError($"[UILayer] Panel {typeof(T)} does not exist in database");
                return null;
            }

            UIPanel newPanel = _panelPool.GetInstance(panelModel);
            if (newPanel == null)
            {
                Debug.LogWarning($"[UILayer] Panel {typeof(T)} exists but pool returned null");
                return null;
            }
            
            newPanel.Initialize(this, prepare);
            
            InsertPopupPanel(newPanel);
            
            // UIPanel is enabled but not the MainPanel inside.
            newPanel.gameObject.SetActive(true);
            
            // Now this panel is part of this layer.
            newPanel.AttachToLayer(this);
            
            var rectTransform = newPanel.gameObject.GetComponent<RectTransform>();
            // Set anchors to stretch both horizontally and vertically
            rectTransform.anchorMin = Vector2.zero;        // (0, 0)
            rectTransform.anchorMax = Vector2.one;         // (1, 1)

            // Reset offsets to zero so it fills completely
            rectTransform.offsetMin = Vector2.zero;        // Left and Bottom
            rectTransform.offsetMax = Vector2.zero;        // Right and Top

            UpdatePanelProperties();
            
            return newPanel as T;
        }
        
        public bool DetachPanel(UIPanel panel)
        {
            if (_panels.Remove(panel))
            { 
                UpdatePanelProperties();
                
                panel.DetachFromLayer(this);
                
                return true;
            }
            return false;
        }
        
        private void InsertPopupPanel(UIPanel panel)
        {
            for (int a = 0; a < _panels.Count; a++)
            {
                if ((int)_panels[a].Priority <= (int)panel.Priority)
                {
                    continue;
                }
                _panels.Insert(a, panel);
                panel.transform.SetParent(transform, false);
                panel.transform.SetSiblingIndex(a);
                return;
            }

            _panels.Add(panel);
            panel.transform.SetParent(transform, false);
            panel.transform.SetSiblingIndex(_panels.Count);
        }
        
        private void UpdatePanelProperties()
        {
            if (_panels.Count > 0)
            {
                UIPanel lastFocused = _focusedPanel;
                _focusedPanel = _panels[_panels.Count - 1];

                if (lastFocused != _focusedPanel)
                {
                    UpdateBlockingPanel();
                    
                    if ((_focusedPanel.Flags & UIPanelFlags.HideOthers) == UIPanelFlags.HideOthers)
                    {
                        SystemShowPopupsFromTop();
                    }
                    else
                    {
                        SystemShowAllPopups();
                    }
                }
            }
            else
            {
                _focusedPanel = null;
                //_blockingPanel.transform.SetParent(_canvasTransform, false);
                //_blockingPanel.Setup(false, false);
            }

        }
        
        private void UpdateBlockingPanel()
        {
            /* bool disableOverlay = ((_popupOnTop.Flags & UIPanelFlags.DisableDarkBackground) == UIPanelFlags.DisableDarkBackground);
             bool allowUIInput = ((_popupOnTop.Flags & UIPanelFlags.AllowUIInput) == UIPanelFlags.AllowUIInput);

             _blockingPanel.transform.SetParent(_popupOnTop.transform, false);
             _blockingPanel.transform.SetSiblingIndex(0);
             _blockingPanel.Setup(!allowUIInput, !disableOverlay);*/
        }
        
        private void SystemShowPopupsFromTop(int showCount = 1)
        {
            for (int a = 0; a < _panels.Count; ++a)
            {
                _panels[a].SystemShow(a >= (_panels.Count - showCount));
            }
        }

        private void SystemShowAllPopups()
        {
            for (int a = 0; a < _panels.Count; ++a)
            {
                _panels[a].SystemShow(true);
            }
        }
        
        public void Tick()
        {
            if (_focusedPanel != null)
            {
                _focusedPanel.Tick();
            }
        }
    }
}