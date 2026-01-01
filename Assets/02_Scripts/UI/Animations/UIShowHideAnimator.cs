using System;
using UnityEngine;

namespace StrangeSpace
{
    public class UIShowHideAnimator : MonoBehaviour
    {
        public enum EInitialState
        {
            Undefined,
            InstantHide,
            InstantShow,
            Hide,
            Show,
        }

        [SerializeField] private UIAnimation _showAnimation;
        [SerializeField] private UIAnimation _hideAnimation;

        [Header("Configurations")]
        [SerializeField] private EInitialState _initialState = EInitialState.InstantHide;

        private void Awake()
        {
            switch (_initialState)
            {
                case EInitialState.InstantHide: HideInstant(); break;
                case EInitialState.InstantShow: ShowInstant(); break;
                case EInitialState.Hide: Hide(); break;
                case EInitialState.Show: Show(); break;
                case EInitialState.Undefined:
                    Debug.LogWarning("Initial state is not being defined on animator, name: " + name);
                    break;
                default:
                    Debug.LogError("Initial state is not being handled: " + _initialState);
                    break;
            }

        }

        public void Show(Action callback = null)
        {
            if (_showAnimation != null)
            {
                _showAnimation.Play(callback);
            }
            else
            {
                gameObject.SetActive(true);
                callback?.Invoke();
            }
        }

        public void Hide(Action callback = null)
        {
            if (_hideAnimation != null)
            {
                _hideAnimation.Play(callback);
            }
            else
            {
                gameObject.SetActive(false);
                callback?.Invoke();
            }
        }

        public void ShowInstant()
        {
            if (_hideAnimation != null)
            {
                _hideAnimation.Sample(0);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void HideInstant()
        {
            if (_showAnimation != null)
            {
                _showAnimation.Sample(0);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}