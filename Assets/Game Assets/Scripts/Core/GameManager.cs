using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;
using Unity.Netcode;

public class GameManager : EpSingletone<GameManager>
{
    [SerializeField] GameController m_gamePrefab;

    ServerStartup m_server;
    ClientStartup m_client;

    public GameController CurrentGame { get; set; }

    void Start()
    {
#if IS_SERVER
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        //NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;

        m_server = new ServerStartup();
        m_server.SetupAndStart();
#else
        ResourceManager.Instance.Setup();
        m_client = new ClientStartup();
        m_client.Setup();
#endif
    }

    //void Server_OnClientDisconnectCallback(ulong clientId)
    //{
    //    if(NetworkManager.Singleton.ConnectedClients.Count == 0)
    //        NetworkManager.Singleton.Shutdown();
    //}

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
        UIManager.Instance.SwitchToLevelMenu();

        m_client.StartClient();
    }

    public void StopGame()
    {
        if(CurrentGame == null)
            return;

        m_client.StopClient();
        UIManager.Instance.SwitchToMainMenu();
    }

    public void StartFishing(float betValue)
    {
        CurrentGame.StartFishing(betValue);
    }
}
