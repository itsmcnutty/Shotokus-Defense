public class SpawnInfo {
    public enum SpawnLocation {
        North,
        South,
        East,
        West
    }

    private SpawnLocation location;
    private int numLightEnemies;
    private int numMedEnemies;
    private int numHeavyEnemies;

    public SpawnInfo(SpawnLocation location, int numLightEnemies, int numMedEnemies, int numHeavyEnemies)
    {
        this.location = location;
        this.numLightEnemies = numLightEnemies;
        this.numMedEnemies = numMedEnemies;
        this.numHeavyEnemies = numHeavyEnemies;
    }

    public SpawnLocation Location
    {
        get 
        {
            return location;
        }
    }

    public int NumLightEnemies
    {
        get 
        {
            return numLightEnemies;
        }
    }

    public int NumMedEnemies
    {
        get 
        {
            return numMedEnemies;
        }
    }

    public int NumHeavyEnemies
    {
        get 
        {
            return numHeavyEnemies;
        }
    }
}