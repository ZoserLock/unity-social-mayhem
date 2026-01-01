
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    // This a class to move dependencies in editor time in unity. 
    public static class EditorDependencies
    {
        public static List<IEditorInspectorProvider> Inspectors = new List<IEditorInspectorProvider>();


        public static void Clear()
        {
            GameTaskManager = null;
            TimeManager = null;
            Inspectors.Clear();
        }
        
        public static void AddInspectorProvider(IEditorInspectorProvider inspector)
        {
            if (inspector == null)
                return;
            
            Inspectors.Add(inspector);
        }
        
        
        public static ZenApplication Application { get; set; }
        public static IZenTaskManager ZenTaskManager { get; set; }
        public static IGameTaskManager GameTaskManager { get; set; }
        public static ITimeManager TimeManager { get; set; }
    }
}
