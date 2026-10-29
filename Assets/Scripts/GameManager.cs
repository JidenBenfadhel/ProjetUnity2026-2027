using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameManager : MonoBehaviour
{
    // Le Singleton : permet d'appeler GameManager.Instance depuis n'importe quel autre script
    public static GameManager Instance { get; private set; }

    // Définition des modes et des états
    public enum GameMode { Libre, Challenge }
    public enum GameState { Menu, Client, Gateau, Score }

    public GameMode CurrentMode { get; private set; }
    public GameState CurrentState { get; private set; }

    // Événements (Actions) pour prévenir automatiquement les autres scripts d'un changement
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<GameMode> OnGameModeChanged;

    private void Awake()
    {
        // Configuration du Singleton
        if (Instance == null)
        {
            Instance = this;
            // Empêche la destruction du Game Manager lors du changement de scène
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Évite d'avoir des clones si on retourne au menu
        }
    }

    private void Start()
    {
        // État initial au lancement du jeu
        ChangeState(GameState.Menu);
    }

    public void SetGameMode(GameMode newMode)
    {
        CurrentMode = newMode;
        OnGameModeChanged?.Invoke(newMode);
        Debug.Log($"Mode de jeu choisi : {CurrentMode}");
    }
    private void Update()
    {
        // RACCOURCIS DE TEST A SUPPRIMER

        if (Keyboard.current == null) return;

        // Appuie sur '1' (pavé numérique ou haut du clavier) pour forcer le mode Challenge
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SetGameMode(GameMode.Challenge);
        }

        // Appuie sur '2' pour simuler l'arrivée d'un client (Génère la recette)
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            ChangeState(GameState.Client);
        }

        // Appuie sur '3' pour lancer la préparation du gâteau (Déclenche le chrono)
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            ChangeState(GameState.Gateau);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Nouvel état du jeu : {CurrentState}");

        // Diffuse l'information à tous les autres scripts abonnés
        OnGameStateChanged?.Invoke(newState);

        // Logique centralisée selon l'état
        switch (newState)
        {
            case GameState.Menu:
                break;

            case GameState.Client:
                break;

            case GameState.Gateau:
                break;

            case GameState.Score:
                break;
        }
    }
}