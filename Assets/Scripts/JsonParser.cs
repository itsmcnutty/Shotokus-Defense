using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class JsonParser
{
    
    public static LocationWaves parseJson(string path)
    {
        string jsonString = File.ReadAllText (Application.dataPath + "/Scripts/Waves/Wave Json Files/Location_1.json");
        return JsonConvert.DeserializeObject<LocationWaves>(jsonString);
    }

}
