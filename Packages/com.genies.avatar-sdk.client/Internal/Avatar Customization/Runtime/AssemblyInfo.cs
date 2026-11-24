using System.Runtime.CompilerServices;

// com.genies.avatars.customization
[assembly: InternalsVisibleTo("Genies.Avatars.Customization")]

// com.genies.sdk.avatar.telemetry
[assembly: InternalsVisibleTo("Genies.Sdk.Avatar.Telemetry")]

// com.genies.sdk.avatareditor
[assembly: InternalsVisibleTo("Genies.Sdk.AvatarEditor")]

// com.genies.sdk.avatar
[assembly: InternalsVisibleTo("Genies.Sdk.Avatar")]

#if GENIES_SDK && !GENIES_INTERNAL
// com.genies.megaeditor
[assembly: InternalsVisibleTo("Genies.MegaEditor")]
[assembly: InternalsVisibleTo("SilverStudioLookDemo")]
[assembly: InternalsVisibleTo("SilverStudioBase")]
[assembly: InternalsVisibleTo("SilverStudioAvatarDemo")]
// com.genies.megaeditor.ugc
[assembly: InternalsVisibleTo("Genies.Customization.MegaEditor")]
#endif
