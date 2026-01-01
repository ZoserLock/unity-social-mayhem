using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

using UnityEngine;

namespace StrangeSpace
{
    public class GamePlayStateInspector : ZenEditorInspector
    {
        private GameplayState _gameplayState;
        private SaveManagerSettings _settings;
        private Vector2 _jsonScrollPosition;
        
        private string _currentJsonData;
        
        private bool _showJson = true;
        private bool _showCurrentData = true;
        private bool _showDebugInfo = true;
        private bool _showSaveSlots = true;
        
        public GamePlayStateInspector(GameplayState gameplayState)
        {
            _gameplayState = gameplayState;
        }

        public override void OnDrawGui()
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);
            
            // DrawCurrentSaveSlot();
            EditorGUILayout.Space(10);
            
            if (_showCurrentData = EditorGUILayout.Foldout(_showCurrentData, "Current Save Data", true))
            {
                //DrawCurrentSaveData();
            }
            EditorGUILayout.Space(10);
#endif
        }

#if UNITY_EDITOR
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Gameplay State Inspector", headerStyle);

            EditorGUILayout.EndVertical();
        }
#endif
    }
}