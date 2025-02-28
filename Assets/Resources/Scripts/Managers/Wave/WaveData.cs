public class WaveData
{
    public int id;
    public float timeLimit;
    public int hpAdditional;
    public int spdAdditional;
    public int rewardId;
    public int waveGroupId; // 웨이브그룹 ID

    public WaveData(int id, float timeLimit, int hpAdditional, int spdAdditional, int rewardId, int waveGroupId)
    {
        this.id = id;
        this.timeLimit = timeLimit;
        this.hpAdditional = hpAdditional;
        this.spdAdditional = spdAdditional;
        this.rewardId = rewardId;
        this.waveGroupId = waveGroupId;
    }
}
