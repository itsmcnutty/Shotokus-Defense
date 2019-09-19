using System.Collections.Generic;

public class LocationWaves
{
    public Queue<Wave> waves;

    public LocationWaves(Queue<Wave> waves)
    { 
        this.waves = waves;
    }

    public Wave GetNextWave()
    {
        return (waves.Count > 0) ? waves.Dequeue() : null;
    }
    
    
}