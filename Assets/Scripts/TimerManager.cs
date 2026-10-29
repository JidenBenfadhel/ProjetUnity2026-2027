using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("UI - Interface")]
    [Tooltip("Composant texte du chrono (HUD)")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isTimerRunning = false;
    private ClientManager clientManager;

    private void Awake()
    {
        // On récupère automatiquement la référence au ClientManager dans la scène
        clientManager = FindAnyObjectByType<ClientManager>();
    }

    private void OnEnable()
    {
        // Abonnement au changement d'état
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Gateau)
        {
            // On vérifie qu'on est bien en mode Challenge avant de lancer le chrono
            if (GameManager.Instance.CurrentMode == GameManager.GameMode.Challenge)
            {
                StartTimer();
            }
            else
            {
                if (timerText != null) timerText.text = "Mode Libre";
                isTimerRunning = false;
            }
        }
        else
        {
            // Stoppe le timer si on repasse au menu ou à l'écran de score
            isTimerRunning = false;
        }
    }

    private void StartTimer()
    {
        if (clientManager != null && clientManager.currentOrder != null)
        {
            // On récupère le temps alloué depuis la recette piochée
            currentTime = clientManager.currentOrder.timeAllowed;
            isTimerRunning = true;
            UpdateTimerUI();
        }
        else
        {
            Debug.LogError("TimerManager : Impossible de lancer le timer, aucune commande trouvée !");
        }
    }

    private void Update()
    {
        // L'Update tourne en boucle à chaque frame
        if (isTimerRunning)
        {
            // Time.deltaTime représente le temps écoulé depuis la dernière frame
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                Debug.Log("Temps écoulé ! Fin de la préparation.");

                // Force le passage à l'étape suivante (Évaluation)
                GameManager.Instance.ChangeState(GameManager.GameState.Score);
            }

            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        // Met à jour l'affichage seulement si un texte UI est assigné
        if (timerText != null)
        {
            // Formate le temps restant en format "Minutes:Secondes" (ex: 01:30)
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}