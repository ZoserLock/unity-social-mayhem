using System;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    public class UIAreaLayer : MonoBehaviour, UILayer
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

        public void SetFocusedPanel(UIPanel panel)
        {
            if (_panels.Contains(panel))
            {
                if (_focusedPanel != null)
                {
                    _focusedPanel.Unfocus();
                    _focusedPanel = null;
                }
                
                _focusedPanel = panel;
                _focusedPanel.Focus();
            }
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
            
            InsertPanel(newPanel);
            
            // UIPanel is enabled but not the MainPanel inside.
            newPanel.gameObject.SetActive(true);
            
            // This is the old "Open"
            newPanel.AttachToLayer(this);
            
            var rectTransform = newPanel.gameObject.GetComponent<RectTransform>();
            // Set anchors to stretch both horizontally and vertically
            rectTransform.anchorMin = Vector2.zero;        // (0, 0)
            rectTransform.anchorMax = Vector2.one;         // (1, 1)

            // Reset offsets to zero so it fills completely
            rectTransform.offsetMin = Vector2.zero;        // Left and Bottom
            rectTransform.offsetMax = Vector2.zero;        // Right and Top
            
            UpdateGroupState();
            
            return newPanel as T;
        }
        
        public bool DetachPanel(UIPanel panel)
        {
            if (_panels.Remove(panel))
            { 
                if (_focusedPanel == panel)
                {
                    _focusedPanel = null;
                }
                
                panel.DetachFromLayer(this);
  
                UpdateGroupState();
                
                return true;
            }
            return false;
        }
        
        private void InsertPanel(UIPanel panel)
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
        
        private void UpdateGroupState()
        {
            if (_panels.Count > 0)
            {
                for (int a = 0; a < _panels.Count; ++a)
                {
                    _panels[a].SystemShow(true);
                }
            }
            else
            {
                _focusedPanel = null;
            }

        }
        
        public void Tick()
        {
            // In Area Layer all active panels are updated
            foreach (var panel in _panels)
            {
                if (panel.State == UIPanelState.Showed)
                {
                    panel.Tick();
                }
            }
        }
    }
}