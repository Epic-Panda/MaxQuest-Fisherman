using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ClientStartup : MonoBehaviour
{
    string m_ticketId;

    void Start()
    {
        SignIn();
    }

    async void SignIn()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    public void StartClient()
    {
        CreateTIcket();
    }

    async void CreateTIcket()
    {
        CreateTicketOptions options = new CreateTicketOptions("default");

        List<Player> player = new List<Player>
        {
            new Player(PlayerID())
        };

        CreateTicketResponse ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(player, options);
        m_ticketId = ticketResponse.Id;

        Debug.Log($"Ticket ID: {m_ticketId}");

        PoolTicketStatus();
    }

    async void PoolTicketStatus()
    {
        bool gotAssignment = false;
        do
        {
            await Task.Delay(1000);

            TicketStatusResponse ticketStatus = await MatchmakerService.Instance.GetTicketAsync(m_ticketId);

            if(ticketStatus == null)
                continue;

            if(ticketStatus.Value is MultiplayAssignment multiplayAssignment)
            {
                switch(multiplayAssignment.Status)
                {
                    case MultiplayAssignment.StatusOptions.Found:
                        gotAssignment = true;
                        TicketAssigned(multiplayAssignment);
                        break;

                    case MultiplayAssignment.StatusOptions.InProgress:
                        break;

                    case MultiplayAssignment.StatusOptions.Timeout:
                        gotAssignment = true;
                        Debug.LogError($"Failed to get ticket status. Ticket timed out.");
                        break;

                    case MultiplayAssignment.StatusOptions.Failed:
                        gotAssignment = true;
                        Debug.LogError($"Failed to get ticket status. Error: {multiplayAssignment.Message}");
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        } while(!gotAssignment);
    }

    void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"{nameof(TicketAssigned)}: {assignment.Ip}:{assignment.Port}");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
    }
}
