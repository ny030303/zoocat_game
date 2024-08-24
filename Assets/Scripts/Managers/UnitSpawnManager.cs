using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ���ֻ�����
/// </summary>
public class UnitSpawnManager : MonoBehaviour
{
    public string owner;
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
        Initialize();
    }
    public void Initialize() {
        eventManager = FindObjectOfType<GameEventManager>();
        if (eventManager == null) Debug.LogError("GameEventManager not found!");

        if(availableState == null)
        {
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

    public int getAvailablePosition(Vector2 targetPosition)
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
    public Transform GetParentTransform() { return parentTransform ? parentTransform : null; }
    public bool IsSpawnNext() { return GetSumOfAvailableState() < (rows * columns) ? true : false; }
    //���� ����
    public GameObject SpawnNextAlly(Vector2? defpos = null, string unitID = null)
    {
        GameObject newAlly = null;
        if (GetSumOfAvailableState() < (rows* columns)) {
            mergeIdx = -1;
            Vector2 spawnPos = defpos ?? new Vector2(-1, -1);

            if (defpos == null) {
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
            UnitData unitData = null;
            if (owner == "player") unitData = unitDatabase.GetUnitDataRandom(owner);
            else if (owner == "ai") { unitData = unitDatabase.GetUnitData(owner, unitID);}
            // ���õ� �Ʊ� �������� �ش� ��ġ�� ����
            newAlly = Instantiate(unitData.unitPrefab, spawnPos, Quaternion.identity);
            if (owner == "ai") newAlly.GetComponent<SpriteRenderer>().flipX = !newAlly.GetComponent<SpriteRenderer>().flipX; // ai �����̸� ������
            newAlly.transform.SetParent(parentTransform, false);

            // ������ �Ӽ��� �����մϴ�.
            Unit unitset = newAlly.GetComponent<Unit>();
            if (unitset != null)
            {
                unitset.Initialize(unitData, owner, spawnPos);
                // ���� ���� �� ��ġ�� �ٽ� �߰��ϵ��� �̺�Ʈ ���
                unitset.OnUnitDestroyed += () => OnUnitDestroyed(spawnPos);
            }
            
            Debug.Log("availableState: " + string.Join(", ", availableState) );
            // ������ȯ �϶��� �����̺�Ʈ ��� (+player�϶���)
            if (defpos == null && owner == "player") eventManager.OnUnitSpawned(unitData.id, spawnPos);
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
        unit.SetActive(false);
        Destroy(unit, 1f);
    }
}
