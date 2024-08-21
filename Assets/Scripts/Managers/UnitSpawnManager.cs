using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ���ֻ�����
/// </summary>
public class UnitSpawnManager : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    public Transform parentTransform; // �Ʊ��� ��ġ�� �θ� ������Ʈ
    public Vector2 startSpawnPosition = new Vector2((float)0, (float)0); // ���� ��ġ
    public Vector2 spawnOffset = new Vector2((float)1.2, (float)1.2); // ���� ����
    public int rows = 3; // �� ��
    public int columns = 5; // �� ��
    private GameEventManager eventManager;
    private List<Vector2> availablePositions;
    private int[] availableState;
    private int mergeIdx = -1;

    public delegate void UnitMergedHandler();

    void Start()
    {
        eventManager = FindAnyObjectByType<GameEventManager>();

        availableState = new int[rows * columns];
        // ������ ��� ��ġ�� ����Ͽ� ����Ʈ�� ����
        availablePositions = new List<Vector2>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 position = startSpawnPosition + new Vector2(col * spawnOffset.x, -row * spawnOffset.y);
                availablePositions.Add(position);
                availableState[row * columns + col] = 0;
                //Debug.Log(position);
            }
        }
    }

    // �迭�� ��� ���� ���� ���� ��ȯ�ϴ� �޼���
    private int GetSumOfAvailableState()
    {
        int sum = 0; // ���� ������ ����

        // �迭�� ��ȸ�ϸ鼭 ��� ����� ���� ���մϴ�.
        for (int i = 0; i < availableState.Length; i++)
        {
            sum += availableState[i];
        }

        return sum; // ���� ���� ��ȯ
    }

    private int getAvailablePosition(Vector2 targetPosition)
    {
        foreach (Vector2 pos in availablePositions)
        {
            if (pos.ToString() == targetPosition.ToString()) // Vector2�� == �����ڸ� �����ε��Ͽ� ��� �񱳰� �����մϴ�.
            {
                return availablePositions.IndexOf(pos);
            }
        }
        return -1;
    }

    public bool IsSpawnNext() { return GetSumOfAvailableState() < (rows * columns) ? true : false; }
    //���� ����
    public GameObject SpawnNextAlly(Vector2? defpos = null)
    {
        GameObject newAlly = null;
        if (GetSumOfAvailableState() < (rows* columns)) {
            mergeIdx = -1;
            Vector2 spawnPos = defpos ?? new Vector2(-1, -1);

            if (defpos == null) {
                //retry:
                //int randomPositionIndex = Random.Range(0, availablePositions.Count);                
                //if (availableState[randomPositionIndex] == 1) goto retry;
                bool validPositionFound = false;
                int randomPositionIndex = -1;

                while (!validPositionFound)
                {
                    randomPositionIndex = Random.Range(0, availablePositions.Count);
                    //Debug.Log("RandNum: "+ randomPositionIndex);
                    if (availableState[randomPositionIndex] != 1)
                    {
                        validPositionFound = true;
                    }
                }
                spawnPos = spawnRandomPosition(randomPositionIndex);
                // ���� ��ġ�� ����Ʈ���� ����
                availableState[randomPositionIndex] = 1;

                
            } else {
                // ���� �ε��� ã��
                int idx = getAvailablePosition(spawnPos);
                Debug.Log("idx: "+ idx);
                availableState[idx] = 1;
                mergeIdx = idx;
            }

            // �����ϰ� �Ʊ� ������ ����
            UnitData unitData = unitDatabase.GetUnitDataRandom();

            // ���õ� �Ʊ� �������� �ش� ��ġ�� ����
            newAlly = Instantiate(unitData.unitPrefab, spawnPos, Quaternion.identity);
            newAlly.transform.SetParent(parentTransform, false);

            // ������ �Ӽ��� �����մϴ�.
            Unit unitset = newAlly.GetComponent<Unit>();
            if (unitset != null)
            {
                unitset.Initialize(unitData);
                // ���� ���� �� ��ġ�� �ٽ� �߰��ϵ��� �̺�Ʈ ���
                unitset.OnUnitDestroyed += () => OnUnitDestroyed(spawnPos);
            }
            
            Debug.Log("availableState: " + string.Join(", ", availableState) );
            // ������ȯ �϶��� �����̺�Ʈ ���
            if (defpos == null) eventManager.OnUnitSpawned(unitData.id, spawnPos);
        }
        return newAlly;
    }

    Vector2 spawnRandomPosition(int randNum)
    {
        // ������ ��ġ�� �� �����ϰ� ����
        Vector2 spawnPosition = availablePositions[randNum];

        return spawnPosition;
    }

    void OnUnitDestroyed(Vector2 position)
    {
        // ������ ������ ��ġ�� �ٽ� �߰�
        int idx = getAvailablePosition(position);
        //if(mergeIdx != idx) 
            availableState[idx] = 0;
    }

    // ������ ����� state�� ����� Kill
    public void KillUnit(GameObject unit)
    {
        //parentTransform = unit.gameObject.transform.parent;
        //Vector2 unitpos = new Vector2(unit.transform.position.x, unit.transform.position.y);
        //// ���� ��ǥ�迡���� ��ġ�� ���� ��ǥ��� ��ȯ
        //Vector2 localPos = parentTransform.InverseTransformPoint(unitpos);

        //int idx = getAvailablePosition(localPos);
        //availableState[idx] = 0;
        //OnUnitDestroyed(localPos);
        unit.SetActive(false);
        Destroy(unit, 1f);
    }
}
