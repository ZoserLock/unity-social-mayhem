using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StrangeSpace
{
    [AddComponentMenu("")]
    public class UIAnimation : MonoBehaviour
    {
        [SerializeField] private string _description; // [AKP] This is not used by code, is just used to identify the animation more easily.

        protected bool _isPlaying;

        private Action _onFinishPlaying;

        public virtual void Sample(float normalizedTime)
        {
            Debug.LogError("The game object is using an UIAnimation this class should only be used as a base class. Base should not be called", gameObject);
        }

        public void Play(Action callback = null)
        {
            if (_isPlaying)
            {
                Debug.LogWarning("Play was called on an animation that was already playing: " + name);
            }

            _isPlaying = true;
            _onFinishPlaying = callback;
            Internal_Play().Forget();
        }

        protected virtual async UniTask Internal_Play()
        {
            Debug.LogError("The game object is using an UIAnimation this class should only be used as a base class. Base should not be called", gameObject);
            await UniTask.NextFrame();
            OnFinishPlaying();
        }

        protected void OnFinishPlaying()
        {
            _isPlaying = false;
            _onFinishPlaying?.Invoke();
        }
    }
}