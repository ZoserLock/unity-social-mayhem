using UnityEngine;
using System.Collections;

namespace StrangeSpace
{
    public enum UIPanelTweenAnimationType
    {
        None = 0,
        Scale = 1,
        Translate = 2,
    }
    public class UIPanelTweenAnimation : UIPanelAnimation
    {
        [SerializeField]
        protected UIPanel m_parentPanel;

        private System.Action m_onComplete;

        [SerializeField]
        private Vector3 m_showPosition;

        [SerializeField]
        private Vector3 m_hidePosition;

        [SerializeField]
        [HideInInspector]
        private Vector3 m_baseScale;

        [Header("Show Animation")]
        [SerializeField]
        private UIPanelTweenAnimationType m_showAnimationType = UIPanelTweenAnimationType.Scale;

        // [SerializeField]
        // private LeanTweenType m_showTweenType = LeanTweenType.linear;

        [SerializeField]
        private float m_showTime = 1.0f;

        [Header("Close Animation")]
        [SerializeField]
        private UIPanelTweenAnimationType m_closeAnimationType = UIPanelTweenAnimationType.Scale;

        // [SerializeField]
        // private LeanTweenType m_closeTweenType = LeanTweenType.linear;

        [SerializeField]
        private float m_closeTime = 1.0f;

        public UIPanel ParentPanel
        {
            get { return m_parentPanel; }
            set { m_parentPanel = value; }
        }

        public Vector3 HidePosition
        {
            get { return m_hidePosition; }
        }

        public Vector3 ShowPosition
        {
            get { return m_showPosition; }
        }

        public UIPanelTweenAnimationType ShowAnimationType
        {
            get { return m_showAnimationType; }
        }

        public UIPanelTweenAnimationType CloseAnimationType
        {
            get { return m_closeAnimationType; }
        }

        public override void OnAnimateShow()
        {
            RectTransform mainPanelTransform = m_parentPanel.MainPanel.GetComponent<RectTransform>();

            if (m_showAnimationType == UIPanelTweenAnimationType.Scale)
            {
                m_parentPanel.MainPanel.transform.localScale = new Vector3(0, 0, 0);
                // LeanTween.scale(m_parentPanel.MainPanel, m_baseScale, m_showTime).setEase(m_showTweenType).setOnComplete(CallOnComplete);
            }
            else if (m_showAnimationType == UIPanelTweenAnimationType.Translate)
            {
                mainPanelTransform.anchoredPosition3D = m_hidePosition;
                //LeanTween.move(mainPanelTransform, m_showPosition, m_showTime).setEase(m_showTweenType).setOnComplete(CallOnComplete);
            }
            else
            {
                CallOnComplete();
            }

        }

        public override void OnAnimateHide()
        {
            RectTransform mainPanelTransform = m_parentPanel.MainPanel.GetComponent<RectTransform>();

            if (m_closeAnimationType == UIPanelTweenAnimationType.Scale)
            {
                // LeanTween.scale(m_parentPanel.MainPanel, new Vector3(0, 0, 0), m_closeTime).setEase(m_closeTweenType).setOnComplete(CallOnComplete);
            }
            else if (m_showAnimationType == UIPanelTweenAnimationType.Translate)
            {
                // LeanTween.move(mainPanelTransform, m_hidePosition, m_closeTime).setEase(m_closeTweenType).setOnComplete(CallOnComplete);
            }
            else
            {
                CallOnComplete();
            }
        }

        private void UpdateTranslateY()
        {

        }

        #region Editor Only Functions
        public void _editorSetBaseScale(Vector3 scale)
        {
            m_baseScale = scale;
        }

        public void _editorSetShowPosition(Vector3 position)
        {
            m_showPosition = position;
        }

        public void _editorSetHidePosition(Vector3 position)
        {
            m_hidePosition = position;
        }

        #endregion
    }

}