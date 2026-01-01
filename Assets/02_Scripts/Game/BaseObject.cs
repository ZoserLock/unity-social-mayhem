using UnityEngine;

namespace StrangeSpace
{
    public interface IBaseObjectData
    {
        public string Id { get; }
        public string Type { get; }
    }
    
    public interface IBaseObject
    {
        public string Id { get; }
        
        public void ReconcileLinks(Linker linker);
    }
}
