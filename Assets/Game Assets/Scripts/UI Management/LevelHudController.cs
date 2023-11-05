using System;
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

    [Header("Player data switch")]
    [SerializeField] Toggle m_myData;
    [SerializeField] Toggle m_otherPlayerData;

    List<CollectedItemController> m_collectedItems;

    List<ItemData> m_myItems;
    List<ItemData> m_otherPlayerItems;

    bool m_isMyDataVisible;

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

        m_myData.onValueChanged.AddListener(MyDataToggle_OnValueChange);
        m_otherPlayerData.onValueChanged.AddListener(OtherPlayerDataToggle_OnValueChange);
    }

    private void OtherPlayerDataToggle_OnValueChange(bool isOn)
    {
        if(isOn)
        {
            ShowMyItems(false);
        }
    }

    private void MyDataToggle_OnValueChange(bool isOn)
    {
        if(isOn)
        {
            ShowMyItems(true);
        }
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

        m_myItems = new List<ItemData>();
        m_otherPlayerItems = new List<ItemData>();

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

    void ShowMyItems(bool showMyItems)
    {
        List<ItemData> items = showMyItems ? m_myItems : m_otherPlayerItems;

        for(int i = 1; i <= items.Count; i++)
        {
            if(m_collectedItems.Count - i < 0)
            {
                CollectedItemController collectedItem = Instantiate(m_collectedItemPrefab, m_collectedItemsContainer);
                m_collectedItems.Insert(0, collectedItem);
            }

            m_collectedItems[^i].SetCollectedItem(items[^i]);
        }

        int diff = m_collectedItems.Count - items.Count;

        for(int i = 0; i < diff; i++)
        {
            m_collectedItems[i].gameObject.SetActive(false);
        }
    }

    public void CollectItem(ItemData item, bool isSecondPlayer = false)
    {
        List<ItemData> collectedItems = isSecondPlayer ? m_otherPlayerItems : m_myItems;

        if(collectedItems.Count == m_collectedItemsToShow)
            collectedItems.RemoveAt(0);

        collectedItems.Add(item);


        if(isSecondPlayer && m_isMyDataVisible || !isSecondPlayer && !m_isMyDataVisible)
            return;

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
