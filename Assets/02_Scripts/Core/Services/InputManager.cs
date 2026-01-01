using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Zen.Debug;

namespace StrangeSpace
{
    public enum DragStatus
    {
        Begin,
        Moving,
        End,
    }

    public enum HoldStatus
    {
        Begin,
        End,
    }

    public enum TouchStatus
    {
        Down,
        Hold,
        Up,
    }

    public enum ButtonStatus
    {
        Down,
        Up,
    }

    [System.Flags]
    public enum SwipeDirection
    {
        None = 0,
        Left = 0x1,
        Up = 0x2,
        Right = 0x4,
        Down = 0x8,

        Horizontal = Left | Right,
        Vertical = Up | Down,
        Full = Horizontal | Vertical
    }
    
    [System.Serializable]
    public class InputManagerSettings
    {
        public float TapMaxTime = 0.1f;
        public float SwipeMaxTime = 0.3f;
        public float DragDeadZone = 10;
        public float SwipeDeadZone = 15;
        public float DragAngleTreshold = 25;
        public float HoldTime = 2.0f;
        
        public bool SimulateTouchWithMouse = true;
        public bool ContinuousDragDetection = true;
    }
    
    public sealed class InputManager : PlainSingleton<InputManager>
    {
        private class TouchInfo
        {
            public int Id;
            public float BeginTime;
            public Vector3 BeginPosition;
            public bool Valid = true;
            public bool ProcessingHold = false;
        }

        private InputManagerRoot _inputManagerRoot;
        private InputManagerSettings _settings;
        
        private Dictionary<int, TouchInfo> _touchInfoList = new Dictionary<int, TouchInfo>(10);

        private TouchInfo _currentDragTouch;
        private Vector3 _lastMousePosition;

        private StaticObjectPool<TouchInfo> _infoPool = new StaticObjectPool<TouchInfo>(16);

        // Events
        // Touch Events
        public event Action<TouchStatus,Vector3> OnTouchEvent;
        public event Action<Vector3> OnTapEvent;
        public event Action<Vector3> OnLongTapEvent;
        public event Action<HoldStatus,Vector3> OnHoldEvent;
        public event Action<DragStatus,Vector3,Vector3> OnDragEvent;
        public event Action<SwipeDirection,Vector3> OnSwipeEvent;
        public event Action<float> OnPinchZoomEvent;
        
        // Mouse Events
        public event Action<int, Vector3> OnMouseButtonEvent;
        public event Action<Vector3,Vector3> OnMouseMoveEvent; 
        public event Action<Vector3,float> OnMouseScrollEvent; 
        
        // Get / Set
        public Vector3 MousePosition => Input.mousePosition;
        
        protected override void OnInitialize()
        {
            Root.OnUnityUpdate += Update;
            
            _inputManagerRoot = Root.Get<InputManagerRoot>();
            _settings = _inputManagerRoot.Settings;
        }
        
        protected override void OnDeinitialize()
        {
            Root.OnUnityUpdate -= Update;
        }
        
        private void Update()
        {
            // Tap / Drag / Swipe
            for (int a = 0; a < Input.touchCount; ++a)
            {
                Touch touch = Input.GetTouch(a);
                HandleTouch(touch.fingerId, touch.position, touch.deltaPosition, touch.phase);
            }

            // Pinch Zoom
            if (Input.touchCount == 2)
            {
                if (OnPinchZoomEvent != null)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    OnPinchZoomEvent(deltaMagnitudeDiff);
                }
            }

            // Mouse Events
            for (int a = 0; a < 5; ++a)
            {
                var overUI = IsPositionOverUIObject(Input.mousePosition);
                
                if (Input.GetMouseButtonDown(a))
                {
                    HandleMouseButton(a, Input.mousePosition, Input.mousePosition - _lastMousePosition, ButtonStatus.Down, overUI);
                }
                
                if (Input.GetMouseButtonUp(a))
                {
                    HandleMouseButton(a, Input.mousePosition, Input.mousePosition - _lastMousePosition, ButtonStatus.Up, overUI);
                }
            }
            
            var diff = Input.mousePosition - _lastMousePosition;

            if (diff.magnitude > 0.1f)
            {
                HandleMouseMove(Input.mousePosition, Input.mousePosition - _lastMousePosition);
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                HandleMouseScroll(Input.mousePosition, 1);
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                HandleMouseScroll(Input.mousePosition, -1);
            }
            
            // Mouse Touch Simulation
            #if UNITY_EDITOR
            if (_settings.SimulateTouchWithMouse)
            {
                if (Input.touchCount == 0)
                {
                    for (int a = 0; a < 3; ++a)
                    {
                        if (Input.GetMouseButtonDown(a))
                        {
                            HandleTouch(- (1 + a), Input.mousePosition, Input.mousePosition - _lastMousePosition, TouchPhase.Began);
                        }

                        if (Input.GetMouseButton(a))
                        {
                            HandleTouch(- (1 + a), Input.mousePosition, Input.mousePosition - _lastMousePosition, TouchPhase.Moved);
                        }

                        if (Input.GetMouseButtonUp(a))
                        {
                            HandleTouch(- (1 + a), Input.mousePosition, Input.mousePosition - _lastMousePosition, TouchPhase.Ended);
                        }
                    }
                    
                    if (OnPinchZoomEvent != null)
                    {
                        if (Input.GetAxis("Mouse ScrollWheel") > 0)
                        {
                            OnPinchZoomEvent(-1);
                        }

                        if (Input.GetAxis("Mouse ScrollWheel") < 0)
                        {
                            OnPinchZoomEvent(1);
                        }
                    }
                }
            }
            #endif
            
            _lastMousePosition = Input.mousePosition;
        }

        private void ProcessTouchHold(Vector3 touchPosition)
        {
            if (OnTouchEvent != null)
            {
                OnTouchEvent(TouchStatus.Hold, touchPosition);
            }
        }

        private void ProcessTouchDown(Vector3 touchPosition)
        {
            if (OnTouchEvent != null)
            {
                OnTouchEvent(TouchStatus.Down,touchPosition);
            }
        }

        private void ProcessTouchUp(Vector3 touchPosition)
        {
            if (OnTouchEvent != null)
            {
                OnTouchEvent(TouchStatus.Up, touchPosition);
            }
        }

        private void ProcessTap(Vector3 touchPosition)
        {
            if (OnTapEvent != null)
            {
                OnTapEvent(touchPosition);
            }
        }

        private void ProcessOnHold(HoldStatus status, Vector3 touchPosition)
        {
            if (OnHoldEvent != null)
            {
                OnHoldEvent(status, touchPosition);
            }
        }

        private void ProcessLongTap(Vector3 touchPosition)
        {
            if (OnLongTapEvent != null)
            {
                OnLongTapEvent(touchPosition);
            }
        }

        private void ProcessDrag(DragStatus status, Vector3 position, Vector3 last)
        {
            if (OnDragEvent != null)
            {
                OnDragEvent(status, position, last);
            }
        }

        private void ProcessSwipe(SwipeDirection direction, Vector3 realDirection)
        {
            if (OnSwipeEvent != null)
            {
                OnSwipeEvent(direction, realDirection);
            }
        }

        private void HandleTouch(int touchFingerId, Vector3 touchPosition, Vector3 delta, TouchPhase touchPhase)
        {
            if (!_touchInfoList.TryGetValue(touchFingerId, out var info))
            {
                info = _infoPool.GetInstance();
                _touchInfoList.Add(touchFingerId, info);
            }

            switch (touchPhase)
            {
                case TouchPhase.Began:

                    info.Id = touchFingerId;
                    info.BeginTime = Time.time;
                    info.BeginPosition = touchPosition;
                    info.Valid = true;
                    info.ProcessingHold = false;

                    if (!IsPositionOverUIObject(touchPosition))
                    {
                        ProcessTouchDown(touchPosition);
                    }
                    else
                    {
                        info.Valid = false;
                    }

                    break;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:

                    if (info.Valid)
                    {
                        ProcessTouchHold(touchPosition);
                    }

                    if (_currentDragTouch == null)
                    {
                        if (Vector3.Distance(touchPosition, info.BeginPosition) > _settings.DragDeadZone)
                        {
                            _currentDragTouch = info;
                            if (info.Valid)
                            {
                                ProcessDrag(DragStatus.Begin, touchPosition, delta);
                            }
                        }
                        else
                        {
                            if (info.Valid)
                            {
                                float elapsed = Time.time - info.BeginTime;
                                if (!info.ProcessingHold && elapsed > _settings.HoldTime)
                                {
                                    ProcessOnHold(HoldStatus.Begin, touchPosition);
                                    info.ProcessingHold = true;
                                }
                            }
                        }
                    }
                    else if (_currentDragTouch == info)
                    {
                        if (info.Valid)
                        {
                            if (_settings.ContinuousDragDetection || delta.magnitude > 0.1f)
                            {
                                ProcessDrag(DragStatus.Moving, touchPosition, delta);
                            }
                        }
                    }

                    break;
                case TouchPhase.Ended:

                    if (_currentDragTouch != null && _currentDragTouch == info)
                    {
                        if (info.Valid)
                        {
                            ProcessDrag(DragStatus.End, touchPosition, delta);
                        }
                        _currentDragTouch = null;
                    }

                    if (Vector3.Distance(touchPosition, info.BeginPosition) > _settings.SwipeDeadZone)
                    {
                        if ((Time.time - info.BeginTime) < _settings.SwipeMaxTime)
                        {
                            if (OnSwipeEvent != null)
                            {
                                SwipeDirection direction = GetSwipeDirection(touchPosition, info.BeginPosition);
                                if (info.Valid && direction != SwipeDirection.None)
                                {
                                    ProcessSwipe(direction, (touchPosition - info.BeginPosition).normalized);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (info.Valid && !IsPositionOverUIObject(touchPosition))
                        {
                            float elapsed = Time.time - info.BeginTime;
                            if (elapsed < _settings.TapMaxTime)
                            {
                                ProcessTap(touchPosition);
                            }
                            else
                            {
                                ProcessLongTap(touchPosition);
                            }
                        }
                    }

                    if (info.Valid)
                    {
                        if (info.ProcessingHold)
                        {
                            ProcessOnHold(HoldStatus.End, touchPosition);
                        }
                        ProcessTouchUp(touchPosition);
                    }

                    _infoPool.ReleaseInstance(info);
                    _touchInfoList.Remove(touchFingerId);

                    break;
                case TouchPhase.Canceled:
                    if (_currentDragTouch != null && _currentDragTouch == info)
                    {
                        if (info.Valid)
                        {
                            ProcessDrag(DragStatus.End, touchPosition, delta);
                        }
                        _currentDragTouch = null;
                    }

                    if (info.Valid)
                    {
                        if (info.ProcessingHold)
                        {
                            ProcessOnHold(HoldStatus.End, touchPosition);
                        }
                    }

                    _infoPool.ReleaseInstance(info);
                    _touchInfoList.Remove(touchFingerId);
                    break;
            }
        }

        private void HandleMouseButton(int buttonId,Vector3 position, Vector3 delta, ButtonStatus status, bool overUI )
        {
            OnMouseButtonEvent?.Invoke(buttonId, position);
            ZenLog.Info(LogCategory.System, $"[InputManager]: Button {buttonId} {status} at {position} over UI: {overUI}");
        }

        private void HandleMouseMove(Vector3 position, Vector3 delta)
        {
            OnMouseMoveEvent?.Invoke(position, delta);
           // ZenLog.Info(LogCategory.System, $"[InputManager]: Mouse Move at {position}");
        }
        
        private void HandleMouseScroll(Vector3 position, float delta)
        {
            OnMouseScrollEvent?.Invoke(position, delta);
        }

        // Check if the given angle is between 2 angles. 360 degress check.
        private bool IsAngleBetween(float angle, float minAngle, float maxAngle)
        {
            if (minAngle > maxAngle)
            {
                if (angle > minAngle)
                {
                    return true;
                }
                else if (angle < maxAngle)
                {
                    return true;
                }
            }
            return angle > minAngle && angle < maxAngle;
        }
        
        public SwipeDirection GetSwipeDirection(Vector3 currentTouchPosition, Vector3 beginPosition)
        {
            SwipeDirection direction = SwipeDirection.None;
            Vector3 to = currentTouchPosition - beginPosition;
            float angle = Vector2.Angle(Vector2.right, to);
            angle = Mathf.Sign(Vector3.Cross(Vector2.right, to).z) < 0 ? (360 - angle) % 360 : angle;

            if (IsAngleBetween(angle, 360 - _settings.DragAngleTreshold, _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Right;
            }
            else if (IsAngleBetween(angle, _settings.DragAngleTreshold, 90 - _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Right | SwipeDirection.Up;
            }
            else if (IsAngleBetween(angle, 90 - _settings.DragAngleTreshold, 90 + _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Up;
            }
            else if (IsAngleBetween(angle, 90 + _settings.DragAngleTreshold, 180 - _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Up | SwipeDirection.Left;
            }
            else if (IsAngleBetween(angle, 180 - _settings.DragAngleTreshold, 180 + _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Left;
            }
            else if (IsAngleBetween(angle, 180 + _settings.DragAngleTreshold, 270 - _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Left | SwipeDirection.Down;
            }
            else if (IsAngleBetween(angle, 270 - _settings.DragAngleTreshold, 270 + _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Down;
            }
            else if (IsAngleBetween(angle, 270 + _settings.DragAngleTreshold, 360 - _settings.DragAngleTreshold))
            {
                direction = SwipeDirection.Down | SwipeDirection.Right;
            }

            return direction;
        }
        
        private bool IsPositionOverUIObject(Vector3 position) 
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(position.x,position.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }


    }
}