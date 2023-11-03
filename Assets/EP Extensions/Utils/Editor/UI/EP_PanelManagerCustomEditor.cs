using UnityEditor;
using UnityEngine;

using EP.Utils.Editable.UI;
using EP.Utils.UI;

namespace EP.Utils.Editor.UI
{
    [CustomEditor(typeof(EP_PanelManager))]
    public class EP_PanelManagerCustomEditor : UnityEditor.Editor
    {
        EP_UIPanelType m_panelType;

        public override void OnInspectorGUI()
        {
            EP_PanelManager panelManager = (EP_PanelManager)target;

            base.OnInspectorGUI();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                {
                    m_panelType = (EP_UIPanelType)EditorGUILayout.EnumPopup(m_panelType);

                    GUILayout.FlexibleSpace();

                    if(GUILayout.Button("Open panel"))
                        panelManager.OpenPanel(m_panelType);

                    if(GUILayout.Button("Close panel"))
                        panelManager.ClosePanel(m_panelType);
                }
                GUILayout.EndHorizontal();

                if(GUILayout.Button("Close last opened panel"))
                {
                    panelManager.ClosePanel();
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}