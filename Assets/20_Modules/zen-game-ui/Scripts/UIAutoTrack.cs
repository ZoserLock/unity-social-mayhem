using System;
using UnityEngine;
using System.Collections;
using Zen.Debug;

namespace StrangeSpace
{
    public class UIAutoTrack : MonoBehaviour
    {
        [SerializeField]
        private string _name;

        private UIPanel _currentPanel;
        
        private bool _isBeingTracked = false;
        
        private void OnEnable()
        {
            if (_currentPanel ==null)
            {
                _currentPanel = GetComponentInParent<UIPanel>();

                if (_currentPanel == null)
                {
                    ZenLog.Info($"UIAutoTrack [{_name}] Can't find UIPanel in parent");
                }
            }

            if (_currentPanel != null && !string.IsNullOrEmpty(_name))
            {
                var fullRoute = _currentPanel.GetFullRoute(_name);

                if (TrackerService.IsInitialized)
                {
                    TrackerService.Instance.RegisterObject(fullRoute, gameObject);
                }
               
                // Track
                _isBeingTracked = true;
            }
        }

        private void OnDisable()
        {
            if (_isBeingTracked)
            {
                if (TrackerService.IsInitialized)
                {
                    TrackerService.Instance.UnregisterObject(gameObject);
                }

                // Untrack
                _isBeingTracked = false; 
            }
        }
    }
}
