using System.Runtime.CompilerServices;

// com.genies.naf.content
[assembly: InternalsVisibleTo("Genies.Naf.Addressables")]

#if GENIES_SDK && !GENIES_INTERNAL
// com.genies.inventory.uidata
[assembly: InternalsVisibleTo("Genies.Inventory.UIData")]
[assembly: InternalsVisibleTo("Genies.Inventory.UIData.Editor")]
// com.genies.avatars.sdk
[assembly: InternalsVisibleTo("Genies.Avatars.Sdk")]
[assembly: InternalsVisibleTo("Genies.Avatars.Sdk.Sample")]
[assembly: InternalsVisibleTo("Genies.Avatars.Sdk.Editor")]
// com.genies.sdk.avatareditor
[assembly: InternalsVisibleTo("Genies.Sdk.AvatarEditor")]
// com.genies.megaeditor
[assembly: InternalsVisibleTo("Genies.MegaEditor")]
[assembly: InternalsVisibleTo("SilverStudioLookDemo")]
[assembly: InternalsVisibleTo("SilverStudioBase")]
[assembly: InternalsVisibleTo("SilverStudioAvatarDemo")]
// com.genies.avatars.behaviors
[assembly: InternalsVisibleTo("Genies.Avatars.Behaviors")]
// com.genies.avatars.customization
[assembly: InternalsVisibleTo("Genies.Avatars.Customization")]
#endif
