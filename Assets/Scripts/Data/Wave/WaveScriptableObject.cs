using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "scriptable Object/Enemy/Round")]
public class WaveScriptableObject : ScriptableObject
{
    public int roundNumber;
    public UnitData[] enemies;
    public int[] enemyCounts; // �� ���ʹ̿� ���� ���� ����

    // �߰������� ���庰 �����̳� ���� ���� ������ �� �ֽ��ϴ�.
}
