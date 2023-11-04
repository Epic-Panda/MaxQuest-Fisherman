using UnityEngine;

namespace EP.Utils.Core
{
    public class EpSingletone<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this as T;
        }
    }
}
