using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using Zen.Debug;
using Zenject;
using Random = UnityEngine.Random;

namespace StrangeSpace
{
    public class GameDirectorRoot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] 
        private GameCameraController _gameCamera;
        
        [SerializeField] 
        private Transform _levelRoot;
        
        [SerializeField] 
        private Transform _worldRoot;
        
        // Get / Set
        public Transform LevelRoot => _levelRoot;
        public Transform WorldRoot => _worldRoot;
        public GameCameraController GameCamera => _gameCamera;
        
    }
}