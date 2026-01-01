using System.Collections.Generic;

namespace Zen.Core
{
    public interface IObjectPoolable
    {
        void OnPoolCreate(){}
        void OnPoolDestroy(){}
    }
    
    // TODO: Add Tests
    public class StaticObjectPool<T> where T : IObjectPoolable, new()
    {
        private int _capacity;
        private readonly List<T> _freeObjects;
        private readonly List<T> _usedObjects;
         
        #if DEBUG
            private readonly HashSet<T> _allObjectSet;
        #endif
        
        // Get / Set
        public int Capacity => _capacity;
        public int FreeCount => _freeObjects.Count;
        public int UsedCount => _usedObjects.Count;
        
        public StaticObjectPool(int size)
        {
            _capacity = size;
            
            _freeObjects = new List<T>(_capacity);
            _usedObjects = new List<T>(_capacity);
            
            #if DEBUG
                _allObjectSet = new HashSet<T>(_capacity);
            #endif
            
            for (var i = 0; i < size; i++)
            {
                var obj = new T();
                
                _freeObjects.Add(obj);
                
                #if DEBUG
                    _allObjectSet.Add(obj);
                #endif
            }
        }
        
        public T GetInstance()
        {
            if (_freeObjects.Count > 0)
            {
                var obj = _freeObjects[_freeObjects.Count - 1];
                _freeObjects.RemoveAt(_freeObjects.Count - 1);
                _usedObjects.Add(obj);
                
                obj.OnPoolCreate();
                return obj;
            }
            
            return default;
        }
        
        public void ReturnInstance(T obj)
        {
            #if DEBUG
                if(!_allObjectSet.Contains(obj))
                {
                    throw new System.Exception("Trying to return an object that was not allocated by this pool");
                }
            #endif
            
            obj.OnPoolDestroy();
            
            _usedObjects.Remove(obj);
            _freeObjects.Add(obj);
        }
    }
}