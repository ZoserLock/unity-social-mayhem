using System;
using UnityEngine;
using System.Collections;
using System.Text;
using shortid;
using Zen.Debug;
using Object = UnityEngine.Object;

namespace StrangeSpace
{
    [System.Flags]
    public enum UIPanelFlags
    {
        None = 0x0,
        DarkBackground        = 0x1,
        DarkBackgroundInput   = 0x2,
        HideOthers            = 0x4,
    }

    public enum UIPanelPriority
    {
        Lowest = 0,
        Low = 1,
        BelowMedium = 2,
        Medium = 3,
        AboveMedium = 4,
        High = 5,
        Hightest = 6,
    }

    public enum UIPanelState
    {
        Hidden = 0, // The panel is hidden
        Hiding = 1, // The panel is transitioning to hidden
        Showed = 2, // The panel is shown
        Showing = 3 // The panel is transitioning to Shown
    }

    public enum UIPanelEventType
    {
        Attach,
        Detach,
        Show,
        Hide,
        Occlude,
        Reveal
    }

    public class UIPanelEvent
    {
        public UIPanelEventType Type { get; set; }
        public string Name { get; set; }
    }
    
    public class UIPanel : MonoBehaviour
    {
        [Header("UIPanel")] 
        [SerializeField] private RectTransform _mainPanel;
        
        [SerializeField] private string _panelName;
        
        [Header("UIPanel - Optional")] 
        [SerializeField] private UIPanelAnimation _animation;
        
        protected UILayer _layer;

        private UIPanelPool _pool;

        private string _panelTag;
        private string _panelId;
        private string _panelPath;
        
        private UIPanelFlags _flags;
        private UIPanelPriority _priority;
        private UIPanelState _state = UIPanelState.Hidden;

        private bool _attached = false;
        private bool _disposed = false;

        private bool _updatingShowStatus = false;

        private bool _shownByUser = true;
        private bool _shownBySystem = true;

        private bool _needUpdatePanelPath = true;
        // Events
        public event System.Action<UIPanel> OnAttachEvent;
        public event System.Action<UIPanel> OnDetachEvent;

        public event System.Action<UIPanel> OnShowEvent;
        public event System.Action<UIPanel> OnHideEvent;

        // Get / Set
        public string Id => _panelId;
        public UILayer Layer => _layer;
        public string PanelName => string.IsNullOrEmpty(_panelName) ? "unnamed" : _panelName;
        public string Tag => _panelTag;

        public UIPanelPriority Priority => _priority;
        public UIPanelFlags Flags => _flags;
        public UIPanelState State => _state;
        public RectTransform MainPanel => _mainPanel;

        // Function called when a panel is created from a pool. This is just called 1 time even if the panel is reused.
        internal void PoolCreate(UIPanelPool pool)
        {
            _pool = pool;

            if (_mainPanel == null)
            {
                ZenLog.Error(LogCategory.GUI, "_mainPanel is null ABORT! You need to define a main panel");
            }
        }
        
        protected virtual UIPanelPriority GetDefaultPriority()
        {
            return UIPanelPriority.Medium;
        }

        protected virtual UIPanelFlags GetDefaultFlags()
        {
            return UIPanelFlags.None;
        }
        
        // Function called befor opening the panel.
        internal void Initialize(UILayer layer, Action<UIPanel> prepare)
        {
            _layer = layer;
    
            _flags = GetDefaultFlags();
            _priority = GetDefaultPriority();
            _panelId = $"panel_{ShortId.Generate()}";

            _disposed = false;
            _shownByUser = false;
            _shownBySystem = true;
            
            // Disable Main Panel when starting always.
            _mainPanel.gameObject.SetActive(false);

            prepare?.Invoke(this);
        }

        private void Deinitialize()
        {
            _layer = null;
        }
        
        // Function To flag a panel with a tag in a builder way
        public void SetTag(string panelTag)
        {
            _panelTag = panelTag;
        }
        
        public void SetName(string panelName)
        {
            _panelName = panelName;
        }
        
        public void SetPriority(UIPanelPriority panelPriority)
        {
            _priority = panelPriority;
        }
        
        public void SetFlags(UIPanelFlags panelFlags)
        {
            _flags = panelFlags;
        }

        // Function called when a panel is created. This begin the life of a panel.
        internal void AttachToLayer(UILayer layer)
        {
            if (!_attached)
            {
                _attached = true;
                
                OnAttachToLayer(layer);
                OnAttachEvent?.Invoke(this);

                PublishEvent(UIPanelEventType.Attach);
            }
            else
            {
                ZenLog.Error(LogCategory.GUI, "Trying to attach an already Attached UIPanel");
            }
        }

        // Function to return this panel to the pool. This ends the life of a panel.
        internal void DetachFromLayer(UILayer layer)
        {
            if (_state != UIPanelState.Hidden)
            {
                _disposed = true;
                UpdateShowStatus();
            }
            else
            {
                if (_attached)
                {
                    _attached = false;
                    OnDetachFromLayer();

                    OnDetachEvent?.Invoke(this);

                    PublishEvent(UIPanelEventType.Detach);
                    
                    Deinitialize();
                    _pool.ReleaseInstance(this);
                }
                else
                {
                    ZenLog.Error(LogCategory.GUI, "Trying to detach an already Dettached UIPanel");
                }
            }
        }

        // Function called only by the UIManager To tell this panel to stay shown or hidden. The system has authority
        internal void SystemShow(bool show)
        {
            _shownBySystem = show;
            UpdateShowStatus();
        }

        public void Show()
        {
            _shownByUser = true;
            UpdateShowStatus();
        }

        private void InternalShow()
        {
            if (_state == UIPanelState.Hidden)
            {
                _mainPanel.gameObject.SetActive(true);
                OnShowStart();
                _state = UIPanelState.Showing;

                if (_animation == null)
                {
                    InternalShowEnd();
                }
                else
                {
                    _animation.AnimateShow(InternalShowEnd);
                }
            }
            else
            {
                ZenLog.Error(LogCategory.GUI, "Trying to call InternalShow in a bad state");
            }
        }

        private void InternalShowEnd()
        {
            _state = UIPanelState.Showed;
            OnShowEnd();

            OnShowEvent?.Invoke(this);

            PublishEvent(UIPanelEventType.Show);
        }

        public void Hide()
        {
            _shownByUser = false;
            UpdateShowStatus();
        }

        private void InternalHide()
        {
            if (_state == UIPanelState.Showed)
            {
                OnHideStart();
                _state = UIPanelState.Hiding;

                if (_animation == null)
                {
                    InternalHideEnd();
                }
                else
                {
                    _animation.AnimateHide(InternalHideEnd);
                }
            }
            else
            {
                ZenLog.Error(LogCategory.GUI, "Trying to call InternalHide in a bad state");
            }
        }

        // Called when this panel enter focus. in popup is the current top popup. In Area is the one manually marked
        public void Focus()
        {
            
        }
        
        // Remove the focus of this panel.
        public void Unfocus()
        {
            
        }
        
        private void InternalHideEnd()
        {
            _state = UIPanelState.Hidden;
            OnHideEnd();
            _mainPanel.gameObject.SetActive(false);

            OnHideEvent?.Invoke(this);

            PublishEvent(UIPanelEventType.Hide);

            if (_disposed)
            {
                DetachFromLayer(_layer);
            }
        }


        internal void Tick()
        {
            OnTick();
        }
        
        private void UpdateShowStatus()
        {
            if (!_updatingShowStatus)
            {
                StartCoroutine(UpdateShowStatusRoutine());
            }
        }

        private IEnumerator UpdateShowStatusRoutine()
        {
            _updatingShowStatus = true;
            
            while (!CheckShowStatus())
            {
                yield return null;
            }

            _updatingShowStatus = false;
        }

        // 2025: this seems to wait to a state be showed of hidden to continue. 
        // TODO: Set another name
        private bool CheckShowStatus()
        {
            bool shouldBeShown = _shownByUser && _shownBySystem && !_disposed;

            if (_state == UIPanelState.Showed)
            {
                if (!shouldBeShown)
                {
                    InternalHide();
                }

                return true;
            }
            else if (_state == UIPanelState.Hidden)
            {
                if (shouldBeShown)
                {
                    InternalShow();
                }

                return true;
            }

            return false;
        }

        // This destroy the panel returning to the pool.
        public void DetachSelf()
        {
            if (_layer != null)
            {
                _layer.DetachPanel(this);
            }
        }
        
        // Called every frame when the panel is in a visible ste
        protected virtual void OnTick()
        {
            // Overridable
        }

        // Called when the show just starts.
        protected virtual void OnShowStart()
        {
            // Overridable
        }

        // Called when the show ends.
        protected virtual void OnShowEnd()
        {
            // Overridable
        }

        // Called when the hide just starts.
        protected virtual void OnHideStart()
        {
            // Overridable
        }

        // Called when whe hide ends.
        protected virtual void OnHideEnd()
        {
            // Overridable
        }

        // Called when the panel is instantiated from the pool and added to the layer
        protected virtual void OnAttachToLayer(UILayer layer)
        {
            // Overridable
        }

        // Called when the panel is destroyed and returned to the pool and removed from the layer.
        protected virtual void OnDetachFromLayer()
        {
            // Overridable
        }
        
        // TODO: Name correctly
        // called when the panel is not the "focused" panel inside the layer
        protected virtual void OnOccluded()
        {
            // Overridable
        }

        // Called when the panel is the main or focused panel of the layer. In a popup layer is the top popup
        protected virtual void OnRevealed()
        {
            // Overridable
        }
        
        // Event functions
        private void PublishEvent(UIPanelEventType eventType)
        {
            if (!string.IsNullOrEmpty(_panelName))
            {
                var panelEvent = new UIPanelEvent()
                {
                    Type = eventType,
                    Name = _panelName
                };

                var eventMessage = GetEventMessage(eventType);

                // TODO: FIX THIS
                // _layer.PublishEvent(eventMessage, panelEvent);
            }
        }

        private string GetEventMessage(UIPanelEventType eventType)
        {
            switch (eventType)
            {
                case UIPanelEventType.Attach:
                    return "Panel Opened";
                case UIPanelEventType.Detach:
                    return "Panel Closed";
                case UIPanelEventType.Hide:
                    return "Panel Hidden";
                case UIPanelEventType.Show:
                    return "Panel Show";
                default:
                    return "Panel Unknown";
            }
        }
        // Experimental
        public string GetFullRoute(string element = null)
        {
            if (_needUpdatePanelPath)
            {
                var parent = transform.parent;

                var builder = new StringBuilder();

                while (parent != null)
                {
                    var panel = parent.GetComponent<UIPanel>();

                    if (panel != null)
                    {
                        builder.Append("/" + panel.PanelName);
                    }

                    parent = parent.parent;
                }

                _panelPath = builder.ToString() + "/" + PanelName;
            }

            if (string.IsNullOrEmpty(element))
            {
                return _panelPath;
            }

            return _panelPath + "/" + element;
        }
    }
}