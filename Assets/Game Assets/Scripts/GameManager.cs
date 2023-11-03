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
        StartGame();
    }

    public void StartGame()
    {
        if(CurrentGame != null)
            return;

        CurrentGame = Instantiate(m_gamePrefab);
    }

    public void StopGame()
    {
        if(CurrentGame == null)
            return;

        Destroy(CurrentGame.gameObject);
    }
}
