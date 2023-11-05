using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] Animator m_characterAnimator;
    [SerializeField] string m_startFishingTrigger;
    [SerializeField] string m_hookTrigger;

    [Header("Player indicator")]
    [SerializeField] RectTransform m_canvas;
    [SerializeField] GameObject m_youText;
    [SerializeField] GameObject m_otherPlayerText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager.Instance.CurrentGame.AddPlayer(this);
        m_canvas.rotation = Quaternion.identity;

        m_youText.SetActive(IsOwner);
        m_otherPlayerText.SetActive(!IsOwner);
    }

    public void StartFishing()
    {
        m_characterAnimator.SetTrigger(m_startFishingTrigger);
    }

    public void Hook()
    {
        m_characterAnimator.SetTrigger(m_hookTrigger);
    }
}
