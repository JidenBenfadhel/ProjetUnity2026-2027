using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MeshSegmentDrawer : MonoBehaviour
{
    public enum CutDrawMode
    {
        None,
        FreeDraw,
        SimpleCut
    }

    [Header("Références")]
    public UnifiedMeshSplitter meshSplitter;
    public Material lineMaterial;

    [Header("Configuration")]
    public float lineThickness = 0.015f;
    public float vertexSnapThreshold = 0.05f;
    public float edgeSnapThreshold = 0.04f;
    public float minSimpleCutLength = 0.01f;

    [Header("Contraintes Simple Cut")]
    [Tooltip("Angle maximal autorisé entre la face du point 1 et celle du point 2 (en degrés).")]
    public float maxPlaneAngleTolerance = 5f;
    [Tooltip("Écart maximal de hauteur autorisé par rapport au plan du point 1 (en espace local).")]
    public float maxPlaneDistanceTolerance = 0.01f;

    private List<Vector3> chainPoints = new List<Vector3>();
    private List<Vector3> chainNormals = new List<Vector3>();
    private List<GameObject> createdSegments = new List<GameObject>();
    private LineRenderer previewLine;

    private Vector3 currentSurfacePoint;
    private Vector3 currentHitNormal = Vector3.up;
    private bool hasValidPoint = false;
    private bool isHoveringValidPlane = true;

    [Header("Activation UI")]
    public CutDrawMode currentMode = CutDrawMode.None;
    public bool disableAfterCut = true;

    // =================================================================
    // MÉTHODES À APPELER DEPUIS LES BOUTONS DU CANVAS
    // =================================================================

    /// <summary>
    /// Bouton 1 : Active / Désactive le mode dessin libre (polygone 2D ou forme 3D).
    /// </summary>
    public void ToggleDrawing()
    {
        SetDrawingEnabled(currentMode != CutDrawMode.FreeDraw);
    }

    public void SetDrawingEnabled(bool state)
    {
        SetMode(state ? CutDrawMode.FreeDraw : CutDrawMode.None);
    }

    /// <summary>
    /// Bouton 2 : Active / Désactive le mode Simple Cut (2 points max sur un même plan).
    /// </summary>
    public void ToggleSimpleCut()
    {
        SetSimpleCutEnabled(currentMode != CutDrawMode.SimpleCut);
    }

    public void SetSimpleCutEnabled(bool state)
    {
        SetMode(state ? CutDrawMode.SimpleCut : CutDrawMode.None);
    }

    private void SetMode(CutDrawMode newMode)
    {
        ResetChain();
        currentMode = newMode;
    }

    // =================================================================
    // BOUCLE PRINCIPALE
    // =================================================================

    private void Update()
    {
        if (currentMode == CutDrawMode.None) return;

        // Empêcher de dessiner à travers les boutons ou éléments du Canvas
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (meshSplitter == null) return;

        UpdateSurfacePoint();
        if (Mouse.current == null) return;

        // Annulation au clic droit
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ResetChain();
            return;
        }

        if (currentMode == CutDrawMode.FreeDraw)
        {
            HandleFreeDraw();
        }
        else if (currentMode == CutDrawMode.SimpleCut)
        {
            HandleSimpleCut();
        }
    }

    // =================================================================
    // MODE 1 : DESSIN LIBRE (EXISTANT)
    // =================================================================

    private void HandleFreeDraw()
    {
        if (!hasValidPoint) return;

        float distToStartLocal = float.MaxValue;
        if (chainPoints.Count > 0)
        {
            Vector3 currentLocal = meshSplitter.transform.InverseTransformPoint(currentSurfacePoint);
            Vector3 startLocal = meshSplitter.transform.InverseTransformPoint(chainPoints[0]);
            distToStartLocal = Vector3.Distance(currentLocal, startLocal);
        }

        // Prévisualisation du tracé
        if (chainPoints.Count > 0 && previewLine != null)
        {
            Vector3 startPos = chainPoints[chainPoints.Count - 1];
            Vector3 endPos = (chainPoints.Count >= 3 && distToStartLocal < vertexSnapThreshold) ? chainPoints[0] : currentSurfacePoint;
            previewLine.startColor = Color.red;
            previewLine.endColor = Color.red;
            previewLine.SetPosition(0, startPos);
            previewLine.SetPosition(1, endPos);
        }

        // Clic Gauche : Ajout / Fermeture
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (chainPoints.Count == 0)
            {
                chainPoints.Add(currentSurfacePoint);
                chainNormals.Add(currentHitNormal);
                CreatePreviewLine(currentSurfacePoint);
            }
            else
            {
                if (chainPoints.Count >= 3 && distToStartLocal < vertexSnapThreshold)
                {
                    CreateFinalSegment(chainPoints[chainPoints.Count - 1], chainPoints[0]);
                    chainPoints.Add(chainPoints[0]);
                    chainNormals.Add(chainNormals[0]);

                    bool success = meshSplitter.PerformCut(chainPoints, chainNormals);

                    if (success)
                    {
                        ResetChain();
                        if (disableAfterCut) SetMode(CutDrawMode.None);
                    }
                    else
                    {
                        RemoveLastPoint();
                    }
                }
                else
                {
                    CreateFinalSegment(chainPoints[chainPoints.Count - 1], currentSurfacePoint);
                    chainPoints.Add(currentSurfacePoint);
                    chainNormals.Add(currentHitNormal);

                    if (previewLine != null)
                    {
                        previewLine.SetPosition(0, currentSurfacePoint);
                        previewLine.SetPosition(1, currentSurfacePoint);
                    }
                }
            }
        }
    }

    // =================================================================
    // MODE 2 : SIMPLE CUT (2 POINTS MAX SUR LE MÊME PLAN)
    // =================================================================

    private void HandleSimpleCut()
    {
        // Prévisualisation du segment entre P1 et P2
        if (chainPoints.Count == 1 && previewLine != null)
        {
            previewLine.SetPosition(0, chainPoints[0]);

            if (hasValidPoint && isHoveringValidPlane)
            {
                previewLine.startColor = Color.red;
                previewLine.endColor = Color.red;
                previewLine.SetPosition(1, currentSurfacePoint);
            }
            else
            {
                // Indique visuellement que le curseur a quitté le plan initial
                previewLine.startColor = Color.gray;
                previewLine.endColor = Color.gray;
            }
        }

        // Clic Gauche
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!hasValidPoint) return;

            // 1er point (P1)
            if (chainPoints.Count == 0)
            {
                chainPoints.Add(currentSurfacePoint);
                chainNormals.Add(currentHitNormal);
                CreatePreviewLine(currentSurfacePoint);
            }
            // 2e point (P2) : autorisé uniquement s'il est sur le même plan que P1
            else if (chainPoints.Count == 1)
            {
                if (!isHoveringValidPlane) return;

                Vector3 p1 = chainPoints[0];
                Vector3 p2 = currentSurfacePoint;
                Vector3 surfaceNormal = chainNormals[0].normalized;

                if (Vector3.Distance(p1, p2) < minSimpleCutLength) return;

                Vector3 segDir = (p2 - p1).normalized;
                Vector3 cutPlaneNormal = Vector3.Cross(segDir, surfaceNormal).normalized;

                if (cutPlaneNormal.sqrMagnitude > 0.0001f)
                {
                    // Construction d'un 3e point virtuel dans le plan de coupe (vers l'intérieur du pavé)
                    // pour déclencher automatiquement ObliquePlaneCut dans CuttingSolidBuilder
                    Vector3 midPoint = (p1 + p2) * 0.5f;
                    Vector3 virtualP3 = midPoint - surfaceNormal * 0.5f;

                    List<Vector3> simpleCutPoints = new List<Vector3> { p1, p2, virtualP3 };
                    List<Vector3> simpleCutNormals = new List<Vector3> { surfaceNormal, surfaceNormal, cutPlaneNormal };

                    bool success = meshSplitter.PerformCut(simpleCutPoints, simpleCutNormals);

                    if (success)
                    {
                        ResetChain();
                        if (disableAfterCut) SetMode(CutDrawMode.None);
                    }
                }
            }
        }
    }

    // =================================================================
    // RAYCAST & VÉRIFICATION DE PLAN
    // =================================================================

    private void UpdateSurfacePoint()
    {
        if (Mouse.current == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == meshSplitter.transform || hit.transform.IsChildOf(meshSplitter.transform))
            {
                MeshFilter mf = meshSplitter.GetComponent<MeshFilter>();
                Vector3 localHit = meshSplitter.transform.InverseTransformPoint(hit.point);
                Vector3 localNorm = meshSplitter.transform.InverseTransformDirection(hit.normal).normalized;

                // En mode SimpleCut, vérifier que le point survolé appartient strictement au plan du point 1
                if (currentMode == CutDrawMode.SimpleCut && chainPoints.Count == 1)
                {
                    Vector3 firstLocalPos = meshSplitter.transform.InverseTransformPoint(chainPoints[0]);
                    Vector3 firstLocalNorm = meshSplitter.transform.InverseTransformDirection(chainNormals[0]).normalized;

                    bool sameNormal = Vector3.Angle(localNorm, firstLocalNorm) <= maxPlaneAngleTolerance;
                    bool coplanar = Mathf.Abs(Vector3.Dot(firstLocalNorm, localHit - firstLocalPos)) <= maxPlaneDistanceTolerance;

                    isHoveringValidPlane = sameNormal && coplanar;
                    if (!isHoveringValidPlane)
                    {
                        hasValidPoint = false;
                        return;
                    }

                    localNorm = firstLocalNorm;
                }
                else
                {
                    isHoveringValidPlane = true;

                    if (chainNormals.Count > 0)
                    {
                        Vector3 firstLocalNorm = meshSplitter.transform.InverseTransformDirection(chainNormals[0]).normalized;
                        if (Vector3.Angle(localNorm, firstLocalNorm) < 45f) localNorm = firstLocalNorm;
                    }
                }

                if (mf != null && mf.sharedMesh != null)
                {
                    localHit = SnapToBoxEdges(localHit, localNorm, mf.sharedMesh.bounds, edgeSnapThreshold);
                }

                currentSurfacePoint = meshSplitter.transform.TransformPoint(localHit) + hit.normal * 0.002f;
                currentHitNormal = hit.normal;
                hasValidPoint = true;
                return;
            }
        }

        hasValidPoint = false;
        isHoveringValidPlane = false;
    }

    private Vector3 SnapToBoxEdges(Vector3 localPos, Vector3 localNorm, Bounds b, float threshold)
    {
        Vector3 absN = new Vector3(Mathf.Abs(localNorm.x), Mathf.Abs(localNorm.y), Mathf.Abs(localNorm.z));

        if (absN.x > 0.5f)
        {
            if (Mathf.Abs(localPos.y - b.min.y) < threshold) localPos.y = b.min.y;
            if (Mathf.Abs(localPos.y - b.max.y) < threshold) localPos.y = b.max.y;
            if (Mathf.Abs(localPos.z - b.min.z) < threshold) localPos.z = b.min.z;
            if (Mathf.Abs(localPos.z - b.max.z) < threshold) localPos.z = b.max.z;
        }
        else if (absN.y > 0.5f)
        {
            if (Mathf.Abs(localPos.x - b.min.x) < threshold) localPos.x = b.min.x;
            if (Mathf.Abs(localPos.x - b.max.x) < threshold) localPos.x = b.max.x;
            if (Mathf.Abs(localPos.z - b.min.z) < threshold) localPos.z = b.min.z;
            if (Mathf.Abs(localPos.z - b.max.z) < threshold) localPos.z = b.max.z;
        }
        else
        {
            if (Mathf.Abs(localPos.x - b.min.x) < threshold) localPos.x = b.min.x;
            if (Mathf.Abs(localPos.x - b.max.x) < threshold) localPos.x = b.max.x;
            if (Mathf.Abs(localPos.y - b.min.y) < threshold) localPos.y = b.min.y;
            if (Mathf.Abs(localPos.y - b.max.y) < threshold) localPos.y = b.max.y;
        }
        return localPos;
    }

    // =================================================================
    // RENDU DES LIGNES & NETTOYAGE
    // =================================================================

    private void CreatePreviewLine(Vector3 startPoint)
    {
        GameObject lineObj = new GameObject("PreviewLine");
        lineObj.transform.SetParent(meshSplitter.transform, true);
        previewLine = lineObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(previewLine);
        previewLine.SetPosition(0, startPoint);
        previewLine.SetPosition(1, startPoint);
    }

    private void CreateFinalSegment(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject lineObj = new GameObject("Segment_" + createdSegments.Count);
        lineObj.transform.SetParent(meshSplitter.transform, true);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(lr);
        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);
        createdSegments.Add(lineObj);
    }

    private void ConfigureLineRenderer(LineRenderer lr)
    {
        lr.useWorldSpace = true;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        lr.startColor = Color.red;
        lr.endColor = Color.red;

        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Sprites/Default");
            lr.material = new Material(shader) { color = Color.red };
        }
    }

    private void RemoveLastPoint()
    {
        if (chainPoints.Count > 0) chainPoints.RemoveAt(chainPoints.Count - 1);
        if (chainNormals.Count > 0) chainNormals.RemoveAt(chainNormals.Count - 1);
        if (createdSegments.Count > 0)
        {
            Destroy(createdSegments[createdSegments.Count - 1]);
            createdSegments.RemoveAt(createdSegments.Count - 1);
        }
    }

    public void ResetChain()
    {
        chainPoints.Clear();
        chainNormals.Clear();
        foreach (GameObject segment in createdSegments) Destroy(segment);
        createdSegments.Clear();
        if (previewLine != null) Destroy(previewLine.gameObject);
    }

    private void OnDisable() => ResetChain();
}