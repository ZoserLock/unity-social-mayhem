using System;
using UnityEngine;


namespace StrangeSpace
{
    public class SingletonRoot : MonoBehaviour
    {
        public Action OnUnityUpdate;
        
        private void Update()
        {
            OnUnityUpdate?.Invoke();
        }
        
        public T Get<T>() where T : MonoBehaviour
        {
            return GetComponentInChildren<T>(true);
        }
    }
}