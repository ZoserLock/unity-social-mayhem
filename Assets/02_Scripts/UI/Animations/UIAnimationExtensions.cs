using System;

namespace StrangeSpace
{
    public static class UIAnimationExtensions
    {
        public static void SafePlay(this UIAnimation animation, Action callback = null)
        {
            if (animation != null)
            {
                animation.Play(callback);
            }
            else
            {
                callback?.Invoke();
            }
        }
    }
}