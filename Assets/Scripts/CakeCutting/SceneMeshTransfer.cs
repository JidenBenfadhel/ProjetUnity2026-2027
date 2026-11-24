using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMeshTransfer : MonoBehaviour
{
    [Header("Configuration de la Scène")]
    [Tooltip("Nom exact de la scène vierge de destination (doit être dans les Build Settings).")]
    public string nextSceneName = "PreviewScene";

    [Header("Configuration Layer & Tag")]
    [Tooltip("Nom du layer attribué aux objets transférés")]
    public string targetLayerName = "Cake";
    [Tooltip("Nom du tag attribué aux objets transférés (doit exister dans Tags & Layers)")]
    public string targetTagName = "Cake";

    [Header("Options de Transfert")]
    [Tooltip("Centre l'ensemble des objets autour de (0,0,0) tout en gardant leurs positions relatives séparées.")]
    public bool centerGroupInNewScene = true;
    public bool includeMeshCollider = true;

    private int GetCakeLayerIndex()
    {
        int layer = LayerMask.NameToLayer(targetLayerName);
        if (layer == -1)
        {
            layer = LayerMask.NameToLayer(targetLayerName.ToLower());
        }
        return layer;
    }

    /// <summary>
    /// Méthode publique à relier à l'événement OnClick() du bouton "Next" sur le Canvas.
    /// </summary>
    public void GoToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("[SceneMeshTransfer] Le nom de la scène suivante n'est pas renseigné.");
            return;
        }

        // 1. Récupérer uniquement les objets qui n'ont pas été supprimés ni désactivés
        List<GameObject> survivingObjects = new List<GameObject>();
        foreach (GameObject obj in UnifiedMeshSplitter.TrackedObjects)
        {
            if (obj != null && obj.activeInHierarchy && !survivingObjects.Contains(obj))
            {
                MeshFilter mf = obj.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    survivingObjects.Add(obj);
                }
            }
        }

        if (survivingObjects.Count == 0)
        {
            Debug.LogWarning("[SceneMeshTransfer] Aucun objet actif à transférer.");
            return;
        }

        // 2. Calculer le décalage si l'on souhaite centrer le groupe dans la nouvelle scène
        Vector3 groupOffset = Vector3.zero;
        if (centerGroupInNewScene)
        {
            foreach (GameObject obj in survivingObjects)
            {
                groupOffset += obj.transform.position;
            }
            groupOffset /= survivingObjects.Count;
        }

        int cakeLayer = GetCakeLayerIndex();
        if (cakeLayer == -1)
        {
            Debug.LogWarning($"[SceneMeshTransfer] Le layer '{targetLayerName}' n'existe pas dans les paramètres du projet !");
        }

        // 3. Créer un GameObject indépendant et épuré pour chaque objet survivant
        for (int i = 0; i < survivingObjects.Count; i++)
        {
            CreateCleanPersistentObject(survivingObjects[i], groupOffset, i, cakeLayer);
        }

        // 4. Nettoyer le registre et charger la scène vierge
        UnifiedMeshSplitter.TrackedObjects.Clear();
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    private void CreateCleanPersistentObject(GameObject sourceObj, Vector3 positionOffset, int index, int layerIndex)
    {
        MeshFilter sourceMf = sourceObj.GetComponent<MeshFilter>();
        MeshRenderer sourceMr = sourceObj.GetComponent<MeshRenderer>();

        // Création d'un GameObject racine séparé (sans scripts de découpe ni LineRenderer enfants)
        GameObject cleanObj = new GameObject($"{sourceObj.name}_{index}");
        cleanObj.transform.SetParent(null); // Obligatoire pour DontDestroyOnLoad

        // Attribution du Layer "Cake"
        if (layerIndex != -1)
        {
            cleanObj.layer = layerIndex;
        }

        // Attribution du Tag "Cake"
        if (!string.IsNullOrEmpty(targetTagName))
        {
            try
            {
                cleanObj.tag = targetTagName;
            }
            catch (UnityException)
            {
                Debug.LogWarning($"[SceneMeshTransfer] Le Tag '{targetTagName}' n'existe pas dans Tags & Layers !");
            }
        }

        cleanObj.transform.position = sourceObj.transform.position - positionOffset;
        cleanObj.transform.rotation = sourceObj.transform.rotation;
        cleanObj.transform.localScale = sourceObj.transform.lossyScale;

        // Copie du Mesh et des Materials
        MeshFilter mf = cleanObj.AddComponent<MeshFilter>();
        mf.sharedMesh = sourceMf.sharedMesh;

        MeshRenderer mr = cleanObj.AddComponent<MeshRenderer>();
        if (sourceMr != null)
        {
            mr.sharedMaterials = sourceMr.sharedMaterials;
        }

        if (includeMeshCollider)
        {
            MeshCollider col = cleanObj.AddComponent<MeshCollider>();
            col.sharedMesh = sourceMf.sharedMesh;
        }

        // Conserve cet objet séparé lors du chargement de la nouvelle scène
        DontDestroyOnLoad(cleanObj);
    }
}