using UnityEngine;

namespace EP.Utils.Core
{
    [CreateAssetMenu(menuName = "EP Extensions/Scriptable Architecture/Tag", order = 100)]
    public class EPScriptableTag : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        [SerializeField] string _developerDescription;
#endif
    }
}
