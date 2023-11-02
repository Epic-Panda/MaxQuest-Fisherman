using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    enum FishTier { Low, Medium, High }

    [SerializeField] Fish[] m_fish;

    [Header("RTP")]
    [SerializeField, Range(0, 100)] float m_rtpValue;

    float m_currentRtp;
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
    }

    public void Spin(float betAmount)
    {
        // fish cant be missed if remaining rounds is less than guarantied catch amount
        Fish coughtFish = SimulateCast();

        float winnings = 0;

        if(coughtFish != null)
        {
            if(coughtFish.tier == FishTier.Low)
                m_lowCount++;
            else if(coughtFish.tier == FishTier.Medium)
                m_mediumCount++;
            else
                m_highCount++;

            winnings = betAmount * coughtFish.payoutMultiplier;
        }
        else
            m_missCount++;

        m_totalBets += betAmount;
        m_totalWinnings += winnings;
        m_currentRtp = m_totalWinnings / m_totalBets * 100;
    }

    public void LogCurrentRtp()
    {
        Debug.Log($"Total Bets: {m_totalBets}"
            + $"\nTotal Winnings: {m_totalWinnings}"
            + $"\nRTP: {m_currentRtp}%"
            + $"\nMissed [{m_missCount}], low [{m_lowCount}], medium [{m_mediumCount}], high [{m_highCount}]");
    }

    Fish SimulateCast()
    {
        if(m_currentRtp > m_rtpValue)
            return null;

        float rnd = Random.value * 100;
        float acc = 0;

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
        [Range(0, 100)] public float catchChance;
        public float payoutMultiplier;
    }
}
