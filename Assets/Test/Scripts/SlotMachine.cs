using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    enum FishTier { Low, Medium, High }

    [SerializeField] Fish[] m_fish;

    [Header("Catch chance")]
    [SerializeField, Range(0, 100)] float m_catchChance;
    [SerializeField] int m_guarantiedCatchRounds;
    [SerializeField] int m_guarantiedCatch;

    int m_remainingRounds;
    int m_remainingCatch;

    float m_totalBets;
    float m_totalWinnings;

    int m_lowCount;
    int m_mediumCount;
    int m_highCount;
    int m_missCount;

    public static SlotMachine Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        LogChance();
        ResetStats();
    }

    public void ResetStats()
    {
        m_lowCount = 0;
        m_mediumCount = 0;
        m_highCount = 0;
        m_missCount = 0;

        m_totalBets = 0;
        m_totalWinnings = 0;

        m_remainingRounds = m_guarantiedCatchRounds;
        m_remainingCatch = m_guarantiedCatch;
    }

    public void Spin(float betAmount)
    {
        m_remainingRounds--;

        // fish cant be missed if remaining rounds is less than guarantied catch amount
        Fish coughtFish = SimulateCast(m_remainingCatch <= m_remainingRounds);

        float winnings = 0;

        if(coughtFish != null)
        {
            if(coughtFish.tier == FishTier.Low)
                m_lowCount++;
            else if(coughtFish.tier == FishTier.Medium)
                m_mediumCount++;
            else
                m_highCount++;

            m_remainingCatch--;
            winnings = betAmount * coughtFish.payoutMultiplier;
        }
        else
            m_missCount++;

        m_totalBets += betAmount;
        m_totalWinnings += winnings;

        if(m_remainingRounds == 0)
        {
            m_remainingRounds = m_guarantiedCatchRounds;
            m_remainingCatch = m_guarantiedCatch;
        }
    }

    public void LogCurrentRtp()
    {
        float rtp = m_totalWinnings / m_totalBets * 100;
        Debug.Log($"{nameof(SlotMachine)} Total Bets: {m_totalBets}"
            + $"\nTotal Winnings: {m_totalWinnings}"
            + $"\nMissed [{m_missCount}], win count [{m_lowCount + m_mediumCount + m_highCount}]");
    }

    public void LogChance()
    {
        foreach(Fish fish in m_fish)
        {
            Debug.Log($"Fish {fish.tier} - {fish.catchChance * m_catchChance / 100.0f}");
        }
        Debug.Log($"Miss chance - {100 - m_catchChance}");
    }

    Fish SimulateCast(bool canMiss)
    {
        float winChance = canMiss ? m_catchChance / 100.0f : 1;

        float rnd = Random.value * 100;
        float acc = 0;

        foreach(Fish fish in m_fish)
        {
            acc += fish.catchChance * winChance;

            if(acc >= rnd)
                return fish;
        }

        return null;
    }

    [System.Serializable]
    class Fish
    {
        public FishTier tier;
        [Range(0, 100)] public float catchChance;
        public float payoutMultiplier;
    }
}
