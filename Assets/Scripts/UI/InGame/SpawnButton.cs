using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 버튼 클릭시 좌표에 배치하는 스크립트
/// </summary>
public class SpawnButton : MonoBehaviour
{
    private Button spawnButton;
    private UnitSpawnManager unitSpawnManager;
    private GameManager gameManager;

    void Start()
    {
        // 버튼 컴포넌트를 가져와 클릭 이벤트에 메서드 연결
        spawnButton = GetComponent<Button>();
        //unitSpawnManager = FindAnyObjectByType<UnitSpawnManager>();
        GameObject manager = GameObject.Find("UnitSpawnManager");
        unitSpawnManager = manager.GetComponent<UnitSpawnManager>();

        gameManager = FindAnyObjectByType<GameManager>();

        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(() => {
                if(gameManager.CheckButtonState() && unitSpawnManager.IsSpawnNext())
                {
                    gameManager.SummonUnit();
                    unitSpawnManager.SpawnNextAlly();
                } else
                {
                    Debug.Log("소환불가");
                }
                
            });
        }

        // 이벤트 구독
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
