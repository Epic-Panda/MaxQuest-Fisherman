using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelHudController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button m_startFishing;

    [Header("General data")]
    [SerializeField] TextMeshProUGUI m_totalAttemptsText;
    [SerializeField] TextMeshProUGUI m_collectedItemsAmountText;

    [Header("Collected items handler")]
    [SerializeField] int m_collectedItemsToShow;
    [SerializeField] RectTransform m_collectedItemsContainer;
    [SerializeField] CollectedItemController m_collectedItemPrefab;

    List<CollectedItemController> m_collectedItems;

    public void Setup()
    {
        ResetData();
        m_startFishing.onClick.AddListener(GameManager.Instance.StartFishing);
    }

    public void ResetData()
    {
        m_totalAttemptsText.text = "0";
        m_collectedItemsAmountText.text = "0";

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
        m_collectedItemsAmountText.text = $"{wins}";
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
}
