using UnityEngine;

public class PoseVisualizer : MonoBehaviour
{
    public UDPReceiver receiver;
    public GameObject noseCube;

    void Update()
    {
        string data = receiver.lastReceivedPacket;
        if (string.IsNullOrEmpty(data)) return;

        // Split the string into points
        string[] points = data.Split(',');
        if (points.Length >= 2)
        {
            float x = float.Parse(points[0]);
            float y = float.Parse(points[1]);

            // Map 0-1 coordinates to Unity World Space (adjust multipliers as needed)
            noseCube.transform.position = new Vector3(x * 10, (1 - y) * 10, 0);
        }
    }
}