using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 버튼 클릭시 좌표에 배치하는 스크립트
/// </summary>
public class SpawnAllies : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    public Transform parentTransform; // 아군을 배치할 부모 오브젝트
    public Vector2 startSpawnPosition = new Vector2(0, 0); // 시작 위치
    public Vector2 spawnOffset = new Vector2(140, 140); // 스폰 간격
    public int rows = 3; // 행 수
    public int columns = 5; // 열 수

    private Button spawnButton;
    private List<Vector2> availablePositions;
    private UnitSpawner unitSpawner;

    void Start()
    {
        // 버튼 컴포넌트를 가져와 클릭 이벤트에 메서드 연결
        spawnButton = GetComponent<Button>();
        unitSpawner = FindAnyObjectByType<UnitSpawner>();
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => { unitSpawner.SpawnNextAlly(); });
        }
        

        //// 가능한 모든 위치를 계산하여 리스트에 저장
        //availablePositions = new List<Vector2>();
        //for (int row = 0; row < rows; row++)
        //{
        //    for (int col = 0; col < columns; col++)
        //    {
        //        Vector2 position = startSpawnPosition + new Vector2(col * spawnOffset.x, -row * spawnOffset.y);
        //        availablePositions.Add(position);
        //    }
        //}
    }

    //void SpawnNextAlly()
    //{
    //    if (availablePositions.Count > 0)
    //    {
    //        // 가능한 위치들 중 랜덤하게 선택
    //        int randomPositionIndex = Random.Range(0, availablePositions.Count);
    //        Vector2 spawnPosition = availablePositions[randomPositionIndex];

    //        // 랜덤하게 아군 프리팹 선택
    //        UnitData unitData = unitDatabase.GetUnitDataRandom();

    //        // 선택된 아군 프리팹을 해당 위치에 생성
    //        GameObject newAlly = unitSpawner.SpawnUnit(unitData.unitName, spawnPosition);
    //        newAlly.transform.SetParent(parentTransform, false);

    //        // 유닛의 속성을 설정합니다.
    //        Unit unitset = newAlly.GetComponent<Unit>();
    //        if (unitset != null)
    //        {
    //            unitset.Initialize(unitData);
    //            // 유닛 삭제 시 위치를 다시 추가하도록 이벤트 등록
    //            unitset.OnUnitDestroyed += () => OnUnitDestroyed(spawnPosition);
    //        }

    //        // 사용된 위치를 리스트에서 제거
    //        availablePositions.RemoveAt(randomPositionIndex);
    //    }
    //}

    //void OnUnitDestroyed(Vector2 position)
    //{
    //    // 삭제된 유닛의 위치를 다시 추가
    //    availablePositions.Add(position);
    //}
}
