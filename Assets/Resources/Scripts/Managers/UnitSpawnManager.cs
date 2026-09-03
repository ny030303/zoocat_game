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
        // 문자열(.ToString) 비교 대신 거리 비교 — Unity 버전별 Vector 포맷/부동소수 오차에 안전
        int best = -1;
        float bestSqr = 0.25f; // 허용 오차(≈0.5 유닛). 셀 간격보다 훨씬 작음
        for (int bi = 0; bi < availablePositions.Count; bi++)
        {
            float sqr = (availablePositions[bi] - targetPosition).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = bi; }
        }
        return best;
    }

    private int getAvailablePosition_legacy_UNUSED(Vector2 targetPosition)
    {
        return -1; // legacy .ToString() 매칭 제거됨
#pragma warning disable
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

    // ---- Phase C 대전 미러용 (셀 인덱스 <-> 위치) ----
    public int CellCount => rows * columns;

    public Vector2 CellToPosition(int cell)
    {
        if (availablePositions == null) Initialize();
        if (cell < 0 || cell >= availablePositions.Count) return new Vector2(-1, -1);
        return availablePositions[cell];
    }

    /// 월드/로컬 위치를 그리드 셀 인덱스로. 못 찾으면 -1.
    public int PositionToCell(Vector2 position)
    {
        if (availablePositions == null) Initialize();
        return getAvailablePosition(position);
    }

    /// 지정 셀에 유닛 스폰(상대 스폰 재현). owner 가 "ai" 면 unitID 로 조회된다.
    public GameObject SpawnAt(int cell, string unitID)
    {
        Vector2 pos = CellToPosition(cell);
        if (pos.x < 0 && pos.y < 0) { Debug.LogWarning("[UnitSpawnManager] SpawnAt bad cell " + cell); return null; }
        return SpawnNextAlly(pos, unitID);
    }
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
                if (idx < 0)
                {
                    Debug.LogWarning("[UnitSpawnManager] SpawnNextAlly: 위치 " + spawnPos + " 가 그리드 셀과 매칭 안 됨 - 스폰 취소");
                    return null;
                }
                availableState[idx] = 1;
                mergeIdx = idx;
            }

            // �����ϰ� �Ʊ� ������ ����
            UnitData unitData = null;
            if (owner == "player") unitData = unitDatabase.GetUnitDataRandom(owner);
            else if (owner == "ai") { unitData = unitDatabase.GetUnitData(owner, unitID);}

            if (unitData == null || unitData.unitPrefab == null)
            {
                Debug.LogError("[UnitSpawnManager] SpawnNextAlly: unitData/unitPrefab null (owner=" + owner + ", id=" + unitID + ") - 스폰 취소");
                return null;
            }

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
        if (idx >= 0) availableState[idx] = 0;
    }

    // ������ ����� state�� ����� Kill
    public void KillUnit(GameObject unit)
    {
        unit.SetActive(false);
        Destroy(unit, 1f);
    }
}
