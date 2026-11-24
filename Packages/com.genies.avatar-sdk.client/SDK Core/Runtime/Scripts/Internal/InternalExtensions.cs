using System;
using System.Linq;

namespace Genies.Sdk
{
    internal static class MegaSkinTattooSlotExtensions
    {
        public static GnWrappers.MegaSkinTattooSlot ToInternal(this MegaSkinTattooSlot megaSkinTattooSlot)
        {
            return (GnWrappers.MegaSkinTattooSlot)megaSkinTattooSlot;
        }
    }

    internal static class AvatarLodsExtensions
    {
        /// <summary>
        /// Converts an array of <see cref="AvatarLods"/> to internal LOD indices
        /// (High = 0, Medium = 1, Low = 2). Duplicate values are removed.
        /// </summary>
        /// <param name="lods">The LOD levels to convert.</param>
        /// <param name="sort">When true, sorts the result from lowest to highest quality (Low, Medium, High).</param>
        /// <returns>An array of internal LOD indices, or null if <paramref name="lods"/> is null.</returns>
        internal static int[] ToInternalLods(this AvatarLods[] lods, bool sort = true)
        {
            if (lods == null)
            {
                return null;
            }

            var distinct = lods.Distinct();
            var ordered = sort ? distinct.OrderBy(l => l) : distinct.AsEnumerable();

            return ordered
                .Select(l => l switch
                {
                    AvatarLods.High => 0,
                    AvatarLods.Medium => 1,
                    AvatarLods.Low => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(l), l, null),
                })
                .ToArray();
        }
    }
}
