using System;
using System.Collections.Generic;
using Genies.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Genies.Sdk.Samples.DebugSdkFunctions
{
    /// <summary>
    /// Unified Color API category: Set Custom Color for different categories
    /// </summary>
    internal class SdkEditorDebugCustomColorMethods : MonoBehaviour
    {
        [Header("Avatar Debug Options")]
        [SerializeField] private ManagedAvatarComponent _avatarToDebug;

        public void SetAvatarToDebug(ManagedAvatarComponent c) { _avatarToDebug = c; }

        private ManagedAvatarComponent AvatarToDebug => _avatarToDebug;

        [Header("Set Custom Skin Color on Avatar")]
        [SerializeField] private Color _skinColor = Color.white;

        [Header("Set Custom Color on Avatar for the given Makeup Category ")]
        [SerializeField] private AvatarMakeupCategory _setMakeupCategory = AvatarMakeupCategory.Lipstick;
        [SerializeField] private Color _makeupBaseColor = Color.white;
        [SerializeField] private Color _makeupColorR = Color.white;
        [SerializeField] private Color _makeupColorG = Color.white;
        [SerializeField] private Color _makeupColorB = Color.white;

        [Header("Set Custom Color on Avatar for the given Hair Type")]
        [SerializeField] private HairType _hairType = HairType.Hair;
        [SerializeField] private Color _hairBaseColor = Color.white;
        [SerializeField] private Color _hairColorR = Color.white;
        [SerializeField] private Color _hairColorG = Color.white;
        [SerializeField] private Color _hairColorB = Color.white;


        [InspectorButton("===== Custom Color API =====", InspectorButtonAttribute.ExecutionMode.EditMode)]
        private void HeaderCustomColorAPI() { }

        [InspectorButton("\nSet Custom Skin Color on Avatar\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void SetColorSkin()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Custom Skin Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var skinColor = AvatarSdk.CreateSkinColor(_skinColor);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, skinColor);

                if (success)
                {
                    var message = $"Successfully set custom skin color on Avatar:\nColor: {_skinColor}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set Custom Skin Color", "Failed to set skin color. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set skin color: {ex.Message}");
                ShowPopUp("⚠️ Set Custom Skin Color", $"Error: {ex.Message}");
            }
        }

        [InspectorButton("\nSet Custom Makeup Color By Makeup Category\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void SetColorMakeupTest()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Custom Makeup Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var makeupColor =  AvatarSdk.CreateMakeupColor(_setMakeupCategory, _makeupBaseColor, _makeupColorR, _makeupColorG, _makeupColorB);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, makeupColor);

                if (success)
                {
                    var message = $"Successfully set makeup color using SetColorAsync:\n" +
                                  $"Category: {_setMakeupCategory}\n" +
                                  $"Base: {_makeupBaseColor}\n" +
                                  $"R: {_makeupColorR}, G: {_makeupColorG}, B: {_makeupColorB}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set Custom Makeup Color", "SetColorAsync returned false. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set makeup color: {ex.Message}");
                ShowPopUp("⚠️ Set Custom Makeup Color", $"Error: {ex.Message}");
            }
        }

        [InspectorButton("\nSet Custom Hair Color By Hair Type\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private void SetHairColorByHairType()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Custom Hair Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            switch (_hairType)
            {
                case HairType.Hair:
                    SetHairColor();
                    break;
                case HairType.FacialHair:
                    SetFacialHairColor();
                    break;
                case HairType.Eyebrows:
                    SetEyebrowColor();
                    break;
                case HairType.Eyelashes:
                    SetEyelashesColor();
                    break;
            }
        }

        private async void SetHairColor()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Hair Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var hairColor = AvatarSdk.CreateHairColor(_hairBaseColor, _hairColorR, _hairColorG, _hairColorB);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, hairColor);
                if (success)
                {
                    var message = $"Successfully set {_hairType} color using SetColorAsync:\n" +
                                  $"Base: {_hairBaseColor}\n" +
                                  $"R: {_hairColorR}\n" +
                                  $"G: {_hairColorG}\n" +
                                  $"B: {_hairColorB}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set Hair Color", "Failed to set hair color. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set hair color: {ex.Message}");
                ShowPopUp("⚠️ Set Hair Color", $"Error: {ex.Message}");
            }
        }

        private async void SetFacialHairColor()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Facial Hair Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var hairColor = AvatarSdk.CreateFacialHairColor(_hairBaseColor, _hairColorR, _hairColorG, _hairColorB);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, hairColor);
                if (success)
                {
                    var message = $"Successfully set {_hairType} color using SetColorAsync:\n" +
                                  $"Base: {_hairBaseColor}\n" +
                                  $"R: {_hairColorR}\n" +
                                  $"G: {_hairColorG}\n" +
                                  $"B: {_hairColorB}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set Facial Hair Color", "Failed to set hair color. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set hair color: {ex.Message}");
                ShowPopUp("⚠️ Set Facial Hair Color", $"Error: {ex.Message}");
            }
        }

        private async void SetEyebrowColor()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Eyebrows Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var eyeBrowsColor = AvatarSdk.CreateEyeBrowsColor(_hairBaseColor, _hairColorR);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, eyeBrowsColor);

                if (success)
                {
                    var message = $"Successfully set eyebrows color using SetColorAsync:\n" +
                                  $"Base: {_hairBaseColor}\n" +
                                  $"R: {_hairColorR}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set EyebrowsColor", "Failed to set eyebrows color. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to Set EyebrowsColor: {ex.Message}");
                ShowPopUp("⚠️ Set EyebrowsColor", $"Error: {ex.Message}");
            }
        }

        private async void SetEyelashesColor()
        {
            if (AvatarToDebug?.ManagedAvatar == null)
            {
                ShowPopUp("⚠️ Set Eyelashes Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.");
                return;
            }

            try
            {
                var eyeLashColor = AvatarSdk.CreateEyeLashColor(_hairBaseColor, _hairColorR);
                var success = await AvatarSdk.SetColorAsync(AvatarToDebug.ManagedAvatar, eyeLashColor);

                if (success)
                {
                    var message = $"Successfully set eyelashes color using SetColorAsync:\n" +
                                  $"Base: {_hairBaseColor}\n" +
                                  $"Base2: {_hairColorR}";
                    Debug.Log(message);
                }
                else
                {
                    ShowPopUp("⚠️ Set Eyelashes Color", "Failed to set eyelashes color. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set eyelashes color: {ex.Message}");
                ShowPopUp("⚠️ Set Eyelashes Color", $"Error: {ex.Message}");
            }
        }

        [InspectorButton("===== Create User Color (Test) =====", InspectorButtonAttribute.ExecutionMode.EditMode)]
        private void HeaderCreateUserColor() { }

        [Header("Create User Color")]
        [Tooltip("Color type for the new user color (Hair, Eyebrow, or Eyelash).")]
        [SerializeField] private UserColorType _createUserColorType = UserColorType.Hair;
        [Tooltip("Array of colors (Hair: up to 4, Eyebrow/Eyelash: up to 2). Missing entries are padded with the first color or black.")]
        [SerializeField] private Color[] _createUserColor = new Color[] { new Color(0.4f, 0.2f, 0.1f, 1f) };

        [InspectorButton("\nCreate User Color\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void CreateUserColor()
        {
            if (AvatarSdk.IsLoggedIn is false)
            {
                ShowPopUp("⚠️ Create User Color", "Log in first!");
                return;
            }

            try
            {
                var padColor = (_createUserColor != null && _createUserColor.Length > 0) ? _createUserColor[0] : Color.black;
                var colors = new List<Color>();
                switch (_createUserColorType)
                {
                    case UserColorType.Hair:
                    case UserColorType.FacialHair:
                        for (int i = 0; i < 4; i++)
                        {
                            colors.Add(i < _createUserColor?.Length ? _createUserColor[i] : padColor);
                        }
                        break;
                    case UserColorType.Eyebrow:
                    case UserColorType.Eyelash:
                        colors.Add(_createUserColor != null && _createUserColor.Length > 0 ? _createUserColor[0] : padColor);
                        colors.Add(_createUserColor != null && _createUserColor.Length > 1 ? _createUserColor[1] : padColor);
                        break;
                    case UserColorType.Skin:
                        colors.Add(_createUserColor != null && _createUserColor.Length > 0 ? _createUserColor[0] : padColor);
                        break;
                    default:
                        ShowPopUp("⚠️ Create User Color", $"Unsupported color type: {_createUserColorType}");
                        return;
                }

                var color = await AvatarSdk.CreateUserColorAsync(_createUserColorType, colors);
                if (color != null)
                {
                    var instanceId = color is IAvatarCustomColor cc ? cc.InstanceId : null;
                    Debug.Log($"CreateUserColorAsync succeeded. InstanceId: {instanceId ?? "(none)"}");
                    ShowPopUp("Create User Color", $"Created successfully.\nInstanceId: {instanceId ?? "(none)"}");
                }
                else
                {
                    ShowPopUp("⚠️ Create User Color", "CreateUserColorAsync returned null (check logs for errors).");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create user color: {ex.Message}");
                ShowPopUp("⚠️ Create User Color", $"Error: {ex.Message}");
            }
        }

        [Header("Get User Colors By Category")]
        [Tooltip("Color type to filter")]
        [SerializeField] private UserColorType _getUserColorsByCategoryType = UserColorType.Hair;

        [InspectorButton("\nGet User Colors By Category\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void GetUserColorsByCategory()
        {
            if (AvatarSdk.IsLoggedIn is false)
            {
                ShowPopUp("⚠️ Get User Colors By Category", "Log in first!");
                return;
            }

            try
            {
                UserColorType filter = _getUserColorsByCategoryType;
                var list = await AvatarSdk.GetUserColorsAsync(filter);
                if (list == null)
                {
                    ShowPopUp("Get User Colors By Category", "Returned null.");
                    return;
                }
                string summary;
                if (list.Count == 0)
                {
                    summary = "No user colors found.";
                }
                else if (list.Count > 10)
                {
                    summary = $"{list.Count} items (first 10):\n" + string.Join("\n", list.GetRange(0, 10).ConvertAll(c => $"{(c is IAvatarCustomColor cc ? cc.InstanceId : "")} ({c.Kind.ToString()})"));
                }
                else
                {
                    summary = $"{list.Count} item(s):\n" + string.Join("\n", list.ConvertAll(c => $"{(c is IAvatarCustomColor cc ? cc.InstanceId : "")} ({c.Kind.ToString()})"));
                }
                Debug.Log($"GetUserColorsByCategoryAsync: {list.Count} items");
                ShowPopUp("Get User Colors By Category", summary);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get user colors by category: {ex.Message}");
                ShowPopUp("⚠️ Get User Colors By Category", $"Error: {ex.Message}");
            }
        }

        [Header("Delete User Color")]
        [Tooltip("User color category used when fetching the list to pick an item to delete.")]
        [SerializeField] private UserColorType _deleteUserColorType = UserColorType.Hair;

        [InspectorButton("\nDelete User Color (pick from list)…\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void DeleteUserColorPickFromList()
        {
            if (AvatarSdk.IsLoggedIn is false)
            {
                ShowPopUp("⚠️ Delete User Color", "Log in first!");
                return;
            }

            try
            {
                var list = await AvatarSdk.GetUserColorsAsync(_deleteUserColorType);
                if (list == null)
                {
                    ShowPopUp("Delete User Color", "GetUserColorsAsync returned null.");
                    return;
                }

                if (list.Count == 0)
                {
                    ShowPopUp("Delete User Color", $"No user colors found for {_deleteUserColorType}.");
                    return;
                }

                Debug.Log($"Delete User Color — {_deleteUserColorType}: opening picker with {list.Count} item(s).");

#if UNITY_EDITOR
                DeleteUserColorListWindow.Show(list, _deleteUserColorType);
#else
                ShowPopUp(
                    "Delete User Color",
                    "Delete picker window is only available in the Unity Editor.");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to prepare delete user color: {ex.Message}");
                ShowPopUp("⚠️ Delete User Color", $"Error: {ex.Message}");
            }
        }

        [Header("Update User Color")]
        [Tooltip("User color category used when fetching the list to pick an item to update.")]
        [SerializeField] private UserColorType _updateUserColorType = UserColorType.Hair;
        [Tooltip("New color values applied when you pick a preset (same format as Create: 4 for Hair/FacialHair, 2 for Eyebrow/Eyelash, 1 for Skin).")]
        [SerializeField] private Color[] _updateUserColors = new Color[] { new Color(0.2f, 0.4f, 0.6f, 1f) };

        [InspectorButton("\nUpdate User Color (pick from list)…\n", InspectorButtonAttribute.ExecutionMode.PlayMode)]
        private async void UpdateUserColorPickFromList()
        {
            if (AvatarSdk.IsLoggedIn is false)
            {
                ShowPopUp("⚠️ Update User Color", "Log in first!");
                return;
            }

            try
            {
                var list = await AvatarSdk.GetUserColorsAsync(_updateUserColorType);
                if (list == null)
                {
                    ShowPopUp("Update User Color", "GetUserColorsAsync returned null.");
                    return;
                }

                if (list.Count == 0)
                {
                    ShowPopUp("Update User Color", $"No user colors found for {_updateUserColorType}.");
                    return;
                }

                var colors = _updateUserColors != null && _updateUserColors.Length > 0
                    ? new List<Color>(_updateUserColors)
                    : new List<Color> { Color.black };

                Debug.Log($"Update User Color — {_updateUserColorType}: opening picker with {list.Count} item(s).");

#if UNITY_EDITOR
                UpdateUserColorListWindow.Show(list, _updateUserColorType, colors);
#else
                ShowPopUp(
                    "Update User Color",
                    "Update picker window is only available in the Unity Editor.");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to prepare update user color: {ex.Message}");
                ShowPopUp("⚠️ Update User Color", $"Error: {ex.Message}");
            }
        }

        private void ShowPopUp(string title, string message)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, message, "OK");
#endif
            Debug.LogWarning($"{title}: {message}");
        }
    }
}
