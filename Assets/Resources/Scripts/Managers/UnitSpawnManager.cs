using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 유닛생성기
/// </summary>
public class UnitSpawnManager : MonoBehaviour
{
    public string owner;
    public UnitDatabase unitDatabase;
    public Transform parentTransform; // 아군을 배치할 부모 오브젝트
    public Vector2 startSpawnPosition = new Vector2((float)0, (float)0); // 시작 위치
    public Vector2 spawnOffset = new Vector2((float)1.2, (float)1.2); // 스폰 간격
    public int rows = 3; // 행 수
    public int columns = 5; // 열 수
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
            // 가능한 모든 위치를 계산하여 리스트에 저장
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

    // 배열의 모든 값을 더한 합을 반환하는 메서드
    private int GetSumOfAvailableState()
    {
        int sum = 0; // 합을 저장할 변수
        // 배열을 순회하면서 모든 요소의 값을 더합니다.
        for (int i = 0; i < availableState.Length; i++)
        {
            sum += availableState[i];
        }

        return sum; // 최종 합을 반환
    }

    public int getAvailablePosition(Vector2 targetPosition)
    {
        foreach (Vector2 pos in availablePositions)
        {
            if (pos.ToString() == targetPosition.ToString()) // Vector2는 == 연산자를 오버로드하여 동등성 비교가 가능합니다.
            {
                return availablePositions.IndexOf(pos);
            }
        }
        return -1;
    }
    public Transform GetParentTransform() { return parentTransform ? parentTransform : null; }
    public bool IsSpawnNext() { return GetSumOfAvailableState() < (rows * columns) ? true : false; }
    //생성 로직
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
                // 사용된 위치를 리스트에서 제거
                availableState[randomPositionIndex] = 1;

                
            } else {
                // 값의 인덱스 찾기
                int idx = getAvailablePosition(spawnPos);
                Debug.Log("idx: "+ idx);
                availableState[idx] = 1;
                mergeIdx = idx;
            }

            // 랜덤하게 아군 프리팹 선택
            UnitData unitData = null;
            if (owner == "player") unitData = unitDatabase.GetUnitDataRandom(owner);
            else if (owner == "ai") { unitData = unitDatabase.GetUnitData(owner, unitID);}
            // 선택된 아군 프리팹을 해당 위치에 생성
            newAlly = Instantiate(unitData.unitPrefab, spawnPos, Quaternion.identity);
            if (owner == "ai") newAlly.GetComponent<SpriteRenderer>().flipX = !newAlly.GetComponent<SpriteRenderer>().flipX; // ai 유닛이면 뒤집기
            newAlly.transform.SetParent(parentTransform, false);

            // 유닛의 속성을 설정합니다.
            Unit unitset = newAlly.GetComponent<Unit>();
            if (unitset != null)
            {
                unitset.Initialize(unitData, owner, spawnPos);
                // 유닛 삭제 시 위치를 다시 추가하도록 이벤트 등록
                unitset.OnUnitDestroyed += () => OnUnitDestroyed(spawnPos);
            }
            
            Debug.Log("availableState: " + string.Join(", ", availableState) );
            // 랜덤소환 일때만 스폰이벤트 기록 (+player일때만)
            if (defpos == null && owner == "player") eventManager.OnUnitSpawned(unitData.id, spawnPos);
        }
        return newAlly;
    }

    Vector2 spawnRandomPosition(int randNum)
    {
        // 가능한 위치들 중 랜덤하게 선택
        Vector2 spawnPosition = availablePositions[randNum];

        return spawnPosition;
    }

    void OnUnitDestroyed(Vector2 position)
    {
        // 삭제된 유닛의 위치를 다시 추가
        int idx = getAvailablePosition(position);
        //if(mergeIdx != idx) 
            availableState[idx] = 0;
    }

    // 유닛은 지우되 state는 남기는 Kill
    public void KillUnit(GameObject unit)
    {
        unit.SetActive(false);
        Destroy(unit, 1f);
    }
}
