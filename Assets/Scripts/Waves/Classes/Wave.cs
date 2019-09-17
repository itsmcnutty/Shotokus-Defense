using System.Collections.Generic;
using System.Linq;

public class Wave
{
    private Dictionary<float, SpawnInfo> waveSections;

    public Wave(Dictionary<float, SpawnInfo> waveSections)
    {
        this.waveSections = waveSections;
    }

    public Wave(float[] times, SpawnInfo[] timeSpawns)
    {
        if (times.Length == timeSpawns.Length)
        {
            for (int i = 0; i < times.Length; i++)
            {
                this.waveSections.Add(times[i], timeSpawns[i]);
            }
        }
        else
        {
            throw new System.Exception("The number of times and spawn information must be the same!");
        }
    }

    public SpawnInfo GetSpawnAtTime(float time)
    {
        SpawnInfo info;
        float? minTime = waveSections.Min(key => key.Key);
        if(minTime != null && minTime.Value < time)
        {
            time = minTime.Value;
        }

        if (waveSections.TryGetValue(time, out info))
        {
            waveSections.Remove(time);
            return info;
        }

        return null;
    }

    public SpawnInfo GetNextSpawnTimeInfo()
    {
        float? minTime = waveSections.Min(key => key.Key);
        if (minTime != null)
        {
            return GetSpawnAtTime(minTime.Value);
        }

        return null;
    }
}