using System.Collections;
using UnityEngine;

using EP.Utils.Core;

namespace EP.Utils.UI
{
    public class EP_SafeAreaManager : EpSingletone<EP_SafeAreaManager>
    {
        [SerializeField] bool m_checkSafeAreaOnce;

        Vector2Int m_screenSize;
        Rect m_screenSafeArea;

        public bool IsLandscape { get; private set; }

        public static event System.Action OnSafeAreaChangeEvent;

        void Start()
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            m_checkSafeAreaOnce = true;
#endif
            StartCoroutine(CheckSafeAreaChange());
        }

        IEnumerator CheckSafeAreaChange()
        {
            while(!m_checkSafeAreaOnce)
            {
                if(m_screenSafeArea != Screen.safeArea)
                {
                    // One frame is required for other canvases to adjust their sizes
                    yield return null;
                    UpdateSafeArea();
                }

                yield return null;
            }
        }

        void UpdateSafeArea()
        {
            m_screenSafeArea = Screen.safeArea;
            m_screenSize = new Vector2Int(Screen.width, Screen.height);
            IsLandscape = Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;

            OnSafeAreaChangeEvent?.Invoke();
        }

        /// <summary>
        /// Returns the safe area based on the canvas.
        /// </summary>
        /// <param name="canvas">Canvas to get the safe area for.</param>
        /// <returns>Returns the safe area that matches the given canvas.</returns>
        public Rect GetSafeArea(Canvas canvas)
        {
            Rect canvasRect = canvas.GetComponent<RectTransform>().rect;

            float canvasScale = canvasRect.height / m_screenSize.y;

            Vector2 center = new Vector2(m_screenSafeArea.center.x - m_screenSize.x / 2.0f, m_screenSafeArea.center.y - m_screenSize.y / 2.0f) * canvasScale;
            Vector2 size = m_screenSafeArea.size * canvasScale;

            return new Rect(center - size * .5f, size);
        }
    }
}