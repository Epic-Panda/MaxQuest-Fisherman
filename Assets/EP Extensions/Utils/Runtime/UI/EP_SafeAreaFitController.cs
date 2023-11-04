using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.UI;

namespace EP.Utils
{
    public class EP_SafeAreaFitController : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] RectTransform m_landscapeContent;
        [SerializeField] RectTransform m_portraitContent;

        Canvas m_canvas;
        RectTransform m_singleContent;
        bool m_isSet = false;

#if UNITY_ANDROID || UNITY_IOS
        void OnEnable()
        {
            StartCoroutine(LateSafeAreaCheck());
        }

        void OnDisable()
        {
            EP_SafeAreaManager.OnSafeAreaChangeEvent -= OnSafeAreaChangeEvent;
        }
#endif

        IEnumerator LateSafeAreaCheck()
        {
            Setup();
            yield return null;

            EP_SafeAreaManager.OnSafeAreaChangeEvent += OnSafeAreaChangeEvent;

            yield return new WaitWhile(() => EP_SafeAreaManager.Instance == null);
            OnSafeAreaChangeEvent();
        }

        void Setup()
        {
            if(m_isSet)
                return;

            m_isSet = true;

            m_canvas = GetComponentInParent<Canvas>();

            if(m_landscapeContent == null || m_portraitContent == null)
            {
                if(m_landscapeContent != null)
                    m_singleContent = m_landscapeContent;
                else
                    m_singleContent = m_portraitContent;
            }

            if(m_landscapeContent)
                m_landscapeContent.anchorMin = m_landscapeContent.anchorMax = new Vector2(.5f, .5f);

            if(m_portraitContent)
                m_portraitContent.anchorMin = m_portraitContent.anchorMax = new Vector2(.5f, .5f);
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
