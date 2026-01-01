using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    [CreateAssetMenu(menuName = "Nerdvania/UI/PanelList", fileName = "UIPanelList")]
    public class UIPanelList : ScriptableObject
    {
        [SerializeField]
        private UIPanel[] _panels;

        #region Get/Set
        public UIPanel[] Panels
        {
            get { return _panels; }
        }
        #endregion
    }
}
