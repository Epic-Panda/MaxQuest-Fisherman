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

    int m_totalAttemptWin;
    int m_totalAttempt;

    public void Setup()
    {
        SpawnPoint spawnPoint = m_playerSpawnPoints[0];

        Quaternion rotation = spawnPoint.inverseX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        m_playerController = Instantiate(m_playerPrefab, spawnPoint.point.position, rotation, m_playerContainer);

        m_slotController.OnSlotStartEvent += SlotController_OnSlotStartEvent;
        m_slotController.OnSlotFinishEvent += SlotController_OnSlotFinishEvent;
    }

    public void StartFishing()
    {
        m_slotController.Spin(m_betAmount);
    }

    public void ResetProgress()
    {
        m_slotController.ResetStats();
        UIManager.Instance.LevelHud.ResetData();
    }

    void SlotController_OnSlotStartEvent(bool success)
    {
        m_playerController.StartFishing();
        m_totalAttempt++;

        UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);
    }

    void SlotController_OnSlotFinishEvent(ItemData item, bool isWin)
    {
        StartCoroutine(SimulateServerWait());

        IEnumerator SimulateServerWait()
        {
            yield return new WaitForSeconds(2);

            if(isWin)
            {
                m_totalAttemptWin++;
                UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);
            }

            m_playerController.Hook();
            UIManager.Instance.LevelHud.CollectItem(item);
        }
    }

    [System.Serializable]
    struct SpawnPoint
    {
        public Transform point;
        public bool inverseX;
    }
}
