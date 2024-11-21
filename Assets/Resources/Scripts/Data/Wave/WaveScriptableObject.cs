using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "scriptable Object/Enemy/Round")]
public class WaveScriptableObject : ScriptableObject
{
    public int roundNumber;
    public UnitData[] enemies;
    public int[] enemyCounts; // 각 에너미에 대한 스폰 수량

    // 추가적으로 라운드별 보상이나 조건 등을 정의할 수 있습니다.
}
