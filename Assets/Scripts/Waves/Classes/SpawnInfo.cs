public class SpawnInfo {
    public enum SpawnLocation {
        North,
        South,
        East,
        West,
        None
    }

    public SpawnLocation Location;
    public int NumLightEnemies;
    public int NumMedEnemies;
    public int NumHeavyEnemies;

    public SpawnInfo(SpawnLocation location, int numLightEnemies, int numMedEnemies, int numHeavyEnemies)
    {
        this.Location = location;
        this.NumLightEnemies = numLightEnemies;
        this.NumMedEnemies = numMedEnemies;
        this.NumHeavyEnemies = numHeavyEnemies;
    }
}