using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotMachine3Of10 : MonoBehaviour
{
    enum FishTier { Low, Medium, High, None }

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

    List<FishTier> m_collected;

    public static SlotMachine3Of10 Instance { get; private set; }

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
        m_collected = new List<FishTier>();
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
        if(m_collected.Count == m_guarantiedCatchRounds)
            m_collected.RemoveAt(0);

        m_remainingRounds--;

        int remainingFishToCatch = m_guarantiedCatch - m_collected.Count(x => x != FishTier.None);
        int remainingRounds = m_guarantiedCatchRounds - m_collected.Count;

        // fish cant be missed if remaining rounds is less than guarantied catch amount
        Fish coughtFish = SimulateCast(remainingFishToCatch < remainingRounds);

        float winnings = 0;

        if(coughtFish != null)
        {
            if(coughtFish.tier == FishTier.Low)
                m_lowCount++;
            else if(coughtFish.tier == FishTier.Medium)
                m_mediumCount++;
            else
                m_highCount++;

            m_collected.Add(coughtFish.tier);

            m_remainingCatch--;
            winnings = betAmount * coughtFish.payoutMultiplier;
        }
        else
        {
            m_collected.Add(FishTier.None);
            m_missCount++;
        }

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
        string catchStr = "";
        foreach(var res in m_collected)
        {
            catchStr += $"[{res}] ";
        }

        Debug.Log($"{nameof(SlotMachine3Of10)} Total Bets: {m_totalBets}"
           + $"\nTotal Winnings: {m_totalWinnings}"
           + $"\nMissed [{m_missCount}], win count [{m_lowCount + m_mediumCount + m_highCount}]"
           + $"\ncatch {catchStr}");
    }

    Fish SimulateCast(bool canMiss)
    {
        if(canMiss && Random.value * 100 > m_catchChance)
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
