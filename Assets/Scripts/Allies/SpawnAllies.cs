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
    }
}
