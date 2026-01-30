using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System; // Für Exception Handling

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;

    [Header("Network Settings")]
    public int port = 6666; // Dieser Port muss im Router freigegeben sein (UDP)

    [Header("Debug Data")]
    public string lastReceivedPacket = "";
    public bool isReceiving = false;

    void Start()
    {
        // Thread starten, damit Unity nicht ruckelt während er auf Daten wartet
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("UDP Receiver gestartet auf Port: " + port);
    }

    private void ReceiveData()
    {
        try
        {
            // Initialisiert den Client für den spezifischen Port
            client = new UdpClient(port);

            // IPEndPoint.Any erlaubt den Empfang von JEDER IP-Adresse (auch öffentliche)
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                // Wartet hier auf ein Paket (Blockiert den Thread, nicht Unity)
                byte[] data = client.Receive(ref remoteEndPoint);

                // Konvertiert die Bytes in einen String (UTF8)
                string text = Encoding.UTF8.GetString(data);

                // Daten für den Haupt-Thread (Unity) bereitstellen
                lastReceivedPacket = text;
                isReceiving = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Error: " + e.Message);
        }
    }

    // WICHTIG: Den Port schließen, wenn Unity gestoppt wird
    void OnDisable()
    {
        CloseConnection();
    }

    void OnApplicationQuit()
    {
        CloseConnection();
    }

    private void CloseConnection()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
        Debug.Log("UDP Verbindung geschlossen.");
    }
}