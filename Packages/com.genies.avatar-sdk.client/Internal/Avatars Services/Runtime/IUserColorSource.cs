using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Genies.Avatars.Services
{
    /// Type of user color preset to retrieve.
    public enum IColorType
    {
        Hair,           // Regular hair colors
        Eyebrow,        // Eyebrow colors
        Eyelash,        // Eyelash colors
        FacialHair,     // Facial hair colors
        Skin            // Skin color
    }
    /// <summary>
    /// Provides user (custom) colors across categories (hair, eyebrow, eyelash, etc.).
    /// Can be implemented by the Avatar Editor SDK service (Inventory/DefaultInventoryService)
    /// or by legacy services (e.g. Game Feature cloud save). Used by HairColorService and FlairColorItemPickerDataSource.
    /// </summary>
    public interface IUserColorSource
    {
        /// <summary>
        /// Gets all user custom colors for the given category.
        /// </summary>
        /// <param name="colorType">colorType key: "hair", "facialhair", "skin", "eyebrow", "eyelash"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of entries (id + colors). Colors are 4 elements (base, r, g, b) for gradient UI.</returns>
        UniTask<List<UserColorEntry>> GetUserColorsAsync(IColorType colorType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single user color by instance ID. May search across categories if category is unknown.
        /// </summary>
        /// <param name="instanceId">The custom color instance ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The entry if found; otherwise null.</returns>
        UniTask<UserColorEntry?> GetUserColorByIdAsync(string instanceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user color in the given category.
        /// </summary>
        /// <param name="colorType">colorType key: "hair", "facialhair", "skin", "eyebrow", "eyelash"/>.</param>
        /// <param name="colors">Color values (e.g. 4 for hair/flair gradient: base, r, g, b).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created entry with Id and Colors; null if creation failed.</returns>
        UniTask<UserColorEntry?> CreateUserColorAsync(IColorType colorType, List<UnityEngine.Color> colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user color by instance ID.
        /// </summary>
        /// <param name="instanceId">The custom color instance ID.</param>
        /// <param name="colors">New color values (e.g. 4 for gradient).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        UniTask UpdateUserColorAsync(string instanceId, List<UnityEngine.Color> colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user color by instance ID.
        /// </summary>
        /// <param name="instanceId">The custom color instance ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        UniTask DeleteUserColorAsync(string instanceId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A single user color entry (instance id + gradient colors for UI).
    /// </summary>
    public struct UserColorEntry
    {
        public string Id { get; set; }
        public Color[] Colors { get; set; }
    }
}
