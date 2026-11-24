namespace Genies.Sdk
{
    public enum MegaSkinTattooSlot : byte
    {
        LeftTopForearm = GnWrappers.MegaSkinTattooSlot.LeftTopForearm,
        LeftTopOuterArm = GnWrappers.MegaSkinTattooSlot.LeftTopOuterArm,
        RightSideThigh = GnWrappers.MegaSkinTattooSlot.RightSideThigh,
        RightSideAboveTheKnee = GnWrappers.MegaSkinTattooSlot.RightSideAboveTheKnee,
        LeftSideCalf = GnWrappers.MegaSkinTattooSlot.LeftSideCalf,
        LeftSideBelowKnee = GnWrappers.MegaSkinTattooSlot.LeftSideBelowKnee,
        LowerBack = GnWrappers.MegaSkinTattooSlot.LowerBack,
        LowerStomach = GnWrappers.MegaSkinTattooSlot.LowerStomach,
    }

    /// <summary>
    /// Quality tiers for avatar LOD. Currently affects material/texture quality only.
    /// Mesh LOD support will be added in a future update.
    /// </summary>
    public enum AvatarLods
    {
        High = 2048,
        Medium = 1024,
        Low = 512,
    }
}
