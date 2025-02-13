using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyForBattlePanel : MonoBehaviour
{
    public SceneLoader sceneLoader; // SceneLoader 스크립트를 참조
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGameEnterButtonClicked()
    {
        // 게임 씬으로 전환
        sceneLoader.LoadScene("GameScene");
    }
}
