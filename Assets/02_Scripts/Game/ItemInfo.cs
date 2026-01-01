using UnityEngine;

namespace StrangeSpace
{
    public enum EItemType
    {
        Mineral = 0,
        
    }

    public enum ERarity
    {
        Common = 0,
    }
    
    [CreateAssetMenu(fileName = "ItemInfo", menuName = "Strange Space/ItemInfo")]
    public class ItemInfo : ScriptableObject
    {
        [SerializeField] 
        public string Id;

        [SerializeField] 
        public EItemType Type;
        
        [SerializeField] 
        public ERarity Rarity;
        
        [SerializeField] 
        public string Icon;
        
        [SerializeField] 
        public string Name;

        [SerializeField] 
        public string Description;
    }
}
