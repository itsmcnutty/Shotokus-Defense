using System.Collections.Generic;

public class LocationWaves
{
    private Queue<Wave> waves;
    private Wave currentWave;

    public LocationWaves(Queue<Wave> waves)
    { 
        this.waves = waves;
    }

    public Wave GetNextWave()
    {
        return (waves.Count > 0) ? waves.Dequeue() : null;
    }
    
}