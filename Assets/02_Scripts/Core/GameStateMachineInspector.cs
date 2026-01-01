using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StrangeSpace
{
    public class GameStateMachineInspector : ZenEditorInspector
    {
        private GameStateMachine _gameStateMachine;
        private Vector2 _statesScrollPosition;
        private string _stateSearchFilter = "";
        private bool _showAllStates = true;
        private Type _selectedStateType;

        public GameStateMachineInspector(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public override void OnDrawGui()
        {
#if UNITY_EDITOR
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(5);
            DrawCurrentState();
            EditorGUILayout.Space(10);
            DrawStatesList();
            EditorGUILayout.Space(10);
            DrawSelectedStateDetails();
            EditorGUILayout.Space(10);
            DrawTransitionControls();
#endif
        }

#if UNITY_EDITOR
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Game State Machine Inspector", headerStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawCurrentState()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Current State", EditorStyles.boldLabel);

            if (_gameStateMachine.CurrentState != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Type:", _gameStateMachine.CurrentState.GetType().Name);

                // Check if we're transitioning
                if (_gameStateMachine.GetType().GetField("_isTransitioning",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) is bool isTransitioning && isTransitioning)
                {
                    EditorGUILayout.LabelField("Status:", "Transitioning", EditorStyles.boldLabel);

                    var originState = _gameStateMachine.GetType().GetField("_originState",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) as GameState;

                    var destinationState = _gameStateMachine.GetType().GetField("_destinationState",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) as GameState;

                    if (originState != null && destinationState != null)
                    {
                        EditorGUILayout.LabelField("Transition:", $"{originState.GetType().Name} → {destinationState.GetType().Name}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Status:", "Active", EditorStyles.boldLabel);
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("No active state", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatesList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Registered States", EditorStyles.boldLabel);

            var states = _gameStateMachine.GetType().GetField("_states",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) as Dictionary<Type, GameState>;

            if (states != null && states.Count > 0)
            {
                // Search and filter controls
                EditorGUILayout.BeginHorizontal();
                _stateSearchFilter = EditorGUILayout.TextField("Filter:", _stateSearchFilter);
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    _stateSearchFilter = "";
                }

                EditorGUILayout.EndHorizontal();

                _showAllStates = EditorGUILayout.Toggle("Show All States", _showAllStates);

                // List of states
                _statesScrollPosition = EditorGUILayout.BeginScrollView(_statesScrollPosition, GUILayout.Height(150));

                foreach (var stateEntry in states)
                {
                    Type stateType = stateEntry.Key;
                    GameState state = stateEntry.Value;

                    // Apply filter
                    if (!string.IsNullOrEmpty(_stateSearchFilter) &&
                        !stateType.Name.ToLower().Contains(_stateSearchFilter.ToLower()))
                    {
                        continue;
                    }

                    // Skip inactive states if not showing all
                    if (!_showAllStates && _gameStateMachine.CurrentState != state)
                    {
                        continue;
                    }

                    bool isCurrentState = _gameStateMachine.CurrentState == state;

                    EditorGUILayout.BeginHorizontal(isCurrentState ? EditorStyles.helpBox : EditorStyles.textField);

                    // Visual indicator for current state
                    if (isCurrentState)
                    {
                        EditorGUILayout.LabelField("►", GUILayout.Width(15));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(" ", GUILayout.Width(15));
                    }

                    // State name with button behavior
                    GUIStyle stateStyle = new GUIStyle(EditorStyles.label);
                    if (_selectedStateType == stateType)
                    {
                        stateStyle.fontStyle = FontStyle.Bold;
                    }

                    if (GUILayout.Button(stateType.Name, stateStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                    {
                        _selectedStateType = stateType;
                    }

                    // Add a transition button only for non-current states
                    if (!isCurrentState)
                    {
                        if (GUILayout.Button("Go", GUILayout.Width(40)))
                        {
                            var moveToStateMethod = typeof(GameStateMachine).GetMethod("MoveToState");
                            var genericMethod = moveToStateMethod.MakeGenericMethod(stateType);
                            genericMethod.Invoke(_gameStateMachine, new object[] { null });
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No states registered", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedStateDetails()
        {
            if (_selectedStateType == null)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var states = _gameStateMachine.GetType().GetField("_states",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) as Dictionary<Type, GameState>;

            if (states != null && states.TryGetValue(_selectedStateType, out GameState selectedState))
            {
                EditorGUILayout.LabelField($"State Details: {_selectedStateType.Name}", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                // State type information
                EditorGUILayout.LabelField("Type Information", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Namespace:", _selectedStateType.Namespace ?? "None");
                EditorGUILayout.LabelField("Base Type:", _selectedStateType.BaseType?.Name ?? "None");
                EditorGUILayout.LabelField("Interfaces:", string.Join(", ", _selectedStateType.GetInterfaces().Select(i => i.Name)));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);

                // Methods from reflection
                EditorGUILayout.LabelField("Overridden Methods", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var methods = _selectedStateType.GetMethods(System.Reflection.BindingFlags.Instance |
                                                            System.Reflection.BindingFlags.Public |
                                                            System.Reflection.BindingFlags.NonPublic |
                                                            System.Reflection.BindingFlags.DeclaredOnly);

                var baseStateMethods = new[] { "OnRegister", "OnUnregister", "OnEnter", "OnLeave", "OnUpdate" };

                foreach (var methodName in baseStateMethods)
                {
                    var method = methods.FirstOrDefault(m => m.Name == methodName);
                    bool isOverridden = method != null && method.DeclaringType == _selectedStateType;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(methodName + "():");
                    EditorGUILayout.LabelField(isOverridden ? "Overridden" : "Not Overridden",
                        isOverridden ? EditorStyles.boldLabel : EditorStyles.label);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;

                // Show state instance data if we want to expose any specific properties
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Instance Data", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                // Add custom state data inspection here based on state type
                // Example for specific state types:
                if (_selectedStateType.Name.Contains("MainMenu"))
                {
                    // Display MainMenu specific properties
                    EditorGUILayout.LabelField("State Type:", "Menu State");
                }
                else if (_selectedStateType.Name.Contains("Game"))
                {
                    // Display Game specific properties
                    EditorGUILayout.LabelField("State Type:", "Gameplay State");
                }
                else if (_selectedStateType.Name.Contains("Loading"))
                {
                    // Display Loading specific properties
                    EditorGUILayout.LabelField("State Type:", "Loading State");
                }
                else
                {
                    EditorGUILayout.LabelField("No specific data available for this state type");
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);

                // State status
                bool isCurrentState = _gameStateMachine.CurrentState == selectedState;
                EditorGUILayout.LabelField("Status:", isCurrentState ? "Active" : "Inactive",
                    isCurrentState ? EditorStyles.boldLabel : EditorStyles.label);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTransitionControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Pending Transactions", EditorStyles.boldLabel);

            var pendingTransactions = _gameStateMachine.GetType().GetField("_pendingTransactions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_gameStateMachine) as List<GameStateMachineTransaction>;

            if (pendingTransactions != null && pendingTransactions.Count > 0)
            {
                EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(100));

                foreach (var transaction in pendingTransactions)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (transaction.NextState != null)
                    {
                        EditorGUILayout.LabelField($"To: {transaction.NextState.GetType().Name}");

                        if (transaction.Data != null)
                        {
                            EditorGUILayout.LabelField($"Data: {transaction.Data.GetType().Name}");
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Data: None");
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No pending transactions", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }
#endif
    }
}