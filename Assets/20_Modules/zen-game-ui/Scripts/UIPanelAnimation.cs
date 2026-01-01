using UnityEngine;

namespace StrangeSpace
{
    [AddComponentMenu("")]
    public class UIPanelAnimation : MonoBehaviour
    {
        private System.Action m_onComplete;

        public void AnimateShow(System.Action action)
        {
            m_onComplete = action;
            OnAnimateShow();
        }

        public void AnimateHide(System.Action action)
        {
            m_onComplete = action;
            OnAnimateHide();
        }

        public virtual void OnAnimateShow()
        {
            CallOnComplete();
            Debug.LogError("The game object is using an UIPanelAnimation this class should only be used as a base class. Base. Should not be called", gameObject);
        }

        public virtual void OnAnimateHide()
        {
            CallOnComplete();
            Debug.LogError("The game object is using an UIPanelAnimation this class should only be used as a base class. Base. Should not be called", gameObject);
        }

        protected void CallOnComplete()
        {
            if (m_onComplete != null)
            {
                m_onComplete();
            }
        }
    }
}