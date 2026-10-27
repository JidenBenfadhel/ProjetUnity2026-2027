using UnityEngine;
using System.Collections.Generic;

// Ajoute un bouton dans le menu clic droit d'Unity
[CreateAssetMenu(fileName = "NewCakeRecipe", menuName = "Pâtisserie/Nouvelle Recette", order = 1)]
public class CakeRecipeSO : ScriptableObject
{
    // L'enum définit les formes de base possibles pour le sculptage
    public enum CakeShape { Rond, Carre, Coeur, Etoile }

    [Tooltip("Temps accordé en mode Challenge (en secondes)")]
    public float timeAllowed = 120f;

    [Header("Exigences du Client")]
    public CakeShape expectedShape = CakeShape.Rond;

    [Tooltip("La couleur du glaçage attendue")]
    public Color expectedFrostingColor = Color.white;

    [Header("Décorations (Toppings)")]
    [Tooltip("Liste des décorations qui doivent obligatoirement être sur le gâteau")]
    
    public List<GameObject> requiredToppings;

    [Header("UI - Interface")]
    [Tooltip("")] // L'image qui s'affichera dans le HUD ou dans la bulle du client
    public Sprite recipeImage;
}