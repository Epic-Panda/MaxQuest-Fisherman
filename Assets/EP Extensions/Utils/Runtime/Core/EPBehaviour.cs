using System.Collections.Generic;
using UnityEngine;

namespace EP.Utils.Core
{
    public class EPBehaviour : MonoBehaviour
    {
        public List<EPScriptableTag> tags;

        private Transform _transform;
        public Transform SelfTransform
        {
            get
            {
                if(_transform == null)
                    _transform = transform;

                return _transform;
            }
        }
    }
}
