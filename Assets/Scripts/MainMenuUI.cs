using UnityEngine;
using UnityEngine.SceneManagement; // Indispensable pour changer de scène

public class MainMenuUI : MonoBehaviour
{
    [Header("Panneaux UI")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;

    [Header("Scènes à charger")]
    [Tooltip("Écris le nom exact de la scène du mode Challenge")]
    public string challengeSceneName = "Boutique";
    [Tooltip("Écris le nom exact de la scène du mode Libre (optionnel pour l'instant)")]
    public string freeSceneName = "";

    private void Start()
    {
        ShowMainMenu();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        }
    }

    public void ShowModeSelection()
    {
        if (mainMenuPanel != null && modeSelectionPanel != null)
        {
            mainMenuPanel.SetActive(false);
            modeSelectionPanel.SetActive(true);
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null && modeSelectionPanel != null)
        {
            modeSelectionPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void StartFreeMode()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.Libre);
            GameManager.Instance.ChangeState(GameManager.GameState.Client);
        }

        if (!string.IsNullOrEmpty(freeSceneName))
        {
            SceneManager.LoadScene(freeSceneName);
        }
    }

    public void StartChallengeMode()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameMode(GameManager.GameMode.Challenge);
            GameManager.Instance.ChangeState(GameManager.GameState.Client);
        }

        // Charge la scène renseignée dans l'Inspector
        if (!string.IsNullOrEmpty(challengeSceneName))
        {
            SceneManager.LoadScene(challengeSceneName);
        }
        else
        {
            Debug.LogWarning("Aucun nom de scène renseigné pour le mode Challenge !");
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}   