using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class GameCameraController : MonoBehaviour
{
    private Vector2 _position;
    private Vector3 _shakeVector;
    
    private float _currentDepth = 0;
    private float _targetDepth = 0;

    private Transform _transform;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float _minDepth = -10;

    [SerializeField]
    private float _maxDepth = -2;

    [SerializeField]
    private float _followSpeed = 5;

    // Camera physical Effects
    private float _shakePower = 1.0f;
    private float _shakeTime = 0.0f;

    private float _stompPower = 1.0f;
    private float _stompTime = 0.0f;
    private float _stompTimeMax = 1.0f;
    
    
    // Get / Set
    public Camera Camera => _camera;
    public float NormalizedCurrentDepth => (_maxDepth - _currentDepth) / (_maxDepth - _minDepth);
    
    protected void Awake()
    {
        _transform = transform;
        _position = _transform.position;
        _targetDepth = _transform.position.z;
        _currentDepth = _targetDepth;
    }

    void LateUpdate()
    {
        // TODO: Fix Later
        var mouse = Vector3.zero;

        /*if (_targetShip != null)
        {
            if (_targetShip.ControlLocked)
            {
                _position = Vector2.Lerp(_position, _targetShip.transform.position, Time.unscaledDeltaTime * _followSpeed);
            }
            else
            {
                float camDis = -_camera.transform.position.z;

                // TODO: Fix Later
                Vector2 mousePosition =  Vector3.zero;


               // Vector2 mousePosition = new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());

                Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, camDis));

                Vector3 mouseDir = mouseWorldPosition - _targetShip.transform.position;

                mouseDir = Vector3.ClampMagnitude(mouseDir, 11.0f);

                _position = Vector2.Lerp(_position, (_targetShip.transform.position + mouseDir * 0.5f), Time.unscaledDeltaTime * _followSpeed);
            }
        }*/

        if (Mathf.Abs(_currentDepth - _targetDepth) > 1.0f)
        {
            _currentDepth = Mathf.Lerp(_currentDepth, _targetDepth, Time.unscaledDeltaTime * 2.0f);
        }
        else
        {
            _currentDepth = Vector2.MoveTowards(new Vector2(0,_currentDepth), new Vector2(0,_targetDepth), 5 * Time.deltaTime).y;
        }


        // Limit the Z Position
        if (_currentDepth > _maxDepth || _currentDepth < _minDepth)
        {
            _currentDepth = Mathf.Clamp(_currentDepth, _minDepth, _maxDepth);
        }

        if (_shakeTime > 0)
        {
            _shakeTime -= Time.unscaledDeltaTime;
            if (_shakeTime < 0)
            {
                _shakeTime = 0.0f;
            }

            _shakeVector.x = Mathf.Abs(_shakePower * _shakeTime * Mathf.Sin(_shakeTime * 50.0f));
            _shakeVector.y = _shakePower * _shakeTime * Mathf.Cos(_shakeTime * 50.0f);
        }

        if (_stompTime > 0)
        {
            _stompTime -= Time.unscaledDeltaTime;
            if (_stompTime < 0)
            {
                _stompTime = 0.0f;
            }
            _shakeVector.x = 0;
            _shakeVector.y = _stompPower * Mathf.Sin((_stompTime / _stompTimeMax) * Mathf.PI);
        }

        _transform.position = new Vector3(_position.x, _position.y, _currentDepth) + _shakeVector;

    }

    public void Shake(float power, float time)
    {
        _shakePower = power;
        _shakeTime = time;

    }

    public void Stomp(float power, float time)
    {
        _stompPower = power;
        _stompTime = time;
        _stompTimeMax = time;
    }

    public Bounds GetCameraBounds(float planeZ = 0)
    {
        float distance = planeZ - (_transform.position.z);

        Vector3 center = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distance));
        Vector3 border = _camera.ViewportToWorldPoint(new Vector3(0, 0, distance));

        return new Bounds(center, (center - border) * 2.0f);
    }
    
    public Bounds GetCameraMaxBounds(float planeZ = 0)
    {
        float distance = planeZ - _minDepth;

        // Get the center point of the view
        Vector3 center = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distance));
    
        // Get the corners of the view
        Vector3 bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0, 0, distance));
        Vector3 topRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, distance));
    
        // Calculate width and height
        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;
    
        // Use the larger dimension to create a square
        float maxDimension = Mathf.Max(width, height);
    
        // Create a square bounds centered on the camera view center
        Vector3 size = new Vector3(maxDimension, maxDimension, 0.1f);
    
        return new Bounds(center, size);
    }
    
    public void ApplyZoom(float delta)
    {
        _targetDepth += -delta;
        // TODO: Avoid to be able to have target depth in boundaries positions.
        if (_targetDepth > _maxDepth || _targetDepth < _minDepth)
        {
            _targetDepth = Mathf.Clamp(_targetDepth, _minDepth, _maxDepth);
        }
    }

    public void InstantMove(Vector3 position)
    {
        _position = new Vector3(position.x, position.y);
        _transform.position = new Vector3(_position.x, _position.y, _currentDepth);
    }

void OnDrawGizmos()
{
    return;
#if UNITY_EDITOR
    if (_camera != null)
    {
        // Choose a common plane distance to visualize all bounds
        float planeZ = 0f; // Common XY plane at Z=0
        
        // Draw current camera view on the common plane
        Vector3 currentCenter = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, planeZ - transform.position.z));
        Vector3 currentBorder = _camera.ViewportToWorldPoint(new Vector3(0, 0, planeZ - transform.position.z));
        Vector3 currentSize = (currentCenter - currentBorder) * 2.0f;
        // Force Z position to be on our chosen plane
        currentCenter = new Vector3(currentCenter.x, currentCenter.y, planeZ);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(currentCenter, currentSize);
        
        // Draw closest possible camera view (at _maxDepth) on the common plane
        Vector3 closestCenter = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, planeZ - _maxDepth));
        Vector3 closestBorder = _camera.ViewportToWorldPoint(new Vector3(0, 0, planeZ - _maxDepth));
        Vector3 closestSize = (closestCenter - closestBorder) * 2.0f;
        // Force Z position to be on our chosen plane
        closestCenter = new Vector3(closestCenter.x, closestCenter.y, planeZ);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(closestCenter, closestSize);
        
        // Draw farthest possible camera view (at _minDepth) on the common plane
        Vector3 farthestCenter = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, planeZ - _minDepth));
        Vector3 farthestBorder = _camera.ViewportToWorldPoint(new Vector3(0, 0, planeZ - _minDepth));
        Vector3 farthestSize = (farthestCenter - farthestBorder) * 2.0f;
        // Force Z position to be on our chosen plane
        farthestCenter = new Vector3(farthestCenter.x, farthestCenter.y, planeZ);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(farthestCenter, farthestSize);
    }
#endif
}
}

