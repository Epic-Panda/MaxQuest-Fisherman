using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;
using UnityEngine.UI;

public class UIManager : EpSingletone<UIManager>
{
    [SerializeField] LevelHudController m_levelHud;
    [SerializeField] MainMenu m_mainMenu;
    [SerializeField] GameObject m_loadingScreen;

    public LevelHudController LevelHud { get { return m_levelHud; } }

    void Start()
    {
        m_mainMenu.Setup();
        m_levelHud.Setup();
    }

    public void ShowLoadingScreen()
    {
        m_loadingScreen.SetActive(true);

        LevelHud.Show(false);
        m_mainMenu.Show(false);
    }

    public void SwitchToMainMenu()
    {
        m_mainMenu.Show(true);
        m_loadingScreen.SetActive(false);
    }

    public void SwitchToLevelMenu()
    {
        LevelHud.Show(true);
        m_loadingScreen.SetActive(false);
    }

    [System.Serializable]
    class MainMenu
    {
        [SerializeField] GameObject m_mainMenu;

        [SerializeField] Button m_playButton;
        [SerializeField] Button m_quitButton;

        public void Setup()
        {
#if UNITY_WEBGL
            m_quitButton.gameObject.SetActive(false);
#endif
            m_playButton.onClick.AddListener(GameManager.Instance.StartGame);
            m_quitButton.onClick.AddListener(Application.Quit);
        }

        public void Show(bool show)
        {
            m_mainMenu.SetActive(show);
        }
    }
}
