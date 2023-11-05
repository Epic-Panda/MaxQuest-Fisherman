using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SlotMachineController : NetworkBehaviour
{
    [SerializeField] VolatilityData m_volatilityData;
    [SerializeField] float m_spinDuration;

    NetworkVariable<int> m_totalRounds = new NetworkVariable<int>();
    NetworkVariable<int> m_totalRoundsWon = new NetworkVariable<int>();

    int m_remainingRounds;
    int m_remainingWins;

    public delegate void OnSlotStartDelegate(ulong clientId, bool success, float betValue);
    public event OnSlotStartDelegate OnSlotStartEvent;

    public delegate void OnSlotFinishDelegate(ulong clientId, ItemData item, bool isWin, float winValue);
    public event OnSlotFinishDelegate OnSlotFinishEvent;

    public delegate void OnTotalDataUpdateDelegate(int totalRounds, int totalRoundsWon);
    public static event OnTotalDataUpdateDelegate OnTotalDataUpdateEvent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsServer)
        {
            m_totalRounds.Value = 0;
            m_totalRoundsWon.Value = 0;

            m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
            m_remainingWins = m_volatilityData.GuarantiedWin;
        }
        else if(IsClient)
        {
            m_totalRounds.OnValueChanged += OnTotalValueChange;
            m_totalRoundsWon.OnValueChanged += OnTotalValueChange;
        }
    }

    void OnTotalValueChange(int previousValue, int newValue)
    {
        OnTotalDataUpdateEvent?.Invoke(m_totalRounds.Value, m_totalRoundsWon.Value);
    }

    public void Spin(float betAmount)
    {
        ConfirmBet_ServerRPC(NetworkManager.LocalClientId, betAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    void ConfirmBet_ServerRPC(ulong clientId, float betAmount)
    {
        OnBetConfirmedByServer_ClientRPC(clientId, betAmount);
        StartCoroutine(SimulateSpin(clientId, betAmount));
    }

    [ClientRpc]
    void OnBetConfirmedByServer_ClientRPC(ulong clientId, float betAmount)
    {
        OnSlotStartEvent?.Invoke(clientId, true, betAmount);
    }

    IEnumerator SimulateSpin(ulong clientId, float betAmount)
    {
        yield return new WaitForSecondsRealtime(m_spinDuration);
        m_remainingRounds--;

        m_totalRounds.Value++;

        // fish cant be missed if remaining rounds is less than guarantied catch amount
        ItemData item = SimulateCast(m_remainingWins <= m_remainingRounds);

        bool isWin = item != null;
        float winAmount = 0;

        if(item != null)
        {
            m_totalRoundsWon.Value++;
            m_remainingWins--;
            winAmount = betAmount * item.PayoutMultiplier;
        }
        else
        {
            item = m_volatilityData.MissItem;
        }

        if(m_remainingRounds == 0)
        {
            m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
            m_remainingWins = m_volatilityData.GuarantiedWin;
        }

        int itemId = System.Array.FindIndex(m_volatilityData.WinItems, x => x.itemData == item);

        OnSpinFinished_ClientRPC(clientId, itemId, isWin, winAmount);
    }

    [ClientRpc]
    void OnSpinFinished_ClientRPC(ulong clientId, int itemId, bool isWin, float winAmount)
    {
        ItemData item = !isWin ? m_volatilityData.MissItem : m_volatilityData.WinItems[itemId].itemData;

        OnSlotFinishEvent?.Invoke(clientId, item, isWin, winAmount);
    }

    ItemData SimulateCast(bool canMiss)
    {
        float winChance = canMiss ? m_volatilityData.WinChance / 100.0f : 1;

        float rnd = Random.value * 100;
        float acc = 0;

        foreach(var item in m_volatilityData.WinItems)
        {
            acc += item.collectChance * winChance;

            if(acc >= rnd)
                return item.itemData;
        }

        return null;
    }
}
