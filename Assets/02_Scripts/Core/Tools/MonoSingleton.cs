using UnityEngine;
using Zen.Debug;

namespace Zen.Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType(typeof(T)) as T;

                    if (_instance == null)
                    {
                        ZenLog.Error("Singleton: Can't find a gameobject of instance " + typeof(T) + "!");
                    }
                    else
                    {
                        _instance.Initialize();
                    }
                }

                return _instance;
            }
        }

        static public bool Exists
        {
            get
            {
                return _instance != null;
            }
        }

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                Initialize();
            }
        }

        private void Initialize()
        {
            _instance.OnAwake();

            if (!DestroyOnLoad())
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected void Start()
        {
            OnStart();
        }

        protected void OnApplicationQuit()
        {
            OnSingletonDestroy();
            _instance = null;
        }

        void OnDestroy()
        {
            OnSingletonDestroy();
            _instance = null;
        }

        protected virtual void OnAwake()
        {
            // Overridable
        }

        protected virtual void OnStart()
        {
            // Overridable
        }

        protected virtual void OnSingletonDestroy()
        {
            // Overridable
        }

        protected virtual bool DestroyOnLoad()
        {
            return true;
        }
    }
}