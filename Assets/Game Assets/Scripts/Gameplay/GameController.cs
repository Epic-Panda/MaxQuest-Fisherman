using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GameController : MonoBehaviour
{
    enum GameState { Idle, Waiting, InProgress }

    [Header("Prefabs")]
    [SerializeField] PlayerController m_playerPrefab;

    [Header("Default")]
    [SerializeField] SlotMachineController m_slotController;

    [Header("Fish positioning")]
    [SerializeField] float m_fishRespawn;
    [SerializeField] float m_fishCatchDelay;
    [SerializeField] Bounds m_fishPoolBounds;
    [SerializeField] FishController[] m_fishController;

    [Header("Player positioning")]
    [SerializeField] Transform m_playerContainer;
    [SerializeField] SpawnPoint[] m_playerSpawnPoints;

    PlayerController m_playerController;

    int m_totalAttemptWin;
    int m_totalAttempt;

    GameState m_currentState;

    public void Setup()
    {
        m_currentState = GameState.Idle;

        SpawnPoint spawnPoint = m_playerSpawnPoints[0];

        Quaternion rotation = spawnPoint.inverseX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        m_playerController = Instantiate(m_playerPrefab, spawnPoint.point.position, rotation, m_playerContainer);

        foreach(FishController fishController in m_fishController)
        {
            fishController.Setup(m_fishPoolBounds);
        }

        m_slotController.OnSlotStartEvent += SlotController_OnSlotStartEvent;
        m_slotController.OnSlotFinishEvent += SlotController_OnSlotFinishEvent;
    }

    public void StartFishing(float betValue)
    {
        if(m_currentState != GameState.Idle)
            return;

        if(ResourceManager.Instance.TryRemoveBalance(betValue))
        {
            m_currentState = GameState.Waiting;
            m_slotController.Spin(betValue);
        }
    }

    public void ResetProgress()
    {
        m_slotController.ResetStats();
        UIManager.Instance.LevelHud.ResetData();
    }

    void SlotController_OnSlotStartEvent(bool success, float betValue)
    {
        // bet is placed successfully
        if(success)
        {
            m_playerController.StartFishing();
            m_totalAttempt++;

            UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);

            m_currentState = GameState.InProgress;
        }
        else // bet placement has failed
        {
            ResourceManager.Instance.AddBalance(betValue);
            m_currentState = GameState.Idle;
        }
    }

    void SlotController_OnSlotFinishEvent(ItemData item, bool isWin, float winValue)
    {
        StartCoroutine(ResultSimulation());

        IEnumerator ResultSimulation()
        {
            if(isWin)
            {
                // try catching the closest fish to the player
                if(m_fishController
                    .Where(x => x.ItemData == item)
                    .OrderBy(x => Vector3.Distance(x.SelfTransform.position, m_playerSpawnPoints[0].fishCatchPoint.position))
                    .FirstOrDefault(x => x.ItemData == item) is FishController fishController)
                {
                    fishController.MoveToPosition(m_playerSpawnPoints[0].fishCatchPoint.position, .5f);
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(SimulateFishCatch(fishController));
                }

                m_totalAttemptWin++;
                ResourceManager.Instance.AddBalance(winValue);
                UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);
            }

            m_playerController.Hook();
            UIManager.Instance.LevelHud.CollectItem(item);

            m_currentState = GameState.Idle;
        }
    }

    IEnumerator SimulateFishCatch(FishController fishController)
    {
        yield return new WaitForSeconds(m_fishCatchDelay);
        fishController.gameObject.SetActive(false);
        yield return new WaitForSeconds(m_fishRespawn);
        fishController.Respawn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_fishPoolBounds.center, m_fishPoolBounds.size);
        Gizmos.color = Color.white;
    }

    [System.Serializable]
    struct SpawnPoint
    {
        public Transform point;
        public bool inverseX;
        [SerializeField]
        public Transform fishCatchPoint;
    }
}
