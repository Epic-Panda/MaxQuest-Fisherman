using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelHudController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] BetButton[] m_betButton;
    [SerializeField] Button m_leaveLevel;
    [SerializeField] BalanceButton m_balanceButton;

    [Header("General data")]
    [SerializeField] TextMeshProUGUI m_totalRoundsText;
    [SerializeField] TextMeshProUGUI m_totalRoundsWonText;

    [Header("Player data")]
    [SerializeField] TextMeshProUGUI m_totalAttemptsText;
    [SerializeField] TextMeshProUGUI m_wonAttemptsText;
    [SerializeField] TextMeshProUGUI m_balanceText;

    [Header("Collected items handler")]
    [SerializeField] int m_collectedItemsToShow;
    [SerializeField] RectTransform m_collectedItemsContainer;
    [SerializeField] CollectedItemController m_collectedItemPrefab;

    List<CollectedItemController> m_collectedItems;

    public void Setup()
    {
        ResetData();

        m_leaveLevel.onClick.AddListener(GameManager.Instance.StopGame);

        SlotMachineController.OnTotalDataUpdateEvent += SlotMachineController_OnTotalDataUpdateEvent;

        m_balanceButton.buttonText.text = $"Add ${m_balanceButton.balanceAmount}";
        m_balanceButton.button.onClick.AddListener(delegate { ResourceManager.Instance.AddBalance(m_balanceButton.balanceAmount); });

        foreach(BetButton betButton in m_betButton)
        {
            betButton.betButtonText.text = $"${betButton.betValue}";
            betButton.betButton.onClick.AddListener(delegate
            {
                GameManager.Instance.StartFishing(betButton.betValue);
            });
        }

        ResourceManager.OnBalanceValueChangeEvent += ResourceManager_OnBalanceValueChangeEvent;
    }

    void SlotMachineController_OnTotalDataUpdateEvent(int totalRounds, int totalRoundsWon)
    {
        m_totalRoundsText.text = $"{totalRounds}";
        m_totalRoundsWonText.text = $"{totalRoundsWon}";
    }

    public void Show(bool show = true)
    {
        gameObject.SetActive(show);
    }

    void ResourceManager_OnBalanceValueChangeEvent(float balance)
    {
        m_balanceText.text = $"${balance}";
    }

    public void ResetData()
    {
        m_totalRoundsText.text = "0";
        m_totalRoundsWonText.text = "0";

        m_totalAttemptsText.text = "0";
        m_wonAttemptsText.text = "0";

        if(m_collectedItems == null)
        {
            m_collectedItems = new List<CollectedItemController>();
        }
        else
        {
            foreach(CollectedItemController controller in m_collectedItems)
            {
                controller.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateAttempts(int total, int wins)
    {
        m_totalAttemptsText.text = $"{total}";
        m_wonAttemptsText.text = $"{wins}";
    }

    public void CollectItem(ItemData item)
    {
        CollectedItemController collectedItem;

        // instantiate new collected item controller
        if(m_collectedItems.Count < m_collectedItemsToShow)
        {
            collectedItem = Instantiate(m_collectedItemPrefab, m_collectedItemsContainer);
        }
        else // replace the first collected item with the new one
        {
            collectedItem = m_collectedItems[0];
            m_collectedItems.RemoveAt(0);

            collectedItem.gameObject.SetActive(true);
        }

        m_collectedItems.Add(collectedItem);

        // reorder items
        collectedItem.SetCollectedItem(item);
        collectedItem.transform.SetAsFirstSibling();
    }

    [System.Serializable]
    struct BetButton
    {
        public Button betButton;
        public TextMeshProUGUI betButtonText;
        public float betValue;
    }

    [System.Serializable]
    struct BalanceButton
    {
        public Button button;
        public TextMeshProUGUI buttonText;
        public float balanceAmount;
    }
}
