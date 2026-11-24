using System.Runtime.CompilerServices;

// com.genies.customizer
[assembly: InternalsVisibleTo("Genies.Customizer.Editor")]

#if GENIES_SDK && !GENIES_INTERNAL
// com.genies.sdk.avatareditor
[assembly: InternalsVisibleTo("Genies.Sdk.AvatarEditor")]
// com.genies.inventory.uidata
[assembly: InternalsVisibleTo("Genies.Inventory.UIData")]
[assembly: InternalsVisibleTo("Genies.Inventory.UIData.Editor")]
// com.genies.megaeditor
[assembly: InternalsVisibleTo("Genies.MegaEditor")]
[assembly: InternalsVisibleTo("SilverStudioLookDemo")]
[assembly: InternalsVisibleTo("SilverStudioBase")]
[assembly: InternalsVisibleTo("SilverStudioAvatarDemo")]
// com.genies.avatars.customization
[assembly: InternalsVisibleTo("Genies.Avatars.Customization")]
[assembly: InternalsVisibleTo("Genies.Looks.Customization.Commands")]
#endif
