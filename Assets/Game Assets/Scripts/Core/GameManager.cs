using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;

public class GameManager : EpSingletone<GameManager>
{
    [SerializeField] GameController m_gamePrefab;

    public GameController CurrentGame { get; private set; }

    void Start()
    {
        ResourceManager.Instance.Setup();
    }

    public void StartGame()
    {
        if(CurrentGame != null)
            return;

        UIManager.Instance.LevelHud.ResetData();
        UIManager.Instance.SwitchToLevelMenu();

        CurrentGame = Instantiate(m_gamePrefab);
        CurrentGame.Setup();
    }

    public void StopGame()
    {
        if(CurrentGame == null)
            return;

        UIManager.Instance.SwitchToMainMenu();

        Destroy(CurrentGame.gameObject);
    }

    public void StartFishing(float betValue)
    {
        CurrentGame.StartFishing(betValue);
    }
}
