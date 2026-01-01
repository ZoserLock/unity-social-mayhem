using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class ItemQuantity
    {
        public ItemInfo Item { get; set; }
        public int Amount { get; set; }
        
        public void AddAmount(int amount)
        {
            Amount += amount;
        }
        
        public override string ToString()
        {
            return $"{Item.Name} x{Amount}";
        }
    }
    
    public class ItemSet
    {
        private readonly ItemDatabase _itemDatabase;
        private Dictionary<string, ItemQuantity> _items = new ();
        
        // Get / Set
        public List<ItemQuantity> Items => _items.Values.ToList();
        
        public ItemSet(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }
        
        public bool HasItemAmount(string item, int amount)
        {
            if (_items.TryGetValue(item, out var itemQuantity))
            {
                return itemQuantity.Amount >= amount;
            }

            return false;
        }

        public void AddItemAmount(string itemId, int amount)
        {
            if (!_items.ContainsKey(itemId))
            {
                // Get Item Info From Database
                if (!_itemDatabase.TryGetItemInfo(itemId, out var itemInfo))
                {
                    ZenLog.Error($"Trying to Add an invalid item ID: {itemId}");
                    return;
                }
                
                _items[itemId] = new ItemQuantity()
                {
                    Item = itemInfo,
                    Amount = amount
                };

            }
            else
            {
                _items[itemId].Amount += amount;
            }
        }

        public bool RemoveItemAmount(string itemId, int amount, bool allowIncomplete = false)
        {
            if (!_items.ContainsKey(itemId))
            {
                return false;
            }

            if (allowIncomplete)
            {
                return _items[itemId].Amount >= 0;
            }
            
            if (amount < _items[itemId].Amount)
            {
                _items[itemId].Amount -= amount;
                return true;
            }

            return false;
        }
        
        public override string ToString()
        {
            if (_items.Count == 0)
            {
                return "ItemSet: Empty";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"ItemSet: {_items.Count} item types\n");
            
            foreach (var item in _items.Values)
            {
                sb.Append($"- {item}\n");
            }
            
            return sb.ToString().TrimEnd();
        }
    }
}
