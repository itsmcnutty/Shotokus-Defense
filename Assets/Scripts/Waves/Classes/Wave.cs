using System.Collections.Generic;
using System.Linq;

public class Wave
{
    public Dictionary<float, SpawnInfo> waveSections;

    public Wave()
    {
        this.waveSections = new Dictionary<float, SpawnInfo>();
    }

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
        if(waveSections.Count == 0)
        {
            return null;
        }

        float? minTime = waveSections.Min(wave => wave.Key);
        if(minTime != null && minTime.Value < time)
        {
            time = minTime.Value;
        }

        if (waveSections.TryGetValue(time, out SpawnInfo info))
        {
            waveSections.Remove(time);
            return info;
        }
        
        return (waveSections.Count > 0) ? new SpawnInfo(SpawnInfo.SpawnLocation.None, 0, 0, 0): null;
    }

    public SpawnInfo GetNextSpawnTimeInfo(out float? nextTime)
    {
        if(waveSections.Count == 0)
        {
            nextTime = -1;
            return null;
        }

        nextTime = waveSections.Min(wave => wave.Key);
        return (nextTime != null)? GetSpawnAtTime(nextTime.Value) : null;
    }
}