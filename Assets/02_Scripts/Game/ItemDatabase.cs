using System.Collections.Generic;
using StrangeSpace;
using UnityEngine;

namespace StrangeSpace
{
    public class ItemDatabase : PlainSingleton<ItemDatabase>
    {
        // Type sort order
        // private Dictionary<string,int> _orderMap = new();
 
        private ItemDatabaseRoot _root;
        private Dictionary<string, ItemInfo> _items = new();
        
        protected override void OnInitialize()
        {
            _root = Root.Get<ItemDatabaseRoot>();

            //var itemList = _root.ItemDefinitionLists;

        }

        protected override void OnDeinitialize()
        {
            _root = null;
        }
        
        public bool RegisterItemInfo(ItemInfo item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                return false;
            }
            
            return _items.TryAdd(item.Id, item);
        }
        
        public bool TryGetItemInfo(string id, out ItemInfo item)
        {
            return _items.TryGetValue(id, out item);
        }

        public List<ItemInfo> GetItemInfosByType(string type)
        {
            List<ItemInfo> items = new();

            foreach (var itemInfo in _items.Values)
            {
                items.Add(itemInfo);
            }

            return items;
        }

        public ItemQuantity CreateItemQuantity(string itemId, int amount)
        {
            if (!TryGetItemInfo(itemId, out ItemInfo itemInfo))
            {
                return null;
            }

            var itemQuantity = new ItemQuantity()
            {
                Item = itemInfo,
                Amount = amount
            };
            
            return null;
        }
        
        public ItemSet CreateItemSet(List<(string,int)> itemRequest)
        {
            var itemSet = new ItemSet(this);
            
            foreach (var tuple in itemRequest)
            {
                var itemId = tuple.Item1;
                var amount = tuple.Item2;
                
                itemSet.AddItemAmount(itemId, amount);
            }
            
            return itemSet;
        }
    }
}
