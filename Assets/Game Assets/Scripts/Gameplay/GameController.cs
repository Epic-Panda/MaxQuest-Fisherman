using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
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

    int m_totalAttemptWin;
    int m_totalAttempt;

    GameState m_currentState;

    class PlayerHolder
    {
        public PlayerController player;
        public SpawnPoint spawnPoint;
    }

    List<PlayerHolder> m_playerHolder;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager.Instance.CurrentGame = this;

        if(IsClient)
        {
            m_playerHolder = new List<PlayerHolder>();
            m_currentState = GameState.Idle;

            SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);

            foreach(FishController fishController in m_fishController)
            {
                fishController.Setup(m_fishPoolBounds);
            }

            m_slotController.OnSlotStartEvent += SlotController_OnSlotStartEvent;
            m_slotController.OnSlotFinishEvent += SlotController_OnSlotFinishEvent;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer_ServerRPC(ulong clientId)
    {
        SpawnPoint spawnPoint = GetEmptySpawnPoint();

        Quaternion rotation = spawnPoint.inverseX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        spawnPoint.Player = Instantiate(m_playerPrefab, spawnPoint.point.position, rotation);

        if(spawnPoint.Player.TryGetComponent(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.SpawnWithOwnership(clientId);
            playerNetworkObject.TrySetParent(m_playerContainer);
        }
    }

    public void AddPlayer(PlayerController controller)
    {
        m_playerHolder.Add(new PlayerHolder
        {
            player = controller,
            spawnPoint = m_playerSpawnPoints.OrderBy(x => Vector3.Distance(x.point.position, controller.transform.position)).FirstOrDefault()
        });
    }

    SpawnPoint GetEmptySpawnPoint()
    {
        return m_playerSpawnPoints.FirstOrDefault(x => x.Player == null);
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

    void SlotController_OnSlotStartEvent(ulong clientId, bool success, float betValue)
    {
        // bet is placed successfully
        if(success)
        {
            m_playerHolder.Find(x => x.player.OwnerClientId.Equals(clientId))?.player.StartFishing();

            if(NetworkManager.LocalClientId.Equals(clientId))
            {
                m_totalAttempt++;
                UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);

                m_currentState = GameState.InProgress;
            }
        }
        else if(NetworkManager.LocalClientId.Equals(clientId)) // bet placement has failed
        {
            ResourceManager.Instance.AddBalance(betValue);
            m_currentState = GameState.Idle;
        }
    }

    void SlotController_OnSlotFinishEvent(ulong clientId, ItemData item, bool isWin, float winValue)
    {
        StartCoroutine(ResultSimulation());

        IEnumerator ResultSimulation()
        {
            PlayerHolder playerHolder = m_playerHolder.Find(x => x.player.OwnerClientId.Equals(clientId));

            if(playerHolder != null && isWin)
            {
                // try catching the closest fish to the player
                if(m_fishController
                    .Where(x => x.ItemData == item && x.CanCatch)
                    .OrderBy(x => Vector3.Distance(x.SelfTransform.position, playerHolder.spawnPoint.fishCatchPoint.position))
                    .FirstOrDefault(x => x.ItemData == item) is FishController fishController)
                {
                    fishController.MoveToCatchPosition(m_playerSpawnPoints[0].fishCatchPoint.position, .5f);
                    yield return new WaitForSeconds(.5f);
                    StartCoroutine(SimulateFishCatch(fishController));
                }

                if(NetworkManager.LocalClientId.Equals(clientId))
                {
                    m_totalAttemptWin++;
                    ResourceManager.Instance.AddBalance(winValue);
                    UIManager.Instance.LevelHud.UpdateAttempts(m_totalAttempt, m_totalAttemptWin);
                }
            }

            if(playerHolder.player)
                playerHolder.player.Hook();

            if(NetworkManager.LocalClientId.Equals(clientId))
            {
                UIManager.Instance.LevelHud.CollectItem(item);
                m_currentState = GameState.Idle;
            }
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
    class SpawnPoint
    {
        public Transform point;
        public bool inverseX;
        [SerializeField]
        public Transform fishCatchPoint;

        public PlayerController Player { get; set; }
    }
}
