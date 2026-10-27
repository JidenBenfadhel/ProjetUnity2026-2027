using UnityEngine;
using System.Collections.Generic;

public class ClientManager : MonoBehaviour
{
    [Header("Base de données des recettes")]
    [Tooltip("Fichiers CakeRecipeSO créés")]
    public List<CakeRecipeSO> allRecipes;

    [Header("Commande en cours")]
    public CakeRecipeSO currentOrder; // On stocke la commande actuelle ici pour que les autres scripts puissent la lire

    private void OnEnable()
    {
        // On s'abonne au changement d'état du GameManager
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Désabonnement pour éviter les fuites de mémoire
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // Dès que le jeu passe dans l'état "Client", on génère la commande
        if (newState == GameManager.GameState.Client)
        {
            GenerateNewOrder();
        }
    }

    public void GenerateNewOrder()
    {
        if (allRecipes == null || allRecipes.Count == 0)
        {
            Debug.LogWarning("Attention : Aucune recette n'a été ajoutée dans la liste du ClientManager !");
            return;
        }

        // Tire un nombre au hasard entre 0 et la taille de la liste
        int randomIndex = Random.Range(0, allRecipes.Count);

        // Assigne la recette choisie comme commande actuelle
        currentOrder = allRecipes[randomIndex];

        Debug.Log($"Nouvelle commande générée : Le client veut un(e) {currentOrder.name} !");
    }
}