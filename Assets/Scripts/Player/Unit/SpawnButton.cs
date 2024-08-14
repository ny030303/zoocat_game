using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// ��ư Ŭ���� ��ǥ�� ��ġ�ϴ� ��ũ��Ʈ
/// </summary>
public class SpawnButton : MonoBehaviour
{
    private Button spawnButton;
    private List<Vector2> availablePositions;
    private UnitSpawnManager unitSpawnManager;
    private GameManager gameManager;

    void Start()
    {
        // ��ư ������Ʈ�� ������ Ŭ�� �̺�Ʈ�� �޼��� ����
        spawnButton = GetComponent<Button>();
        unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        gameManager = FindAnyObjectByType<GameManager>();

        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => {
                if(gameManager.SummonUnit())
                {
                    unitSpawnManager.SpawnNextAlly();
                } else
                {
                    Debug.Log("��ȯ�Ұ�");
                }
                
            });
        }

        // �̺�Ʈ ����
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged += changeState;
        }
    }

    public void changeState()
    {
        if(gameManager.CheckButtonState())
        {
            spawnButton.interactable = true;
        } else
        {
            spawnButton.interactable = false;
        }
    }
}
