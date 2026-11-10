using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panneaux UI")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;

    private void Start()
    {
        // Au lancement de la scène, on s'assure d'être sur le menu principal
        ShowMainMenu();

        // Optionnel : S'assurer que le GameManager sait qu'on est dans le Menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        }
    }

    // --- NAVIGATION INTERNE DU MENU ---

    public void ShowModeSelection()
    {
        mainMenuPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        modeSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- COMMUNICATION AVEC LE GAME MANAGER (BACK) ---

    public void StartFreeMode()
    {
        // On utilise les Enums définis par India dans son GameManager
        GameManager.Instance.SetGameMode(GameManager.GameMode.Libre);

        // On passe à l'état Client pour déclencher la suite
        GameManager.Instance.ChangeState(GameManager.GameState.Client);
    }

    public void StartChallengeMode()
    {
        GameManager.Instance.SetGameMode(GameManager.GameMode.Challenge);
        GameManager.Instance.ChangeState(GameManager.GameState.Client);
    }

    public void QuitGame()
    {
        Debug.Log("Fermeture du jeu !");
        Application.Quit();

        // Stoppe le mode Play dans l'éditeur Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}