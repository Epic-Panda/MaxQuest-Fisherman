using UnityEngine;

using EP.Utils.Core;

public class ResourceManager : EpSingletone<ResourceManager>
{
    [SerializeField] float m_initialBalance;

    float m_currentBalance;

    public static event System.Action<float> OnBalanceValueChangeEvent;

    public void Setup()
    {
        m_currentBalance = m_initialBalance;
        OnBalanceValueChangeEvent?.Invoke(m_currentBalance);
    }

    public void AddBalance(float amount)
    {
        m_currentBalance += amount;
        OnBalanceValueChangeEvent?.Invoke(m_currentBalance);
    }

    public bool TryRemoveBalance(float amount)
    {
        if(m_currentBalance >= amount)
        {
            m_currentBalance -= amount;

            OnBalanceValueChangeEvent?.Invoke(m_currentBalance);
            return true;
        }

        return false;
    }
}
