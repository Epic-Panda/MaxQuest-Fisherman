using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ScriptableObject
{
    [SerializeField] string m_name;
    [SerializeField] float m_payoutMultiplier;
    [SerializeField] Sprite m_icon;

    public string Name { get { return m_name; } }
    public float PayoutMultiplier { get { return m_payoutMultiplier; } }
    public Sprite Icon { get { return m_icon; } }
}
