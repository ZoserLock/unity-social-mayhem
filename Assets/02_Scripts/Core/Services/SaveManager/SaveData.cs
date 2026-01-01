using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    [System.Serializable]
    public class SaveData
    {
        // Save Specific
        public int Version { get; set; } = 1;
        public int SaveCount { get; set; } = 0;
        public int LoadCount { get; set; } = 0;
        
        // Game Specific
        public GameData GameData { get; set; } = new();
    }

    public class GameData
    {
        // Dev
        public GameDirectorData GameDirector { get; set; } = new();
        public TimeManagerData TimeManager { get; set; } = new();
    }
}
