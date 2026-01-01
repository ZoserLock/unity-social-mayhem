using UnityEngine;
using System.Collections;

namespace StrangeSpace
{
    public class UIWorldItem : MonoBehaviour
    {
        private Camera _worldCamera;
        private Camera _uiCamera;

        private bool _autoUpdatePosition = true;
        private Vector3 _position;

        public bool AutoUpdatePosition
        {
            get { return _autoUpdatePosition; }
            set { _autoUpdatePosition = value; }
        }

        public void SetWorldCamera(Camera worldCamera)
        {
            _worldCamera = worldCamera;
        }

        public void SetUICamera(Camera uiCamera)
        {
            _uiCamera = uiCamera;
        }

        public void SetPosition(Vector3 position)
        {
            _position = position;
            UpdatePosition();
        }

        protected virtual void Update()
        {
            if (_autoUpdatePosition)
            {
                UpdatePosition();
            }
        }

        public void UpdatePosition()
        {
            var viewportPos = _worldCamera.WorldToViewportPoint(_position);
            transform.position = _uiCamera.ViewportToWorldPoint(viewportPos);
        }
    }
}
