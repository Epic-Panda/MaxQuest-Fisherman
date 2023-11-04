using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;
using UnityEngine.UI;

public class UIManager : EpSingletone<UIManager>
{
    [SerializeField] LevelHudController m_levelHud;
    [SerializeField] MainMenu m_mainMenu;

    public LevelHudController LevelHud { get { return m_levelHud; } }

    void Start()
    {
        m_mainMenu.Setup();
        m_levelHud.Setup();
    }

    public void SwitchToMainMenu()
    {
        LevelHud.Show(false);
        m_mainMenu.Show(true);
    }

    public void SwitchToLevelMenu()
    {
        LevelHud.Show(true);
        m_mainMenu.Show(false);
    }

    [System.Serializable]
    class MainMenu
    {
        [SerializeField] GameObject m_mainMenu;

        [SerializeField] Button m_playButton;
        [SerializeField] Button m_quitButton;

        public void Setup()
        {
            m_playButton.onClick.AddListener(GameManager.Instance.StartGame);
            m_quitButton.onClick.AddListener(Application.Quit);
        }

        public void Show(bool show)
        {
            m_mainMenu.SetActive(show);
        }
    }
}
