using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Zen.Debug;
using Zenject;
using Random = UnityEngine.Random;

namespace StrangeSpace
{
    public class GameDirectorData
    {
    }
    
    public class GameDirector : IEditorInspectorProvider
    {
        private GameDirectorRoot _root;
        
        private readonly Transform _worldRootTransform;
        private readonly Transform _levelRootTransform;
        
        private IGameTaskManager _gameTaskManager;
        private ITimeManager _timeManager;
        private GameDatabase _gameDatabase;
        
        private GameCameraController _gameCamera;
        
        // Get / Set
        public GameCameraController GameCamera => _gameCamera;

        public GameDirector(IGameTaskManager gameTaskManager, 
                            ITimeManager timeManager, 
                            GameDatabase gameDatabase, 
                            GameDirectorRoot root)
        {
            _gameTaskManager = gameTaskManager;
            _timeManager     = timeManager;
            _gameDatabase    = gameDatabase;
            
            _root       = root;
            _gameCamera = root.GameCamera;
            
            _worldRootTransform = root.WorldRoot;
            _levelRootTransform = root.LevelRoot;
            
            #if UNITY_EDITOR
            EditorDependencies.AddInspectorProvider(this);
            #endif
        }
        
        // New Game entry Point - This is called when a new game is generated.
        public void InitializeAsNew()
        {
            // Create Universe
            // _sectorController.InitializeAsNew();
       
            // droneTransport.SetItemRequest();
            var requestedItems = new List<(string, int)>()
            {
                ("mineral_iron_ore", 2),
            };
        }
        
        // Load Game Entry Point - Called when a game is loaded.
        public void InitializeWithData(GameDirectorData data)
        {
            // Initialize with data if needed
            if (data == null)
            {
                ZenLog.Error(LogCategory.System, "[GameDirector]: GameDirector Data is null");
                return;
            }

            var linker = new Linker();
            var loadedObjects = new List<IBaseObject>();
            
            // Create Saved Objects.
            // _sectorController.InitializeWithData(data.SectorController);
            
            
            // Spaceships
            /* foreach (var spaceshipData in data.Spaceships)
            {
                var spaceship = CreateMainSpaceshipWithData(spaceshipData);
                
                linker.Add(spaceship.Id, spaceship);
                
                loadedObjects.Add(spaceship);
            }
            
            // Drones
            foreach (var droneData in data.Drones)
            {
                var drone = CreateTransportDroneWithData(droneData);
         
                linker.Add(drone.Id, drone);
                
                loadedObjects.Add(drone);
            }
            
            //Item Generators
            foreach (var generatorData in data.ItemGenerators)
            {
                var itemGenerator = CreateItemGeneratorWithData(generatorData);
         
                linker.Add(itemGenerator.Id, itemGenerator);
                
                loadedObjects.Add(itemGenerator);
            } */
            
            // Reconcile objects links
            foreach (var loadedObject in loadedObjects)
            {
                loadedObject.ReconcileLinks(linker);
            }
        }

        public GameDirectorData GetSaveData()
        {
            
            var data = new GameDirectorData
            { };
            
            return data;
        }
        
        // ************ //
        // EDITOR STUFF //
        // ************ //
        public InspectorInfo GetInspectorInfo()
        {
            return new InspectorInfo
            {
                Name = "GameDirector",
                Description = "Manage the Dev Test Simulation",
            };
        }

        public IEditorInspector GetInspector()
        {
            #if UNITY_EDITOR
                return new GameDirectorInspector(this);
            #endif
            return null;
        }

        public void Tick(float deltaTime)
        {
        
        }
    }
}