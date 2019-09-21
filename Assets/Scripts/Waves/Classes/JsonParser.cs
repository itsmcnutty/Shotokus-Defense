using UnityEngine;
using Valve.Newtonsoft.Json;

public static class JsonParser
{
    
    public static LocationWaves parseJson(TextAsset file)
    {
        return JsonConvert.DeserializeObject<LocationWaves>(file.text);
    }

}
