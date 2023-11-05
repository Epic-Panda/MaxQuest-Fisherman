using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] Animator m_characterAnimator;
    [SerializeField] string m_startFishingTrigger;
    [SerializeField] string m_hookTrigger;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameManager.Instance.CurrentGame.AddPlayer(this);
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
