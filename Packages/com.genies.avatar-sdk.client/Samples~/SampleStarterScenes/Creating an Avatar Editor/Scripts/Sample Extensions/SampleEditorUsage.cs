using System;
using Cysharp.Threading.Tasks;
using Genies.Sdk.Samples.Common;
using UnityEngine;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Shows an example of how the Editor Canvas prefab in its entirety,
    /// or it's smaller components, can be used
    /// </summary>
    public class SampleEditorUsage : MonoBehaviour
    {
        private enum UsageType
        {
            ShowFullEditor,
            ShowSingleSection
        }

        [SerializeField] private UsageType usageType = UsageType.ShowFullEditor;

        [SerializeField] private EditorInitializer _editorInitializer;
        [SerializeField] private UIGeneratorGroup _generatorGroup;
        [SerializeField] private Transform _avatarSpawn;

        private ManagedAvatar _avatar;

        // Start is called before the first frame update
        private void Start()
        {
            Application.targetFrameRate = 60;

            AvatarSdk.Events.UserLoggedOut += OnUserLoggedOut;

            if (AvatarSdk.IsLoggedIn)
            {
                OnUserLoggedIn();
            }
            else
            {
                AvatarSdk.Events.UserLoggedIn += OnUserLoggedIn;
            }
        }

        private void OnDestroy()
        {
            AvatarSdk.Events.UserLoggedIn -= OnUserLoggedIn;
            AvatarSdk.Events.UserLoggedOut -= OnUserLoggedOut;
        }

        private void OnUserLoggedIn()
        {
            // Displaying the editor requires API calls which need a logged-in user
            if (usageType == UsageType.ShowFullEditor)
            {
                DisplayEditor().Forget();
            }
            else
            {
                DisplaySingleGeneratorGroup().Forget();
            }
        }

        private void OnUserLoggedOut()
        {
            _avatar.Dispose();
            AvatarLoadedNotifier.InvokeDestroyed(_avatar);

            _editorInitializer.UITransitionManager.HideMainView();
            _generatorGroup.gameObject.SetActive(false);
        }

        private async UniTask DisplayEditor()
        {
            // The editor will handle all camera angles around the avatar
            _avatar = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.User());
            _editorInitializer.Initialize(_avatar).Forget();
        }

        private async UniTask DisplaySingleGeneratorGroup()
        {
            // The smaller pieces of the editor (the individual views) do not handle camera angles, so
            // here we position the avatar ourselves
            _avatar = await AvatarSdk.LoadAvatarAsync(new LoadAvatarOptions.User());
            _avatar.Root.transform.SetPositionAndRotation(_avatarSpawn.position, _avatarSpawn.rotation);

            _generatorGroup.gameObject.SetActive(true);
            _generatorGroup.Initialize(_avatar).Forget();
        }
    }
}
