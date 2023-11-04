using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EP.Utils.Core;
using EP.Utils.Editable.UI;

namespace EP.Utils.UI
{
    public class EP_PanelManager : EpSingletone<EP_PanelManager>
    {
        [SerializeField] EP_PanelController[] m_panels;

        readonly List<EP_PanelController> m_openPanels = new();

        public void Setup()
        {
            foreach(EP_PanelController panel in m_panels)
            {
                panel.Setup();
            }
        }

        public void OpenPanel(EP_UIPanelType panelType)
        {
            if(m_openPanels.Exists(x => x.PanelType == panelType))
                return;

            if(m_panels.FirstOrDefault(x => x.PanelType == panelType) is EP_PanelController panel)
            {
                m_openPanels.Add(panel);
                panel.transform.SetAsLastSibling();
                panel.OpenPanel();
            }
        }

        public void ClosePanel()
        {
            if(m_openPanels.Count == 0)
                return;

            m_openPanels[^1].ClosePanel();
            m_openPanels.RemoveAt(m_openPanels.Count - 1);
        }

        public void ClosePanel(EP_UIPanelType panelType)
        {
            int index = m_openPanels.FindIndex(x => x.PanelType == panelType);

            if(index == -1)
                return;

            m_openPanels[index].ClosePanel();
            m_openPanels.RemoveAt(index);
        }

        public void ClosePanel(EP_PanelController panel)
        {
            if(!m_openPanels.Contains(panel))
                return;

            panel.ClosePanel();
            m_openPanels.Remove(panel);
        }
    }
}