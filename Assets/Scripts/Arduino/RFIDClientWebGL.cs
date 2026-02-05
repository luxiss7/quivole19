using UnityEngine;
using NativeWebSocket;
using System.Text;

public class RFIDClientWebGL : MonoBehaviour
{
    WebSocket websocket;
    public string websocketURL = "ws://127.0.0.1:8765"; // serveur Python WebSocket

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        Debug.Log("RFIDClient WebGL démarré");

        websocket = new WebSocket(websocketURL);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connecté au serveur WebSocket");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("WebSocket Erreur: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket Fermé: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            Debug.Log("Reçu WebSocket: " + json);

            // 1️⃣ Lire le type de capteur
            SensorMessage msg = JsonUtility.FromJson<SensorMessage>(json);

            switch (msg.capteur)
            {
                case "rfid":
                    RFIDData rfid = JsonUtility.FromJson<RFIDData>(json);
                    HandleRFID(rfid);
                    break;

                case "couleur":
                    ColorData color = JsonUtility.FromJson<ColorData>(json);
                    HandleColor(color);
                    break;

                default:
                    Debug.LogWarning("Type de capteur inconnu: " + msg.capteur);
                    break;
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
        // ✅ Pas besoin de DispatchMessageQueue dans cette version de NativeWebSocket
        // Les callbacks OnMessage sont appelés automatiquement
    }

    void HandleRFID(RFIDData data)
    {
        Debug.Log("Lecteur " + data.lecteur + " → " + data.role);
        RFIDEventManager.TriggerRFID(data.lecteur, data.role);
    }

    void HandleColor(ColorData data)
    {
        Debug.Log("Couleur détectée → " + data.color + " (" + data.value + ")");
        ColorEventManager.TriggerColor(data.color, data.value);
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }
}