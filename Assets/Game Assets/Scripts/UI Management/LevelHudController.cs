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
    [SerializeField] TextMeshProUGUI m_roundText;
    [SerializeField] TextMeshProUGUI m_collectedItemsAmountText;
    [SerializeField] TextMeshProUGUI m_missedItemsAmountText;

    [Header("Collected items handler")]
    [SerializeField] int m_collectedItemsToShow;
    [SerializeField] RectTransform m_collectedItemsContainer;
    [SerializeField] CollectedItemController m_collectedItemPrefab;

    List<CollectedItemController> m_collectedItems;

    public void Setup()
    {
        m_startFishing.onClick.AddListener(GameManager.Instance.StartFishing);
    }

    public void ResetData()
    {
        m_roundText.text = "0";
        m_collectedItemsAmountText.text = "0";
        m_missedItemsAmountText.text = "0";

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

    public void SetRound(int round)
    {
        m_roundText.text = $"{round}";
    }

    public void CollectItem(ItemData item, int collectedItems, int missedItems)
    {
        // update item collection state
        m_collectedItemsAmountText.text = $"{collectedItems}";
        m_missedItemsAmountText.text = $"{missedItems}";

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
