using UnityEngine;

namespace StrangeSpace
{
    public class InputManagerRoot : MonoBehaviour
    {
        [SerializeField] 
        private InputManagerSettings _settings = new InputManagerSettings();
        
        // Get / Set
        public InputManagerSettings Settings => _settings;
    }

}