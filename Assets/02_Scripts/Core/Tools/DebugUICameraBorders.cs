using UnityEngine;
using System.Collections;

namespace StrangeSpace
{
    [System.Serializable]
    public class DebugUICameraBorderDefinition
    {
        [SerializeField] 
        private Vector2 _aspect;

        [SerializeField] 
        private Color _color;
        
        // Get / Set
        public Vector2 Aspect => _aspect;
        public Color Color => _color;

        public DebugUICameraBorderDefinition(Vector2 aspect, Color color)
        {
            _aspect = aspect;
            _color = color;
        }
    }

    public class DebugUICameraBorders : MonoBehaviour
    {
        [SerializeField] 
        private DebugUICameraBorderDefinition[] _aspectList;

        [SerializeField] 
        private Camera _camera;

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (_camera != null)
            {
                if (_aspectList != null && _aspectList.Length != 0)
                {
                    float distance = -(transform.position.z);

                    Vector3 center = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distance));
                    Vector3 borderCurrent = _camera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, distance));
                    float width = _camera.pixelWidth;
                    float height = _camera.pixelHeight;

                    Vector3 borderScreen = _camera.ScreenToWorldPoint(new Vector3(width, height, distance));

                    for (int a = 0; a < _aspectList.Length; ++a)
                    {
                        float aspectWidth = borderScreen.y / (_aspectList[a].Aspect.y / _aspectList[a].Aspect.x);
                        Vector3 aspectBorder = new Vector3(aspectWidth, borderScreen.y, distance);
                        Gizmos.color = _aspectList[a].Color;
                        Gizmos.DrawWireCube(center, (aspectBorder) * 2.0f);
                    }

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(center, (borderCurrent) * 2.0f);
                }
            }
            else
            {
                _camera = GetComponent<Camera>();
            }
        }
#endif
    }
}
