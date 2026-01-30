using UnityEngine;
using System.Collections.Generic;
using System.Globalization; // WICHTIG für float.Parse

public class PoseManager : MonoBehaviour
{
    public UDPReceiver receiver;
    public GameObject jointPrefab;
    public GameObject facePrefab;

    private int maxSkeletons = 4;
    private List<Skeleton> skeletons = new List<Skeleton>();
    public Color[] personColors = { Color.red, Color.blue, Color.green, Color.yellow };

    // Verbindungen (Ohne Nase 0, da das Face-Prefab dort sitzt)
    private int[,] connections = new int[,] {
        {1, 2}, {1, 3}, {3, 5}, {2, 4}, {4, 6},
        {1, 7}, {2, 8}, {7, 8}, {7, 9}, {9, 11}, {8, 10}, {10, 12}
    };

    private class Skeleton
    {
        public GameObject[] joints = new GameObject[13];
        public LineRenderer[] lines = new LineRenderer[12];
    }

    private struct PersonData
    {
        public string rawData;
        public float xPos;
    }

    void Start()
    {
        for (int p = 0; p < maxSkeletons; p++)
        {
            Skeleton s = new Skeleton();
            for (int i = 0; i < 13; i++)
            {
                s.joints[i] = Instantiate(i == 0 ? facePrefab : jointPrefab, transform);
                if (i != 0) s.joints[i].GetComponent<Renderer>().material.color = personColors[p];
            }
            for (int l = 0; l < connections.GetLength(0); l++)
            {
                GameObject lineObj = new GameObject($"Line_{p}_{l}");
                lineObj.transform.parent = transform;
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.05f; lr.endWidth = 0.05f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = personColors[p]; lr.endColor = personColors[p];
                lr.positionCount = 2;
                s.lines[l] = lr;
            }
            skeletons.Add(s);
            SetSkeletonVisible(s, false);
        }
    }

    void Update()
    {
        if (receiver == null || string.IsNullOrEmpty(receiver.lastReceivedPacket)) return;

        string data = receiver.lastReceivedPacket;
        string[] personDataStrings = data.Split('#');
        List<PersonData> framePersons = new List<PersonData>();

        foreach (string pData in personDataStrings)
        {
            if (string.IsNullOrEmpty(pData)) continue;

            // ID abschneiden falls vorhanden (ID|x,y...), sonst bleibt der String gleich
            string coordString = pData.Contains("|") ? pData.Split('|')[1] : pData;
            string[] coords = coordString.Split(',');

            if (coords.Length >= 2)
            {
                // Sicherer Parse mit InvariantCulture (erkennt Punkt als Dezimaltrenner)
                if (float.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float xPos))
                {
                    framePersons.Add(new PersonData { rawData = coordString, xPos = xPos });
                }
            }
        }

        // Sortiere: Wer am kleinsten X hat (links), kommt zuerst in die Liste
        framePersons.Sort((a, b) => a.xPos.CompareTo(b.xPos));

        for (int i = 0; i < skeletons.Count; i++)
        {
            if (i < framePersons.Count)
            {
                UpdateSkelett(framePersons[i].rawData, skeletons[i]);
                SetSkeletonVisible(skeletons[i], true);
            }
            else
            {
                SetSkeletonVisible(skeletons[i], false);
            }
        }
    }

    void UpdateSkelett(string pData, Skeleton skel)
    {
        string[] coords = pData.Split(',');
        for (int i = 0; i < skel.joints.Length; i++)
        {
            int startIndex = i * 2;
            if (startIndex + 1 < coords.Length)
            {
                float x = float.Parse(coords[startIndex], CultureInfo.InvariantCulture);
                float y = float.Parse(coords[startIndex + 1], CultureInfo.InvariantCulture);

                Vector3 target = new Vector3(x * 16f - 8f, (1 - y) * 9f - 4.5f, 0);
                skel.joints[i].transform.position = Vector3.Lerp(skel.joints[i].transform.position, target, Time.deltaTime * 20f);
            }
        }

        for (int l = 0; l < connections.GetLength(0); l++)
        {
            skel.lines[l].SetPosition(0, skel.joints[connections[l, 0]].transform.position);
            skel.lines[l].SetPosition(1, skel.joints[connections[l, 1]].transform.position);
        }
    }

    void SetSkeletonVisible(Skeleton skel, bool state)
    {
        foreach (var j in skel.joints) if (j != null) j.SetActive(state);
        foreach (var l in skel.lines) if (l != null) l.enabled = state;
    }
}