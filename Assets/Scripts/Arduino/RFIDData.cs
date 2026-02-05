[System.Serializable]
public class SensorMessage
{
    public string capteur;
}

[System.Serializable]
public class RFIDData
{
    public string capteur;
    public int lecteur;
    public string role;
}

[System.Serializable]
public class ColorData
{
    public string capteur;
    public string color;
    public int value;
}