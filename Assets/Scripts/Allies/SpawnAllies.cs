using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// ��ư Ŭ���� ��ǥ�� ��ġ�ϴ� ��ũ��Ʈ
/// </summary>
public class SpawnAllies : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    public Transform parentTransform; // �Ʊ��� ��ġ�� �θ� ������Ʈ
    public Vector2 startSpawnPosition = new Vector2(0, 0); // ���� ��ġ
    public Vector2 spawnOffset = new Vector2(140, 140); // ���� ����
    public int rows = 3; // �� ��
    public int columns = 5; // �� ��

    private Button spawnButton;
    private List<Vector2> availablePositions;
    private UnitSpawner unitSpawner;

    void Start()
    {
        // ��ư ������Ʈ�� ������ Ŭ�� �̺�Ʈ�� �޼��� ����
        spawnButton = GetComponent<Button>();
        unitSpawner = FindAnyObjectByType<UnitSpawner>();
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => { unitSpawner.SpawnNextAlly(); });
        }
        

        //// ������ ��� ��ġ�� ����Ͽ� ����Ʈ�� ����
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
    //        // ������ ��ġ�� �� �����ϰ� ����
    //        int randomPositionIndex = Random.Range(0, availablePositions.Count);
    //        Vector2 spawnPosition = availablePositions[randomPositionIndex];

    //        // �����ϰ� �Ʊ� ������ ����
    //        UnitData unitData = unitDatabase.GetUnitDataRandom();

    //        // ���õ� �Ʊ� �������� �ش� ��ġ�� ����
    //        GameObject newAlly = unitSpawner.SpawnUnit(unitData.unitName, spawnPosition);
    //        newAlly.transform.SetParent(parentTransform, false);

    //        // ������ �Ӽ��� �����մϴ�.
    //        Unit unitset = newAlly.GetComponent<Unit>();
    //        if (unitset != null)
    //        {
    //            unitset.Initialize(unitData);
    //            // ���� ���� �� ��ġ�� �ٽ� �߰��ϵ��� �̺�Ʈ ���
    //            unitset.OnUnitDestroyed += () => OnUnitDestroyed(spawnPosition);
    //        }

    //        // ���� ��ġ�� ����Ʈ���� ����
    //        availablePositions.RemoveAt(randomPositionIndex);
    //    }
    //}

    //void OnUnitDestroyed(Vector2 position)
    //{
    //    // ������ ������ ��ġ�� �ٽ� �߰�
    //    availablePositions.Add(position);
    //}
}
