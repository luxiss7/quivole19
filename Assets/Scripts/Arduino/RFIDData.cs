[System.Serializable]
public class SensorMessage
{
    public string capteur;
}

[System.Serializable]
public class RFIDData
{
    public int lecteur;
    public string role;
}

[System.Serializable]
public class ColorData
{
    public string color;
    public int value;
}