using System.Collections.Generic;

namespace Zen.Core
{
    // TODO: Add Tests
    public class DynamicObjectPool<T> where T : IObjectPoolable, new()
    {
        private readonly List<T> _allObjects;
        private readonly List<T> _usedObjects;
         
        private int _usedCount;
        private int _capacity;
        
        #if DEBUG
            private readonly HashSet<T> _allObjectSet;
        #endif
        
        // Get / Set
        public int Capacity => _capacity;
        public int FreeCount => _capacity - _usedCount;
        public int UsedCount => _usedCount;
        
        public DynamicObjectPool(int capacity)
        {
            _capacity = capacity;
            
            _allObjects = new List<T>(_capacity);
            _usedObjects = new List<T>(_capacity);
            
            #if DEBUG
                _allObjectSet = new HashSet<T>(_capacity);
            #endif
            
            for (var i = 0; i < _capacity; i++)
            {
                AddNewObject();
            }
        }
        
        private void AddNewObject()
        {
            var obj = new T();
            
            _allObjects.Add(obj);

            #if DEBUG
                _allObjectSet.Add(obj);
            #endif
        }
        
        public T GetInstance()
        {
            if (_usedCount == _allObjects.Count)
            {
                AddNewObject();
                _capacity++;
            }

            if (_usedCount < _allObjects.Count)
            {
                var obj = _allObjects[(int)_usedCount];
                _usedCount++;
                
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

            _usedCount--;
            _usedObjects.Remove(obj);
        }
    }
}