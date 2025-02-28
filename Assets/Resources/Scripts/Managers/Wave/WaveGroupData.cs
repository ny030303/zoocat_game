using System.Collections.Generic;

public class WaveGroupData
{
    public int id;
    public bool isBoss;
    public List<string> monsterIds;
    public List<int> monsterCounts;

    public WaveGroupData(int id, bool isBoss, List<string> monsterIds, List<int> monsterCounts)
    {
        this.id = id;
        this.isBoss = isBoss;
        this.monsterIds = monsterIds;
        this.monsterCounts = monsterCounts;
    }
}
