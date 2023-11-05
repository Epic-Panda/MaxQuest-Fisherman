using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFPS : MonoBehaviour
{
    [SerializeField] int m_targetFPS = 60;

    void Awake()
    {
#if IS_SERVER
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = m_targetFPS;
#endif
    }
}
