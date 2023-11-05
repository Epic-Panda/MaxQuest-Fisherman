using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;
using Unity.Netcode;

public class GameManager : EpSingletone<GameManager>
{
    [SerializeField] GameController m_gamePrefab;

    [SerializeField] ServerStartup m_server;
    [SerializeField] ClientStartup m_client;

    public GameController CurrentGame { get; set; }

    void Start()
    {
#if IS_SERVER
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;

        m_server.SetupAndStart();
#else
        ResourceManager.Instance.Setup();

        m_client.OnClientStartFinishEvent += OnClientStartFinishEvent;
        m_client.OnDisconnectEvent += OnDisconnectEvent;
        m_server.OnOtherClientDisconnectedClientEvent += OnOtherClientDisconnectedClientEvent;

        m_client.Setup();
#endif
    }

    void OnClientStartFinishEvent(bool success)
    {
        if(success)
        {
            UIManager.Instance.SwitchToLevelMenu();
        }
        else
        {
            UIManager.Instance.SwitchToMainMenu();
        }
    }

    void OnDisconnectEvent()
    {
        StartCoroutine(GoToMenu());

        static IEnumerator GoToMenu()
        {
            UIManager.Instance.ShowLoadingScreen();
            yield return new WaitForSecondsRealtime(1);
            UIManager.Instance.SwitchToMainMenu();
        }
    }

    void OnOtherClientDisconnectedClientEvent(ulong clientId)
    {
        UIManager.Instance.LevelHud.RestartOtherPlayerData();
    }

    void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count == 0)
            NetworkManager.Singleton.Shutdown();
        else
            OnClientDisconnected_ClientRPC(clientId);
    }

    private void OnServerStarted()
    {
        if(CurrentGame != null)
            return;

        Instantiate(m_gamePrefab)
            .GetComponent<NetworkObject>().Spawn();
    }

    public void StartGame()
    {
        if(CurrentGame != null)
            return;

        UIManager.Instance.LevelHud.ResetData();

        UIManager.Instance.ShowLoadingScreen();
        m_client.StartClient();
    }

    public void StopGame()
    {
        if(CurrentGame == null)
            return;

        UIManager.Instance.ShowLoadingScreen();
        m_client.StopClient();
    }

    public void StartFishing(float betValue)
    {
        CurrentGame.StartFishing(betValue);
    }
}
