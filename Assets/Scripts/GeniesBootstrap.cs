
using UnityEngine;
using Genies.Sdk;
using System.Threading.Tasks;

public class GeniesBootstrap : MonoBehaviour
{
    public static bool IsReady { get; private set; }

    private async void Start()
    {
        if (AvatarSdk.IsLoggedIn)
        {
            IsReady = true;
            return;
        }

        var (success, reason) =
            await AvatarSdk.StartLoginAnonymousAsync();

        if (success)
        {
            Debug.Log("Genies anonymous login successful");
            IsReady = true;
        }
        else
        {
            Debug.LogError("Genies login failed: " + reason);
        }
    }
}
