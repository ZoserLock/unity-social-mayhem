using StrangeSpace;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace StrangeSpace
{
    public class GameDirectorInspector : ZenEditorInspector
    {
        private GameDirector _director;

        public GameDirectorInspector(GameDirector director)
        {
            _director = director;
        }

        public override void OnDrawGui()
        {
            #if UNITY_EDITOR
          
            #endif
        }
        
        #if UNITY_EDITOR
              
        #endif
    }
}