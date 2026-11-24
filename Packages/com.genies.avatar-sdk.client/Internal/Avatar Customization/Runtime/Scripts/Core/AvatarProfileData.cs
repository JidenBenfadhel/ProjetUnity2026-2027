// AvatarProfileData.cs - Avatar profile data model
using System;
using Genies.Naf;
using UnityEngine;

namespace Genies.Avatars.Customization
{
[Serializable]
internal class AvatarProfileData
    {
        public Genies.Naf.AvatarDefinition Definition;
        public string HeadshotPath;
        public GameObject AvatarGameObject;
    }
}

