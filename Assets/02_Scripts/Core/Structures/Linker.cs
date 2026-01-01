using System;
using System.Collections.Generic;


namespace StrangeSpace
{
    public class Linker
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _typeStorage = new Dictionary<Type, Dictionary<string, object>>();

        public void Add<T>(string id, T item)
        {
            var type = typeof(T);

            if (!_typeStorage.TryGetValue(type, out var typeDict))
            {
                typeDict = new Dictionary<string, object>();
                _typeStorage[type] = typeDict;
            }

            typeDict[id] = item;
        }

        public T Get<T>(string id)
        {
            var type = typeof(T);

            if (!_typeStorage.TryGetValue(type, out var typeDict))
            {
                throw new KeyNotFoundException($"No objects of type {type.Name} have been registered");
            }

            if (!typeDict.TryGetValue(id, out var item))
            {
                throw new KeyNotFoundException($"No object with id {id} found for type {type.Name}");
            }

            return (T)item;
        }

        public bool TryGet<T>(string id, out T result)
        {
            var type = typeof(T);
            result = default;

            if (!_typeStorage.TryGetValue(type, out var typeDict))
            {
                return false;
            }

            if (!typeDict.TryGetValue(id, out var item))
            {
                return false;
            }

            result = (T)item;
            return true;
        }

        public bool Contains<T>(string id)
        {
            var type = typeof(T);

            return _typeStorage.TryGetValue(type, out var typeDict) && typeDict.ContainsKey(id);
        }

        public bool Remove<T>(string id)
        {
            var type = typeof(T);

            if (!_typeStorage.TryGetValue(type, out var typeDict))
            {
                return false;
            }

            return typeDict.Remove(id);
        }

        public void Clear<T>()
        {
            var type = typeof(T);

            if (_typeStorage.TryGetValue(type, out var typeDict))
            {
                typeDict.Clear();
            }
        }

        public void ClearAll()
        {
            _typeStorage.Clear();
        }
    }
}