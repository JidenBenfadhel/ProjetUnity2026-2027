using UnityEngine;
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
                // Ex: Réinitialiser les scores, vider l'inventaire
                break;

            case GameState.Client:
                // Ex: Générer un nouveau client et sa commande
                break;

            case GameState.Gateau:
                // Ex: Si CurrentMode == GameMode.Challenge, lancer le time
                break;

            case GameState.Score:
                // Ex: Lancer l'algorithme d'évaluation de ressemblance
                break;
        }
    }
}