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
    public class UIWorkRoot : MonoBehaviour
    {
        [SerializeField] private GameCameraController _camera;
        
        public GameCameraController Camera => _camera;
    }
}