using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StrangeSpace
{
    [AddComponentMenu("")]
    public class UIClipAnimation : UIAnimation
    {
        [SerializeField] private Animation _animationComponent;
        [SerializeField] private AnimationClip _clip;
        [SerializeField] private float _crossFadeDuration = 0;
        [SerializeField] private bool _playInReverse;

        public override void Sample(float normalizedTime)
        {
            AnimationClip clip = _clip;
            _animationComponent.clip = clip;

            AnimationState animationState = _animationComponent[clip.name];
            animationState.normalizedTime = normalizedTime;
            animationState.enabled = true;
            animationState.weight = 1;
            _animationComponent.Sample();
            animationState.enabled = false;
        }

        protected override async UniTask Internal_Play()
        {
            AnimationClip clip = _clip;
            _animationComponent.clip = clip;

            if (_crossFadeDuration > 0)
            {
                _animationComponent.CrossFade(clip.name, _crossFadeDuration);
            }
            else
            {
                _animationComponent.Play();
            }

            AnimationState animationState = _animationComponent[clip.name];
            if (_playInReverse)
            {
                animationState.speed = -1;
                animationState.time = clip.length;
            }
            else
            {
                animationState.speed = 1;
                animationState.time = 0;
            }

            await UniTask.WaitForSeconds(clip.length);

            if (_isPlaying)
            {
                _isPlaying = false;
                if (_animationComponent.clip == clip)
                {
                    OnFinishPlaying();
                }
            }
        }

        private void OnValidate()
        {
            if (_animationComponent == null)
            {
                _animationComponent = GetComponent<Animation>();
            }
        }
    }
}