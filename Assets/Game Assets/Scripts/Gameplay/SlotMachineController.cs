using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachineController : MonoBehaviour
{
    [SerializeField] VolatilityData m_volatilityData;

    int m_missCount;
    int m_winCount;

    float m_totalBets;
    float m_totalWinnings;

    int m_remainingRounds;
    int m_remainingWins;

    void Start()
    {
        ResetStats();
    }

    public void ResetStats()
    {
        m_winCount = 0;
        m_missCount = 0;

        m_totalBets = 0;
        m_totalWinnings = 0;

        m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
        m_remainingWins = m_volatilityData.GuarantiedWin;
    }

    public void Spin(float betAmount)
    {
        m_remainingRounds--;

        // fish cant be missed if remaining rounds is less than guarantied catch amount
        ItemData item= SimulateCast(m_remainingWins <= m_remainingRounds);
        
        float winnings = 0;

        if(item != null)
        {
            m_winCount++;
            m_remainingWins--;
            winnings = betAmount * item.PayoutMultiplier;
        }
        else
        {
            m_missCount++;
            item = m_volatilityData.MissItem;
        }

        UIManager.Instance.LevelHud.CollectItem(item, m_winCount, m_missCount);

        m_totalBets += betAmount;
        m_totalWinnings += winnings;

        if(m_remainingRounds == 0)
        {
            m_remainingRounds = m_volatilityData.GuarantiedWinInRound;
            m_remainingWins = m_volatilityData.GuarantiedWin;
        }
    }

    public void LogCurrentRtp()
    {
        float rtp = m_totalWinnings / m_totalBets * 100;
        Debug.Log($"{nameof(SlotMachineController)} Total Bets: {m_totalBets}"
            + $"\nTotal Winnings: {m_totalWinnings}"
            + $"\nMissed [{m_missCount}], win count [{m_winCount}]");
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
