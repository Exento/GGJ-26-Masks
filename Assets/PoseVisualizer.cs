using UnityEngine;

public class PoseManager : MonoBehaviour
{
    public UDPReceiver receiver;
    public GameObject jointPrefab;
    private GameObject[] jointsPerson1 = new GameObject[13];
    private GameObject[] jointsPerson2 = new GameObject[13];

    void Start()
    {
        // Erstelle 26 Kugeln insgesamt
        for (int i = 0; i < 13; i++)
        {
            jointsPerson1[i] = Instantiate(jointPrefab);
            jointsPerson2[i] = Instantiate(jointPrefab);
            // Optische Unterscheidung
            jointsPerson1[i].GetComponent<Renderer>().material.color = Color.red;
            jointsPerson2[i].GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    void Update()
    {
        string data = receiver.lastReceivedPacket;
        if (string.IsNullOrEmpty(data)) return;

        string[] persons = data.Split('#'); // Teile den String bei '#'

        // Verarbeite Person 1
        UpdateSkelett(persons[0], jointsPerson1);

        // Verarbeite Person 2 (falls vorhanden)
        if (persons.Length > 1)
        {
            UpdateSkelett(persons[1], jointsPerson2);
        }
    }

    void UpdateSkelett(string pData, GameObject[] jointArray)
    {
        string[] coords = pData.Split(',');
        for (int i = 0; i < jointArray.Length; i++)
        {
            int startIndex = i * 2;
            if (startIndex + 1 < coords.Length)
            {
                float x = float.Parse(coords[startIndex]);
                float y = float.Parse(coords[startIndex + 1]);
                Vector3 newPos = new Vector3(x * 16f - 8f, (1 - y) * 9f - 4.5f, 0);
                jointArray[i].transform.position = Vector3.Lerp(jointArray[i].transform.position, newPos, Time.deltaTime * 15f);
            }
        }
    }
}