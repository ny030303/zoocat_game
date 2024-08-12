using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 버튼 클릭시 좌표에 배치하는 스크립트
/// </summary>
public class SpawnButton : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    public Transform parentTransform; // 아군을 배치할 부모 오브젝트
    public Vector2 startSpawnPosition = new Vector2(0, 0); // 시작 위치
    public Vector2 spawnOffset = new Vector2(140, 140); // 스폰 간격
    public int rows = 3; // 행 수
    public int columns = 5; // 열 수

    private Button spawnButton;
    private List<Vector2> availablePositions;
    private UnitSpawnManager unitSpawnManager;

    void Start()
    {
        // 버튼 컴포넌트를 가져와 클릭 이벤트에 메서드 연결
        spawnButton = GetComponent<Button>();
        unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => { unitSpawnManager.SpawnNextAlly(); });
        }
    }
}
