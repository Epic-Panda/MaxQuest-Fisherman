using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] float m_betAmount;

    [Header("Prefabs")]
    [SerializeField] PlayerController m_playerPrefab;

    [Header("Other")]
    [SerializeField] SlotMachineController m_slotController;
    [SerializeField] FishController[] m_fishController;

    [Header("Positioning")]
    [SerializeField] Transform m_playerContainer;
    [SerializeField] SpawnPoint[] m_playerSpawnPoints;

    PlayerController m_playerController;

    int m_winCount;
    int m_loseCount;

    public void Setup()
    {
        SpawnPoint spawnPoint = m_playerSpawnPoints[0];

        Quaternion rotation = spawnPoint.inverseX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        m_playerController = Instantiate(m_playerPrefab, spawnPoint.point.position, rotation, m_playerContainer);

        m_slotController.OnSlotCatchCompleteEvent += SlotController_OnSlotCatchCompleteEvent;
    }

    public void StartFishing()
    {
        m_playerController.StartFishing();
        m_slotController.Spin(m_betAmount);
    }

    void SlotController_OnSlotCatchCompleteEvent(ItemData item, bool isWin)
    {
        if(isWin)
            m_winCount++;
        else
            m_loseCount++;

        StartCoroutine(SimulateServerWait());

        IEnumerator SimulateServerWait()
        {
            yield return new WaitForSeconds(2);
            m_playerController.Hook();
            UIManager.Instance.LevelHud.CollectItem(item, m_winCount, m_loseCount);
        }
    }

    public void ResetProgress()
    {
        m_slotController.ResetStats();
        UIManager.Instance.LevelHud.ResetData();
    }

    [System.Serializable]
    struct SpawnPoint
    {
        public Transform point;
        public bool inverseX;
    }
}
