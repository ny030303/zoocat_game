using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderUpdater : MonoBehaviour
{
    public UserData userdata;
    public TMP_Text gold;
    public TMP_Text gems;
    public TMP_Text username;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeInventory());
    }

    IEnumerator InitializeInventory()
    {
        // 데이터 로드가 완료될 때까지 대기
        while (UserManager.Instance == null || UserManager.Instance.currentUser == null)
        {
            yield return null;
        }

        // 데이터 할당
        userdata = UserManager.Instance.currentUser;
        UpdateHeaderData();
    }

    private void UpdateHeaderData()
    {
        username.text = userdata.username;
        gold.text = userdata.gold + "";
        gems.text = userdata.gems + "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
