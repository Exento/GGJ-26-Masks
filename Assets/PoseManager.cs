using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

[ExecuteAlways] // This allows visual updates to show up in the Scene View without hitting Play
public class PoseManager : MonoBehaviour
{
    public UDPReceiver receiver;
    public GameObject jointPrefab;
    public GameObject facePrefab;

    [Header("Scaling Settings")]
    public float lineThickness = 0.05f;
    public float jointVisualScale = 1.0f;

    [Header("Boundary Settings")]
    public bool showBoundary = true;
    public Color boundaryColor = Color.white;
    public float boundaryThickness = 0.05f;

    private int maxSkeletons = 4;
    private List<Skeleton> skeletons = new List<Skeleton>();
    public Color[] personColors = { Color.red, Color.blue, Color.green, Color.yellow };

    private LineRenderer boundaryLine;
    private readonly int[,] connections = new int[,] {
        {1, 2}, {1, 3}, {3, 5}, {2, 4}, {4, 6},
        {1, 7}, {2, 8}, {7, 8}, {7, 9}, {9, 11}, {8, 10}, {10, 12}
    };

    private List<PersonData> framePersons = new List<PersonData>();

    private class Skeleton
    {
        public GameObject[] jointObjs = new GameObject[13];
        public Transform[] jointTransforms = new Transform[13];
        public LineRenderer[] lines = new LineRenderer[12];
    }

    private struct PersonData
    {
        public string rawData;
        public float xPos;
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        CreateBoundary();

        for (int p = 0; p < maxSkeletons; p++)
        {
            Skeleton s = new Skeleton();
            for (int i = 0; i < 13; i++)
            {
                GameObject go = Instantiate(i == 0 ? facePrefab : jointPrefab, transform);
                s.jointObjs[i] = go;
                s.jointTransforms[i] = go.transform;
                
                if (i != 0)
                {
                    Renderer r = go.GetComponentInChildren<Renderer>();
                    if (r != null) r.material.color = personColors[p];
                }
            }
            for (int l = 0; l < connections.GetLength(0); l++)
            {
                GameObject lineObj = new GameObject($"Line_{p}_{l}");
                lineObj.transform.parent = transform;
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = true; 
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = lr.endColor = personColors[p];
                lr.positionCount = 2;
                s.lines[l] = lr;
            }
            skeletons.Add(s);
            SetSkeletonVisible(s, false);
        }
    }

    void CreateBoundary()
    {
        // Check if it already exists (useful for [ExecuteAlways])
        Transform existing = transform.Find("TrackingBoundary");
        if (existing != null) {
            boundaryLine = existing.GetComponent<LineRenderer>();
            return;
        }

        GameObject boundaryObj = new GameObject("TrackingBoundary");
        boundaryObj.transform.parent = this.transform;
        boundaryObj.transform.localPosition = Vector3.zero;
        boundaryLine = boundaryObj.AddComponent<LineRenderer>();
        
        boundaryLine.useWorldSpace = true; 
        boundaryLine.positionCount = 5;
        boundaryLine.loop = true;
        boundaryLine.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        // Only run the boundary logic in Editor; run everything in Play mode
        if (!Application.isPlaying)
        {
            if (boundaryLine == null) CreateBoundary();
            UpdateVisualSettings();
            return;
        }

        UpdateVisualSettings();

        if (receiver == null || string.IsNullOrEmpty(receiver.lastReceivedPacket)) return;

        string[] personDataStrings = receiver.lastReceivedPacket.Split('#');
        framePersons.Clear();

        for (int i = 0; i < personDataStrings.Length; i++)
        {
            string pData = personDataStrings[i];
            if (string.IsNullOrEmpty(pData)) continue;
            int pipeIndex = pData.IndexOf('|');
            string coordString = (pipeIndex != -1) ? pData.Substring(pipeIndex + 1) : pData;
            int firstComma = coordString.IndexOf(',');
            if (firstComma != -1 && float.TryParse(coordString.Substring(0, firstComma), NumberStyles.Any, CultureInfo.InvariantCulture, out float xPos))
                framePersons.Add(new PersonData { rawData = coordString, xPos = xPos });
        }

        framePersons.Sort((a, b) => a.xPos.CompareTo(b.xPos));

        for (int i = 0; i < skeletons.Count; i++)
        {
            if (i < framePersons.Count)
            {
                UpdateSkelett(framePersons[i].rawData, skeletons[i]);
                SetSkeletonVisible(skeletons[i], true);
            }
            else SetSkeletonVisible(skeletons[i], false);
        }
    }

    // This draws the lines in the Scene view using Unity Gizmos
    void OnDrawGizmos()
    {
        if (showBoundary)
        {
            Gizmos.color = boundaryColor;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            // Define local corners
            Vector3 tr = new Vector3(8f, 4.5f, 0);
            Vector3 tl = new Vector3(-8f, 4.5f, 0);
            Vector3 br = new Vector3(8f, -4.5f, 0);
            Vector3 bl = new Vector3(-8f, -4.5f, 0);

            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }
    }

    void UpdateSkelett(string pData, Skeleton skel)
    {
        string[] coords = pData.Split(',');
        for (int i = 0; i < skel.jointTransforms.Length; i++)
        {
            int startIndex = i * 2;
            if (startIndex + 1 < coords.Length)
            {
                float x = float.Parse(coords[startIndex], CultureInfo.InvariantCulture);
                float y = float.Parse(coords[startIndex + 1], CultureInfo.InvariantCulture);

                Vector3 targetLocalPos = new Vector3(x * 16f - 8f, (1 - y) * 9f - 4.5f, 0);
                skel.jointTransforms[i].localPosition = Vector3.Lerp(skel.jointTransforms[i].localPosition, targetLocalPos, Time.deltaTime * 20f);
            }
        }

        for (int l = 0; l < skel.lines.Length; l++)
        {
            skel.lines[l].SetPosition(0, skel.jointTransforms[connections[l, 0]].position);
            skel.lines[l].SetPosition(1, skel.jointTransforms[connections[l, 1]].position);
        }
    }

    void UpdateVisualSettings()
    {
        float sFactor = transform.lossyScale.x;

        if (boundaryLine != null) {
            boundaryLine.enabled = showBoundary;
            boundaryLine.startWidth = boundaryLine.endWidth = boundaryThickness * sFactor;
            boundaryLine.startColor = boundaryLine.endColor = boundaryColor;

            Vector3[] corners = new Vector3[] {
                transform.TransformPoint(new Vector3(-8f, -4.5f, 0)),
                transform.TransformPoint(new Vector3(-8f, 4.5f, 0)),
                transform.TransformPoint(new Vector3(8f, 4.5f, 0)),
                transform.TransformPoint(new Vector3(8f, -4.5f, 0)),
                transform.TransformPoint(new Vector3(-8f, -4.5f, 0))
            };
            boundaryLine.SetPositions(corners);
        }

        if (Application.isPlaying)
        {
            foreach (var s in skeletons) {
                for (int i = 0; i < s.jointTransforms.Length; i++)
                    s.jointTransforms[i].localScale = Vector3.one * jointVisualScale;
                
                for (int i = 0; i < s.lines.Length; i++)
                    s.lines[i].startWidth = s.lines[i].endWidth = lineThickness * sFactor;
            }
        }
    }

    void SetSkeletonVisible(Skeleton skel, bool state)
    {
        if (skel.jointObjs[0] == null || skel.jointObjs[0].activeSelf == state) return;
        foreach (var j in skel.jointObjs) if(j != null) j.SetActive(state);
        foreach (var l in skel.lines) if(l != null) l.enabled = state;
    }
}