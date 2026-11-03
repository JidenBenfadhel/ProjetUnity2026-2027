#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genies.Sdk
{
    public class SamplePrefabTracker : MonoBehaviour
    {
        [SerializeField] private string _prefabName;
        public static event Action<string> SamplePrefabUsed = delegate { };

        // Static variable to prevent multiple of the same prefab being recorded
        private static readonly HashSet<string> ReportedThisSession = new HashSet<string>();

        private void Start()
        {
            if (string.IsNullOrEmpty(_prefabName))
            {
                _prefabName = gameObject.name;
            }

            if (!ReportedThisSession.Add(_prefabName))
            {
                return;
            }

            SamplePrefabUsed.Invoke(_prefabName);
        }

        // Track prefab usage every time we enter playmode
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ReportedThisSession.Clear();
        }
    }
}
#endif
