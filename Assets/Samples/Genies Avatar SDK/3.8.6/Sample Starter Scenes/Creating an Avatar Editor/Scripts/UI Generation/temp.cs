using UnityEngine;

public class ShowSavePath : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Persistent Data Path : " +
                  Application.persistentDataPath);
    }
}