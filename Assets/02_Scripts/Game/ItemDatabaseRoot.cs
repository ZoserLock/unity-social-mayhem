using System.Collections.Generic;
using UnityEngine;


namespace StrangeSpace
{
    public class ItemDatabaseRoot : MonoBehaviour
    {
        [Header("Lists")]
        [SerializeField]
        private List<ItemInfoListAsset> _itemInfoLists = new List<ItemInfoListAsset>();
    }
}