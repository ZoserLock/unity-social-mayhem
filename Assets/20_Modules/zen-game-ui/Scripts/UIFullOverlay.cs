using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using StrangeSpace;
using UnityEngine.Serialization;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public sealed class UIFullOverlay : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
