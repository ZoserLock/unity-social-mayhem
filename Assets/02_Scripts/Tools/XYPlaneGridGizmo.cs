using UnityEngine;

namespace StrangeSpace
{
    public class XYPlaneGridGizmo : MonoBehaviour
    {
        [Header("Grid Settings")] [SerializeField]
        private float _cellSize = 1f;

        [SerializeField] 
        private int _gridSize = 10;
        
        [SerializeField] 
        private Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField]
        private bool _highlightAxis = true;
        [SerializeField] private Color _xAxisColor = Color.red;
        [SerializeField] private Color _yAxisColor = Color.green;

        private void OnDrawGizmos()
        {
            var originalColor = Gizmos.color;

            var halfWidth = _gridSize * _cellSize * 0.5f;
            var halfHeight = _gridSize * _cellSize * 0.5f;

            for (int y = 0; y <= _gridSize; y++)
            {
                var yPos = y * _cellSize - halfHeight;

                if (_highlightAxis && Mathf.Approximately(yPos, 0f))
                {
                    Gizmos.color = _yAxisColor;
                }
                else
                { 
                    Gizmos.color = _gridColor;
                }

                var startPoint = new Vector3(-halfWidth, yPos, 0f);
                var endPoint = new Vector3(halfWidth, yPos, 0f);
                Gizmos.DrawLine(startPoint, endPoint);
            }

            for (int x = 0; x <= _gridSize; x++)
            {
                var xPos = x * _cellSize - halfWidth;

                if (_highlightAxis && Mathf.Approximately(xPos, 0f))
                {
                    Gizmos.color = _xAxisColor;
                }
                else
                {
                    Gizmos.color = _gridColor;
                }

                var startPoint = new Vector3(xPos, -halfHeight, 0f);
                var endPoint = new Vector3(xPos, halfHeight, 0f);
                Gizmos.DrawLine(startPoint, endPoint);
            }

            Gizmos.color = originalColor;
        }
    }
}