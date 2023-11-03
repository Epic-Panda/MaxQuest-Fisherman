using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_SlotMachineSimulation : MonoBehaviour
{
    [SerializeField] float m_rounds;
    [SerializeField] float m_betAmount;
    [Space]
    [SerializeField] Button m_spin;
    [SerializeField] Button m_resetStats;

    void Start()
    {
        m_spin.onClick.AddListener(StartTest);
        m_resetStats.onClick.AddListener(delegate
        {
            SlotMachine.Instance.ResetStats();
            SlotMachine3Of10.Instance.ResetStats();
        });
    }

    void StartTest()
    {
        for(int i = 0; i < m_rounds; i++)
        {
            SlotMachine.Instance.Spin(m_betAmount);
            SlotMachine3Of10.Instance.Spin(m_betAmount);
        }

        SlotMachine.Instance.LogCurrentRtp();
        SlotMachine3Of10.Instance.LogCurrentRtp();
    }
}
