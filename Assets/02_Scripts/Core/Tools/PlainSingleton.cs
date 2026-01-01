using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public abstract class PlainSingleton<T> where T : PlainSingleton<T>, new()
    {
        // Static Variables
        private static T _instance = null;
        
        // static Get / Set
        public static T Instance => _instance;
        public static bool IsInitialized => _instance != null;
        
        // Instance Variables
        private SingletonRoot _root;
        
        // Get / Set
        protected SingletonRoot Root => _root;
        
        public static T Create()
        {
            _instance = new T();

            return _instance;
        }

        public static void BindToRoot(SingletonRoot root)
        {
            _instance.SetRoot(root);
        }

        public static void Initialize()
        {
            if (_instance == null)
            {
                ZenLog.Error("Singleton: Can't initialize a gameobject of instance " + typeof(T) + "!");
                return;
            }
            
            _instance.OnInitialize();
        }

        public static void Deinitialize()
        {
            _instance.OnDeinitialize();
            _instance.SetRoot(null);
            
            _instance = null;
        }

        private void SetRoot(SingletonRoot root)
        {
            _root = root;
        }
        
        protected virtual void OnInitialize()
        {
            // Overridable
        }

        protected virtual void OnDeinitialize()
        {
            // Overridable
        }
    }
}