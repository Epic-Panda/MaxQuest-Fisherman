using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] SlotMachineController m_slotController;

    public void Spin(float betAmount)
    {
        m_slotController.Spin(betAmount);
    }

    public void ResetProgress()
    {
        m_slotController.ResetStats();
        UIManager.Instance.LevelHud.ResetData();
    }
}
