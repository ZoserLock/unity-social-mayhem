using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    [CreateAssetMenu(menuName = "Strange Space/UI/UIManager Configuration", fileName = "UIManagerConfiguration")]
    public class UIManagerConfiguration : ScriptableObject
    {
        [SerializeField]
        private UIPanelList[] _panelLists;
        
        // Get / Set
        public UIPanelList[] PanelLists => _panelLists;
    }
}
