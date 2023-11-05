using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class ServerStartup : NetworkBehaviour
{
    [SerializeField] ushort m_maxPlayers = 2;

    const string InternalServerIP = "0.0.0.0";
    string m_externalServerIP = "0.0.0.0";
    ushort m_serverPort = 7777;

    IMultiplayService m_multiplayService;
    const int MultiplayServiceTimeout = 20000;

    string m_allocationId;
    MultiplayEventCallbacks m_serverCallbacks;
    IServerEvents m_serverEvents;

    BackfillTicket m_localBackfillTicket;
    CreateBackfillTicketOptions m_createBackfillTicketOptions;
    const int TicketCheckMs = 1000;

    MatchmakingResults m_matchmakingPayload;

    bool m_backfilling = false;

    string ExternalConnectionString { get { return $"{m_externalServerIP}:{m_serverPort}"; } }

    public event System.Action<ulong> OnOtherClientDisconnectedClientEvent;

    public async void SetupAndStart()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for(int i = 0; i < args.Length - 1; i++)
        {
            if(args[i].Equals("-port"))
                m_serverPort = ushort.Parse(args[i + 1]);

            else if(args[i].Equals("-ip"))
                m_externalServerIP = args[i + 1];
        }

        StartServer();
        await StartServerServices();
    }

    void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(InternalServerIP, m_serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        if(NetworkManager.Singleton.ConnectedClients.Count >= m_maxPlayers)
        {
            response.Approved = false;
            response.Reason = "Server is Full";
        }

        response.Pending = false;
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            m_multiplayService = MultiplayService.Instance;
            await m_multiplayService.StartServerQueryHandlerAsync(m_maxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service:\n{ex}");
        }

        try
        {
            m_matchmakingPayload = await GetMatchMakerPayload(MultiplayServiceTimeout);

            if(m_matchmakingPayload != null)
            {
                Debug.Log($"Got payload: {m_matchmakingPayload}");
                await StartBackfill(m_matchmakingPayload);
            }
            else
                Debug.LogWarning("Getting the Matchmaker Payload timed out, starting with defaults.");
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the Allocation & Backfill Service:\n{ex}");
        }
    }

    async Task<MatchmakingResults> GetMatchMakerPayload(int timeout)
    {
        Task<MatchmakingResults> matchmakerPayloadTask = SubscribeAndWaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    async Task<MatchmakingResults> SubscribeAndWaitMatchmakerAllocation()
    {
        if(m_multiplayService == null)
            return null;

        m_allocationId = null;

        m_serverCallbacks = new MultiplayEventCallbacks();
        m_serverCallbacks.Allocate += OnMultiplayAllocation;
        m_serverEvents = await m_multiplayService.SubscribeToServerEventsAsync(m_serverCallbacks);

        m_allocationId = await AwaitAllocationID();

        return await GetMatchMakerAllocationPayloadAsync();
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"{nameof(OnMultiplayAllocation)}: {allocation.AllocationId}");

        if(string.IsNullOrEmpty(allocation.AllocationId)) return;

        m_allocationId = allocation.AllocationId;
    }

    async Task<string> AwaitAllocationID()
    {
        ServerConfig config = m_multiplayService.ServerConfig;

        Debug.Log($"{nameof(AwaitAllocationID)} - Server config is:\n" +
            $"-ServerID: {config.ServerId}\n" +
            $"-AllocationID: {config.AllocationId}\n" +
            $"-Port: {config.Port}\n" +
            $"-QPort: {config.QueryPort}\n" +
            $"-logs: {config.ServerLogDirectory}");

        while(string.IsNullOrEmpty(config.AllocationId))
        {
            string configId = config.AllocationId;
            if(!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(m_allocationId))
            {
                m_allocationId = configId;
                break;
            }

            await Task.Delay(100);
        }

        return m_allocationId;
    }

    async Task<MatchmakingResults> GetMatchMakerAllocationPayloadAsync()
    {
        try
        {
            MatchmakingResults payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();

            string modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"{nameof(GetMatchMakerAllocationPayloadAsync)}:\n {modelAsJson}");

            return payloadAllocation;
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to get the Matchmaker Payload in {nameof(GetMatchMakerAllocationPayloadAsync)}:\n{ex}");
        }

        return null;
    }

    async Task StartBackfill(MatchmakingResults payload)
    {
        BackfillTicketProperties backfillProperties = new BackfillTicketProperties(payload.MatchProperties);

        m_localBackfillTicket = new BackfillTicket
        {
            Id = payload.MatchProperties.BackfillTicketId,
            Properties = backfillProperties
        };

        await BeginBackfilling(payload);
    }

    async Task BeginBackfilling(MatchmakingResults payload)
    {
        if(string.IsNullOrEmpty(m_localBackfillTicket.Id))
        {
            m_createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = ExternalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(payload.MatchProperties)
            };

            m_localBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(m_createBackfillTicketOptions);
        }

        m_backfilling = true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        BackfillLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    async Task BackfillLoop()
    {
        while(m_backfilling && NeedsPlayers())
        {
            m_localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(m_localBackfillTicket.Id);

            if(!NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(m_localBackfillTicket.Id);
                m_localBackfillTicket.Id = null;
                m_backfilling = false;
                return;
            }

            await Task.Delay(TicketCheckMs);
        }

        m_backfilling = false;
    }

    void Singleton_OnClientDisconnectCallback(ulong cliendId)
    {
        if(NetworkManager.Singleton.ConnectedClients.Count > 0)
            OnClientDisconnected_ClientRPC(cliendId);

        if(!m_backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedsPlayers())
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            BeginBackfilling(m_matchmakingPayload);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    [ClientRpc]
    void OnClientDisconnected_ClientRPC(ulong cliendId)
    {
        if(!NetworkManager.LocalClientId.Equals(cliendId))
            OnOtherClientDisconnectedClientEvent?.Invoke(cliendId);
    }

    bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < m_maxPlayers;
    }

    void Dispose()
    {
        m_serverCallbacks.Allocate -= OnMultiplayAllocation;
        m_serverEvents?.UnsubscribeAsync();
    }
}
