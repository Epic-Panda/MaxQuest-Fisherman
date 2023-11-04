using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolatilityData : ScriptableObject
{
    [Header("RTP")]
    [SerializeField, Range(0, 100)] float m_winChance;

    [Header("Guarantied win data")]
    [SerializeField] int m_guarantiedWinInRound;
    [SerializeField] int m_guarantiedWin;

    [Header("Items data")]
    [SerializeField] ItemData m_missItem;
    [SerializeField] ItemsVolatility[] m_winItems;

    public float WinChance { get { return m_winChance; } }
    public int GuarantiedWinInRound { get { return m_guarantiedWinInRound; } }
    public int GuarantiedWin { get { return m_guarantiedWin; } }

    public ItemData MissItem { get { return m_missItem; } }
    public ItemsVolatility[] WinItems { get { return m_winItems; } }

    [System.Serializable]
    public struct ItemsVolatility
    {
        public ItemData itemData;
        public float collectChance;
    }
}
