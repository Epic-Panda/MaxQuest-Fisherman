using UnityEngine;
using UnityEngine.UI;

using EP.Utils.Editable.UI;

namespace EP.Utils.UI
{
    public class EP_PanelController : MonoBehaviour
    {
        [SerializeField] EP_UIPanelType m_panelType;

        [Space]
        [SerializeField] Button m_blockerBackButton;

        [Header("Content")]
        [SerializeField] RectTransform m_landscapeContent;
        [SerializeField] RectTransform m_portraitContent;

        RectTransform m_singleContent;

        protected Canvas m_canvas;

        public EP_UIPanelType PanelType { get { return m_panelType; } }

        public virtual void Setup()
        {
            if(m_blockerBackButton != null)
                m_blockerBackButton.onClick.AddListener(delegate { EP_PanelManager.Instance.ClosePanel(this); });

            m_canvas = GetComponentInParent<Canvas>();

            if(m_landscapeContent == null || m_portraitContent == null)
            {
                if(m_landscapeContent != null)
                    m_singleContent = m_landscapeContent;
                else
                    m_singleContent = m_portraitContent;
            }
        }

        public virtual void OpenPanel()
        {
            OnSafeAreaChangeEvent();
            gameObject.SetActive(true);

            EP_SafeAreaManager.OnSafeAreaChangeEvent += OnSafeAreaChangeEvent;
        }

        public virtual void ClosePanel()
        {
            gameObject.SetActive(false);

            EP_SafeAreaManager.OnSafeAreaChangeEvent -= OnSafeAreaChangeEvent;
        }

        void OnSafeAreaChangeEvent()
        {
            Rect safeArea = EP_SafeAreaManager.Instance.GetSafeArea(m_canvas);
            bool isLandscape = EP_SafeAreaManager.Instance.IsLandscape;

            if(m_singleContent)
            {
                m_singleContent.sizeDelta = safeArea.size;
                m_singleContent.anchoredPosition = safeArea.center;

                m_singleContent.gameObject.SetActive(true);
                return;
            }

            m_landscapeContent.gameObject.SetActive(isLandscape);
            m_portraitContent.gameObject.SetActive(!isLandscape);

            if(isLandscape)
            {
                m_landscapeContent.sizeDelta = safeArea.size;
                m_landscapeContent.anchoredPosition = safeArea.center;
            }
            else
            {
                m_portraitContent.sizeDelta = safeArea.size;
                m_portraitContent.anchoredPosition = safeArea.center;
            }
        }
    }
}