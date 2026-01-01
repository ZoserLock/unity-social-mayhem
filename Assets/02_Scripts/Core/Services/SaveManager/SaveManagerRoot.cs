 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;


namespace StrangeSpace
{
    public class SaveManagerRoot : SingletonRoot
    {
        [SerializeField] 
        private SaveManagerSettings _settings = new SaveManagerSettings();
        
        // Get / Set
        public SaveManagerSettings Settings => _settings;
    }
}