using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshSegmentDrawer : MonoBehaviour
{
    [Header("Références")]
    public UnifiedMeshSplitter meshSplitter;
    public Material lineMaterial;

    [Header("Configuration")]
    public float lineThickness = 0.015f;
    public float vertexSnapThreshold = 0.05f;
    public float edgeSnapThreshold = 0.04f;

    private List<Vector3> chainPoints = new List<Vector3>();
    private List<Vector3> chainNormals = new List<Vector3>();
    private List<GameObject> createdSegments = new List<GameObject>();
    private LineRenderer previewLine;

    private Vector3 currentSurfacePoint;
    private Vector3 currentHitNormal = Vector3.up;
    private bool hasValidPoint = false;

    private void Update()
    {
        if (meshSplitter == null) return;

        UpdateSurfacePoint();
        if (!hasValidPoint || Mouse.current == null) return;

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

                    // Appel unifié au découpeur 3D
                    bool success = meshSplitter.PerformCut(chainPoints, chainNormals);

                    if (success) ResetChain();
                    else RemoveLastPoint();
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

        if (Mouse.current.rightButton.wasPressedThisFrame) ResetChain();
    }

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

                if (chainNormals.Count > 0)
                {
                    Vector3 firstLocalNorm = meshSplitter.transform.InverseTransformDirection(chainNormals[0]).normalized;
                    if (Vector3.Angle(localNorm, firstLocalNorm) < 45f) localNorm = firstLocalNorm;
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