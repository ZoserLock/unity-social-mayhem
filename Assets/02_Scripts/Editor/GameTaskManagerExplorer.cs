using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace StrangeSpace
{
    public class GameTaskManagerExplorer : EditorWindow
    {
        private List<IGameTask> _allTasks = new List<IGameTask>();
        private List<IGameTask> _filteredTasks = new List<IGameTask>();

        private static Color _level0Color = new Color(1.0f, 0.3f, 0.1f);
        private static Color _level1Color = new Color(0.8f, 0.8f, 0.2f);
        private static Color _level2Color = new Color(0.4f, 0.7f, 0.1f);
        
        private readonly ActionQueue _actionQueue = new ActionQueue();
        
        private string _filterText = "";
        private Vector2 _listScrollPosition;
        private Vector2 _detailsScrollPosition;
        private IGameTask _selectedTask;

        [MenuItem("Strange/Explorer/GameTaskManager")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameTaskManagerExplorer>();
            window.titleContent = new GUIContent("Game Task Manager");
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnEditorUpdate()
        {   
            Repaint();
        }
        
        
        
        private void OnGUI()
        {
            if(EditorDependencies.GameTaskManager == null)
            {
                EditorGUILayout.LabelField("GameTaskManager not found");
                return;
            }

            if (EditorDependencies.TimeManager == null)
            {
                EditorGUILayout.LabelField("TimeManager not found");
                return;
            }
            
      
            
            _actionQueue.ProcessQueuedActions();
            
            EditorGUILayout.BeginVertical();

            DrawTopBar();
            


            EditorGUILayout.BeginHorizontal();

            DrawTaskList();
            DrawTaskDetails();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTopBar()
        {
            var timeManager = EditorDependencies.TimeManager;

            var currentTimeString = timeManager.CurrentTimeString();
            var currentTime = timeManager.CurrentTime();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{currentTimeString}]", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{currentTime.Seconds:F2} s");
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Pause"))
            {
                timeManager.TogglePause();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("0.5 s >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(500);
                });
            }
            if (GUILayout.Button("1 s >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(1*1000);
                });
            }
            if (GUILayout.Button("10 s >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(10*1000);
                });
            }
            if (GUILayout.Button("30 s >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(30*1000);
                });
            }
            if (GUILayout.Button("1 m >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(60*1000);
                });
            }
            if (GUILayout.Button("10 m >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(10*60*1000);
                });
            }
            if (GUILayout.Button("1 h >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(60*60*1000);
                });
            }
            if (GUILayout.Button("12 h >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(12*60*60*1000);
                });
            }
            if (GUILayout.Button("1 d >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(24*60*60*1000);
                });
            }
            if (GUILayout.Button("1 w >"))
            {
                _actionQueue.EnqueueAction(() =>
                {
                    timeManager.AddTimeMs(7*24*60*60*1000);
                });
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTaskList()
        {
            var tasks = EditorDependencies.GameTaskManager.Tasks;
            
            EditorGUILayout.BeginVertical(GUILayout.Width(400));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Task List", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            string newFilterText = EditorGUILayout.TextField("Filter:", _filterText);
            if (newFilterText != _filterText)
            {
                _filterText = newFilterText;
                //FilterTasks();
            }

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                {
                    continue;
                }
                
                GUILayout.BeginHorizontal();
                
                var isSelected = task == _selectedTask;
                
          
                var buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.alignment = TextAnchor.MiddleLeft;

                var old = GUI.color;
                GUI.color = GetColorForTime(task.GetPendingTimeSecs());
                
                var newSelected = GUILayout.Button($"{task.FullName}", buttonStyle);
                
                GUI.color = old;
                
                GUILayout.Label($"{task.GetPendingTimeSecs():F2} s",GUILayout.Width(50));

                GUILayout.EndHorizontal();
                
                if (newSelected != isSelected && newSelected)
                {
                    _actionQueue.EnqueueAction(() =>
                    {
                        _selectedTask = task;
                    });
                }
                
                
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private Color GetColorForTime(float seconds)
        {
            if (seconds < 5)
            {
                return _level0Color;
            }

            if (seconds < 15)
            {
                return _level1Color;
            }
            
            if (seconds < 30)
            {
                return _level0Color;
            }

            return GUI.color;
        }

        private void DrawTaskDetails()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2 - 5));
      
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Task Details", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _detailsScrollPosition = EditorGUILayout.BeginScrollView(_detailsScrollPosition);

            if (_selectedTask != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Ping Owner"))
                {
                    _actionQueue.EnqueueAction(() =>
                    {
                        var pingable = _selectedTask.Owner as IEditorPing;
                        pingable.OnEditorPing();
                    });
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                int allowedIterations = 100;
                int count = 1;
                var currentParent = _selectedTask.Original;
                while (currentParent != null)
                {
                    if (GUILayout.Button("["+count+"]"))
                    {
                        // Select that task.
                    }
                    
                    currentParent = currentParent.Original;

                    count++;
                    allowedIterations--;
                    if (allowedIterations<0)
                    {
                        break;
                    }
                    
                }
                if (GUILayout.Button("Ping Owner"))
                {
                    _actionQueue.EnqueueAction(() =>
                    {
                        var pingable = _selectedTask.Owner as IEditorPing;
                        pingable.OnEditorPing();
                    });
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("Name:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_selectedTask.TaskId);
            }
            else
            {
                EditorGUILayout.LabelField("Select a task to view details", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}