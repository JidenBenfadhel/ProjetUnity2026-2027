
using Genies.Sdk;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCManager : MonoBehaviour
{
    private ManagedAvatar avatar;
    private Transform avatarTransform;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 10f;
    private Animator avatarAnimator;

    private async void Start()
    {
        try
        {
            Debug.Log("1 - Connexion Genies...");

            if (!AvatarSdk.IsLoggedIn)
            {
                var (success, reason) =
                    await AvatarSdk.StartLoginAnonymousAsync();

                if (!success)
                {
                    Debug.LogError("Connexion impossible : " + reason);
                    return;
                }
            }

            Debug.Log("2 - Connexion confirmée.");

            avatar =
                await AvatarSdk.LoadFromLocalAvatarDefinitionAsync(
                    "SampleLocalAvatar"
                );

            if (avatar == null)
            {
                Debug.LogError("Avatar NULL !");
                return;
            }

            Debug.Log("3 - Avatar chargé !");

            avatarTransform = avatar.Root.transform;
            avatarAnimator = avatar.Animator;

            if (avatarAnimator != null)
            {
                avatarAnimator.SetFloat("Speed", 0f);
                Debug.Log("Animator connecté au déplacement !");
            }
            else
            {
                Debug.LogError("Animator introuvable !");
            }

            avatarTransform.position = Vector3.zero;
            avatarTransform.rotation = Quaternion.identity;

            Debug.Log("Animator : " + avatar.Animator);
            if (avatar.Animator != null)
            {
                Debug.Log("Animator Controller : " +
                          avatar.Animator.runtimeAnimatorController);

                foreach (var parameter in avatar.Animator.parameters)
                {
                    Debug.Log("Paramètre Animator : " +
                              parameter.name + " / " + parameter.type);
                }
            }
            Debug.Log("Déplacement activé !");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERREUR : " + e);
        }
    }


    private void Update()
    {
        if (avatarTransform == null)
            return;

        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        // Avancer / reculer
        if (Keyboard.current.zKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            vertical -= 1f;

        // Gauche / droite
        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.qKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            horizontal -= 1f;

        Vector3 direction =
            new Vector3(horizontal, 0f, vertical).normalized;

        if (avatarAnimator != null)
        {
            avatarAnimator.SetFloat("Speed", direction.magnitude * moveSpeed);
        }

        if (direction.magnitude > 0.01f)
        {
            avatarTransform.position +=
                direction * moveSpeed * Time.deltaTime;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            avatarTransform.rotation =
                Quaternion.Slerp(
                    avatarTransform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }
    }


    private void OnDestroy()
    {
        if (avatar != null)
            avatar.Dispose();
    }
}
