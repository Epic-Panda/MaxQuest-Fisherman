using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    enum FishTier { Low, Medium, High }

    [SerializeField] Fish[] m_fish;

    [Header("Rounds")]
    [SerializeField] int m_totalRounds;
    [SerializeField] int m_guarantiedCatch;

    [Header("Balance")]
    [SerializeField] float m_initialBalance;
    [SerializeField] float m_betAmount;

    public void Spin()
    {
        float m_balance = m_initialBalance;

        float totalBets = 0;
        float totalWinnings = 0;

        int guarantiedCatch = m_guarantiedCatch;

        int remainingRounds = m_totalRounds;
        while(remainingRounds > 0 && m_balance >= m_betAmount)
        {
            remainingRounds--;

            Fish coughtFish = SimulateCast(guarantiedCatch < remainingRounds);

            float winnings = 0;

            if(coughtFish != null)
            {
                guarantiedCatch--;
                winnings = m_betAmount * coughtFish.payoutMultiplier;
            }

            m_balance += winnings - m_betAmount;
            totalBets += m_betAmount;
            totalWinnings += winnings;
        }

        float rtp = (totalWinnings / totalBets) * 100;
        Debug.Log($"Initial balance: {m_initialBalance}, final balance: {m_balance}, remaining bet rounds {remainingRounds}, remaining guarantied catch {guarantiedCatch}");
        Debug.Log($"Total Bets: {totalBets}");
        Debug.Log($"Total Winnings: {totalWinnings}");
        Debug.Log($"RTP: {rtp}%");
    }

    Fish SimulateCast(bool canMiss)
    {
        float rnd = Random.value;
        float acc = 0;

        if(!canMiss)
        {
            float totalChance = m_fish.Sum(x => x.catchChance);
            rnd *= totalChance;
        }

        foreach(Fish fish in m_fish)
        {
            if(acc + fish.catchChance >= rnd)
                return fish;

            acc += fish.catchChance;
        }

        return null;
    }

    [System.Serializable]
    class Fish
    {
        public FishTier tier;
        public float catchChance;
        public float payoutMultiplier;
    }
}
