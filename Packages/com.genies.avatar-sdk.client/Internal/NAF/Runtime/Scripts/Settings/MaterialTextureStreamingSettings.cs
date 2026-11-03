using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genies.Naf
{
    [Serializable]
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class MaterialTextureStreamingSettings
#else
    public sealed class MaterialTextureStreamingSettings
#endif
    {
        [Tooltip("Allows materials to be created without the textures being loaded yet")]
        public bool enableTextureStreaming = true;

        [Tooltip("If texture streaming is enabled, LODs in this list that are lower quality than the requested will be loaded first, in sequence")]
        public List<int> dynamicLods = new List<int>() { 0, 1, 2 };
    }
}
