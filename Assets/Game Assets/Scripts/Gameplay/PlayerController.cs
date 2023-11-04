using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Animator m_characterAnimator;
    [SerializeField] string m_startFishingTrigger;
    [SerializeField] string m_hookTrigger;

    public void StartFishing()
    {
        m_characterAnimator.SetTrigger(m_startFishingTrigger);
    }

    public void Hook()
    {
        m_characterAnimator.SetTrigger(m_hookTrigger);
    }
}
