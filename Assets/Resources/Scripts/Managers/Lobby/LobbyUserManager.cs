using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUserManager : MonoBehaviour
{
    public UnitDatabase unitDatabase;
    // �� ���� �̺�Ʈ �߰�
    public Action<List<UnitData>> OnUnitDeckChanged;

    void Awake()
    {
        if (UserManager.Instance == null)
        {
            Debug.LogWarning("UserManager.Instance is null!");
            return;
        }

        // ���� ������ �ε�
        UserData userdata = UserManager.Instance.currentUser;
        if (userdata != null)
        {
            // selectedUnits �����͸� ������� �� �ʱ�ȭ
            InitializeUnitDeck(userdata.selectedUnits);
        }
        else
        {
            Debug.LogWarning("userdata is null!");
        }
    }

    public void AddUnitToDeck(UnitData unit)
    {
        if (!unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Add(unit);
            OnUnitDeckChanged?.Invoke(unitDatabase.unitDeck); // �̺�Ʈ ȣ��
        }
    }

    public void RemoveUnitFromDeck(UnitData unit)
    {
        if (unitDatabase.unitDeck.Contains(unit))
        {
            unitDatabase.unitDeck.Remove(unit);
            OnUnitDeckChanged?.Invoke(unitDatabase.unitDeck); // �̺�Ʈ ȣ��
        }
    }

    // ���� �� �ʱ�ȭ
    public void InitializeUnitDeck(string[] selectedUnitIds)
    {
        // �� �ʱ�ȭ
        unitDatabase.unitDeck.Clear();

        foreach (string unitId in selectedUnitIds)
        {
            // ScriptableObject ��� ����
            string unitPath = $"Scripts/Data/UnitData/Unit_UnitData/CHA_{unitId:D4}";
            UnitData unit = Resources.Load<UnitData>(unitPath);

            if (unit != null)
            {
                unitDatabase.unitDeck.Add(unit);
                Debug.Log($"{unit.unitName}��(��) ���� �߰��Ǿ����ϴ�.");
            }
            else
            {
                Debug.LogWarning($"���� {unitId}��(��) �ε��� �� �����ϴ�. ���: {unitPath}");
            }
        }

        Debug.Log("���� �� �ʱ�ȭ �Ϸ�");
        PrintUnitDeck(); // �ʱ�ȭ �� ��� ���
    }

    // �� �ʱ�ȭ
    public void ClearUnitDeck()
    {
        unitDatabase.unitDeck.Clear();
        Debug.Log("���� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    // �� ��ü
    public void ReplaceUnitDeck(List<UnitData> newDeck)
    {
        unitDatabase.unitDeck = newDeck;
        Debug.Log("���ο� ������ ��ü�Ǿ����ϴ�.");
    }

    // �� ��� (������)
    public void PrintUnitDeck()
    {
        Debug.Log("���� ���� ��:");
        foreach (var unit in unitDatabase.unitDeck)
        {
            Debug.Log($"- {unit.unitName}");
        }
    }
}
