using System.Runtime.CompilerServices;

// com.genies.inventory.uidata
[assembly: InternalsVisibleTo("Genies.Inventory.UIData.Editor")]

#if GENIES_SDK && !GENIES_INTERNAL
// com.genies.sdk.avatareditor
[assembly: InternalsVisibleTo("Genies.Sdk.AvatarEditor")]
// com.genies.megaeditor
[assembly: InternalsVisibleTo("Genies.MegaEditor")]
[assembly: InternalsVisibleTo("SilverStudioLookDemo")]
[assembly: InternalsVisibleTo("SilverStudioBase")]
[assembly: InternalsVisibleTo("SilverStudioAvatarDemo")]
// com.genies.avatars.customization
[assembly: InternalsVisibleTo("Genies.Avatars.Customization")]
#endif
