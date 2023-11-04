using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachineController : MonoBehaviour
{
    [SerializeField] VolatilityData m_volatilityData;
    [SerializeField] float m_spinDuration;

    int m_totalAttempts;
    int m_totalAttemptsWon;

    float m_totalBets;
    float m_totalBetsWon;

    int m_remainingRounds;
    int m_remainingWins;

    public delegate void OnSlotStartDelegate(bool success, float betValue);
    public event OnSlotStartDelegate OnSlotStartEvent;

    public delegate void OnSlotFinishDelegate(ItemData item, bool isWin, float winValue);
    public event OnSlotFinishDelegate OnSlotFinishEvent;

    void Start()
    {
        ResetStats();
    }

    public void ResetStats()
    {
        m_totalAttempts = 0;
        m_totalAttemptsWon = 0;

        m_totalBets = 0;
        m_totalBetsWon = 0;

        m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
        m_remainingWins = m_volatilityData.GuarantiedWin;
    }

    public void Spin(float betAmount)
    {
        OnSlotStartEvent?.Invoke(true, betAmount);

        StartCoroutine(SimulateSpin(betAmount));
    }

    IEnumerator SimulateSpin(float betAmount)
    {
        yield return new WaitForSecondsRealtime(m_spinDuration);
        m_remainingRounds--;

        m_totalAttempts++;

        // fish cant be missed if remaining rounds is less than guarantied catch amount
        ItemData item = SimulateCast(m_remainingWins <= m_remainingRounds);

        bool isWin = item != null;
        float winAmount = 0;

        if(item != null)
        {
            m_totalAttemptsWon++;
            m_remainingWins--;
            winAmount = betAmount * item.PayoutMultiplier;
        }
        else
        {
            item = m_volatilityData.MissItem;
        }

        m_totalBets += betAmount;
        m_totalBetsWon += winAmount;

        if(m_remainingRounds == 0)
        {
            m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
            m_remainingWins = m_volatilityData.GuarantiedWin;
        }

        OnSlotFinishEvent?.Invoke(item, isWin, winAmount);
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
