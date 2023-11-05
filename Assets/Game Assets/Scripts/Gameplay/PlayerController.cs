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

    // sync animation fishing state for newly spawned players
    NetworkVariable<bool> m_isFishing = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager.Instance.CurrentGame.AddPlayer(this);
        m_canvas.rotation = Quaternion.identity;

        m_youText.SetActive(IsOwner);
        m_otherPlayerText.SetActive(!IsOwner);

        if(!IsOwner)
        {
            if(m_isFishing.Value)
                StartFishing();
        }
    }

    public void StartFishing()
    {
        if(IsOwner)
            m_isFishing.Value = true;

        m_characterAnimator.SetTrigger(m_startFishingTrigger);
    }

    public void Hook()
    {
        if(IsOwner)
            m_isFishing.Value = false;

        m_characterAnimator.SetTrigger(m_hookTrigger);
    }
}
