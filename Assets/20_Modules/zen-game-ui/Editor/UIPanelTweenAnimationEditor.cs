using UnityEngine;
using UnityEditor;

using System.Collections;
using StrangeSpace;

[CustomEditor(typeof(UIPanelTweenAnimation))]
public class UIPanelTweenAnimationEditor : Editor
{
    UIPanelTweenAnimation m_target = null;

    public void OnEnable()
    {
        m_target = (UIPanelTweenAnimation)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        m_target.ParentPanel = m_target.GetComponent<UIPanel>();
        
        if (m_target.ParentPanel != null && m_target.ParentPanel.MainPanel != null)
        {
            RectTransform rect = m_target.ParentPanel.MainPanel;

            if (rect != null)
            {
                m_target._editorSetBaseScale(rect.localScale);

                GUILayout.BeginVertical("HelpBox");
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Set As Show Position"))
                {
                    m_target._editorSetShowPosition(rect.anchoredPosition3D);
                }

                if (GUILayout.Button("Set As Hide Position"))
                {
                    m_target._editorSetHidePosition(rect.anchoredPosition3D);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Move To Show Position"))
                {
                    rect.anchoredPosition3D = m_target.ShowPosition;
                }

                if (GUILayout.Button("Move To Hide Position"))
                {
                    rect.anchoredPosition3D = m_target.HidePosition;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
        
    }

    public void OnSceneGUI()
    {
       /* if (m_target.ShowAnimationType == UIPanelTweenAnimationType.Translate || m_target.CloseAnimationType == UIPanelTweenAnimationType.Translate)
        {
            RectTransform rect = m_target.ParentPanel.MainPanel;

            Handles.color = Color.red;

            Vector3[] corners = new Vector3[4];

            rect.GetLocalCorners(corners);

             Vector3 hidePos = rect.TransformVector(rect.position + m_target.HidePosition);

            for (int a = 0; a < corners.Length; a++)
            {
                Handles.DrawAAPolyLine(hidePos, rect.position);
                Handles.DrawAAPolyLine(hidePos + rect.TransformVector(corners[0]), hidePos + rect.TransformVector(corners[1]), hidePos + rect.TransformVector(corners[2]),
                                       hidePos + rect.TransformVector(corners[3]), hidePos + rect.TransformVector(corners[0]));
            }
        }*/
    }


}
