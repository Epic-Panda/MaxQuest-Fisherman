using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;

public class UIManager : EpSingletone<UIManager>
{
    [SerializeField] LevelHudController m_levelHud;

    public LevelHudController LevelHud { get { return m_levelHud; } }

    private void Start()
    {
        m_levelHud.Setup();

        m_levelHud.ResetData();
    }
}
