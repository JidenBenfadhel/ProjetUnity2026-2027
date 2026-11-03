using System.Runtime.CompilerServices;

// com.genies.ugc
[assembly: InternalsVisibleTo("Genies.Ugc.Editor.Tests")]

#if GENIES_SDK && !GENIES_INTERNAL
// com.genies.wearable
[assembly: InternalsVisibleTo("Genies.Wearables")]
// com.genies.avatars.context
[assembly: InternalsVisibleTo("Genies.Avatars.Context")]
// com.genies.sdk.avatareditor
[assembly: InternalsVisibleTo("Genies.Sdk.AvatarEditor")]
// com.genies.megaeditor
[assembly: InternalsVisibleTo("Genies.MegaEditor")]
[assembly: InternalsVisibleTo("SilverStudioLookDemo")]
[assembly: InternalsVisibleTo("SilverStudioBase")]
[assembly: InternalsVisibleTo("SilverStudioAvatarDemo")]
// com.genies.avatars.customization
[assembly: InternalsVisibleTo("Genies.Avatars.Customization")]
// com.genies.avatars.sdk
[assembly: InternalsVisibleTo("Genies.Avatars.Sdk")]
#endif
