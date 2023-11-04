using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectedItemController : MonoBehaviour
{
    [SerializeField] Image m_icon;
    [SerializeField] TextMeshProUGUI m_itemName;

    public void SetCollectedItem(ItemData item)
    {
        if(item == null)
            return;

        m_icon.sprite = item.Icon;
        m_itemName.text = item.Name;
    }
}
