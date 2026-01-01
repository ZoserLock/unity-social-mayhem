using System.Collections.Generic;
using StrangeSpace;
using UnityEngine;
using Zen.Debug;

namespace StrangeSpace
{
    public class GameDatabase : PlainSingleton<GameDatabase>
    {
        private GameDatabaseRoot _root;
        private Dictionary<string, ItemInfo> _items = new();
        
        protected override void OnInitialize()
        {
            _root = Root.Get<GameDatabaseRoot>();
            
            // Fill Object Info dictionaries
           /* foreach (var info in _root.ItemGeneratorInfoList.List)
            {
                _idToItemGeneratorInfo.Add(info.Id, info);
            }

            foreach (var info in _root.TransportDroneInfoList.List)
            {
                _idToTransportDroneInfo.Add(info.Id, info);
            }
            
            foreach (var info in _root.SectorInfoList.List)
            {
                _idToSectorInfo.Add(info.Id, info);
            }

            // Fill Object View Dictionaries
            foreach (var view in _root.ItemGeneratorViewList.List)
            {
                _idToItemGeneratorView.Add(view.Id, view);
            }
            
            foreach (var view in _root.TransportDroneViewList.List)
            {
                _idToTransportDroneView.Add(view.Id, view);
            }
            
            foreach (var view in _root.SectorViewList.List)
            {
                _idToSectorView.Add(view.Id, view);
            }*/
        }

        protected override void OnDeinitialize()
        {
            _root = null;
        }
        
        /* public ItemGeneratorInfo GetItemGeneratorInfo(string infoId)
        {
            if (_idToItemGeneratorInfo.TryGetValue(infoId, out var info))
            {
                return info;
            }
            
            ZenLog.Error($"[Game Database]: Unable to get ItemGeneratorInfo for key {infoId}");

            return null;
        }

        public TransportDroneInfo GetTransportDroneInfo(string infoId)
        {
            if (_idToTransportDroneInfo.TryGetValue(infoId, out var info))
            {
                return info;
            }

            ZenLog.Error($"[Game Database]: Unable to get TransportDroneInfo for key {infoId}");
            
            return null;
        }
        public SectorInfo GetSectorInfo(string infoId)
        {
            if (_idToSectorInfo.TryGetValue(infoId, out var info))
            {
                return info;
            }

            ZenLog.Error($"[Game Database]: Unable to get Sector Info for key {infoId}");
            
            return null;
        }
        
        // View Functions
        public ItemGeneratorView GetItemGeneratorView(string viewId, Transform parent)
        {
            if (_idToItemGeneratorView.TryGetValue(viewId, out var view))
            {
                var instance = Object.Instantiate(view, parent, false);
                
                return instance;
            }
            
            ZenLog.Error($"[Game Database]: Unable to get ItemGeneratorView for key {viewId}");
            
            return null;
        }

        public TransportDroneView GetTransportDroneView(string viewId, Transform parent)
        {
            if (_idToTransportDroneView.TryGetValue(viewId, out var view))
            {
                var instance = Object.Instantiate(view, parent, false);

                return instance;
            }

            ZenLog.Error($" [Game Database]: Unable to get TransportDroneView for key {viewId}");
            
            return null;
        }
        public SectorView GetSectorView(string viewId, Transform parent)
        {
            if (_idToSectorView.TryGetValue(viewId, out var view))
            {
                var instance = Object.Instantiate(view, parent, false);

                return instance;
            }

            ZenLog.Error($" [Game Database]: Unable to get Sector View for key {viewId}");
            
            return null;
        }*/
    }
}
