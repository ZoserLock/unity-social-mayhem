using UnityEngine;
using UnityEditor;

namespace StrangeSpace
{


    public class StrangePlaneViewEditor : EditorWindow
    {
        private float _marginSize = 1.0f;
        private bool _includeSelection = true;

        [MenuItem("Strange/Strange Plane View")]
        public static void ShowWindow()
        {
            GetWindow<StrangePlaneViewEditor>("Strange Plane View");
        }

        private void OnGUI()
        {
            GUILayout.Label("Strange Plane View Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Set XY View"))
            {
                _marginSize = EditorGUILayout.FloatField("Margin Size (units)", _marginSize);
                _includeSelection = EditorGUILayout.Toggle("Focus on Selection", _includeSelection);
                SetXYView();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Access", EditorStyles.boldLabel);

            if (GUILayout.Button("XY View (Default Settings)"))
            {
                _marginSize = 1.0f;
                _includeSelection = true;
                SetXYView();
            }
        }

        private void SetXYView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogError("No active scene view found!");
                return;
            }

            sceneView.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            Bounds bounds;

            if (_includeSelection && Selection.activeGameObject != null)
            {
                bounds = CalculateSelectionBounds();

                if (bounds.size == Vector3.zero)
                {
                    bounds = new Bounds(Selection.activeGameObject.transform.position, Vector3.one);
                }
            }
            else
            {
                bounds = CalculateSceneBounds();

                if (bounds.size == Vector3.zero)
                {
                    bounds = new Bounds(Vector3.zero, Vector3.one * 10);
                }
            }

            bounds.Expand(_marginSize * 2);

            var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
            var orthographicSize = maxDimension * 0.5f;

            sceneView.orthographic = true;
            sceneView.size = orthographicSize;

            var cameraPosition = bounds.center;
            cameraPosition.z = sceneView.pivot.z - 10f;
            sceneView.pivot = bounds.center;
            sceneView.Repaint();

            Debug.Log($"Set XY view with margin: {_marginSize} units");
        }

        private Bounds CalculateSelectionBounds()
        {
            var bounds = new Bounds();
            var boundsInitialized = false;

            foreach (var obj in Selection.gameObjects)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>();

                foreach (var renderer in renderers)
                {
                    if (!boundsInitialized)
                    {
                        bounds = renderer.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                var rectTransforms = obj.GetComponentsInChildren<RectTransform>();
                foreach (var rectTransform in rectTransforms)
                {
                    var corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);

                    foreach (var corner in corners)
                    {
                        if (!boundsInitialized)
                        {
                            bounds = new Bounds(corner, Vector3.zero);
                            boundsInitialized = true;
                        }
                        else
                        {
                            bounds.Encapsulate(corner);
                        }
                    }
                }
            }

            return bounds;
        }

        private Bounds CalculateSceneBounds()
        {
            var bounds = new Bounds();
            var boundsInitialized = false;

            var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            foreach (var renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            var rectTransforms = GameObject.FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
            foreach (var rectTransform in rectTransforms)
            {
                var corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                foreach (var corner in corners)
                {
                    if (!boundsInitialized)
                    {
                        bounds = new Bounds(corner, Vector3.zero);
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(corner);
                    }
                }
            }

            return bounds;
        }
    }
}