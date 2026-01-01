using UnityEngine;
using UnityEngine.UI;

namespace StrangeSpace
{
    public class UIListPanel : UIPanel
    {
        [SerializeField]
        private Button _button;
        
        [SerializeField]
        private GameObject _targetObject;
        
        private UIOverlayMarker _marker;
        
        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);

            //_marker = UIOverlay.Instance.CreateMarker();
        }

        private void OnDestroy()
        {
        }

        private void HandleClick()
        {
            if (_targetObject == null)
                return;

            var rectTransform = _targetObject.GetComponent<RectTransform>();

            if (rectTransform == null)
                return;

            /*var uiCamera = _manager.UICamera;
            var overlayCamera = UIOverlay.Instance.Camera;


            var viewportPoint = uiCamera.WorldToViewportPoint(rectTransform.transform.position);

            var worldPoint = overlayCamera.ViewportToWorldPoint(viewportPoint);

            if (_marker != null)
            {
                _marker.transform.position = new Vector3(worldPoint.x, worldPoint.y, 0);
            }

            var canvas = _targetObject.GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = _targetObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UIOverlay";
            canvas.sortingOrder = 1;*/
        }
    }
}