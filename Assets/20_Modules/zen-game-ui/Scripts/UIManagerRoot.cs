using System;
using UnityEngine;

namespace StrangeSpace
{
    public class UIManagerRoot : MonoBehaviour
    {
        [SerializeField]
        public Camera CanvasCamera = null;
        
        [SerializeField]
        public RectTransform CanvasRoot = null;
        
        [SerializeField]
        public UIFullOverlay FullOverlayObject = null;   
        
        [Header("Utility Objects")]
        [SerializeField]
        public UIBlockPanel BlockingPanelObject = null;

        [SerializeField]
        public UIPanelPool PanelPool = null;
        
        [Header("Properties")]
        [SerializeField]
        public UIManagerConfiguration Configuration = null;

        [Header("Layers")]
        [SerializeField]
        private UIAreaLayer _areaLayer;
        
        [SerializeField]
        private UIStackLayer _popupLayer;
        
        public UIAreaLayer AreaLayer => _areaLayer;
        public UIStackLayer PopupLayer => _popupLayer;
        
        private void Update()
        {
            _areaLayer.Tick();
            _popupLayer.Tick();
        }
    }
}