using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ITEM
{
    public ITEM_OBJ item;
    public int count;

    public ITEM(ITEM_OBJ name, int count)
    {
        this.item = name;
        this.count = count;
    }
}

public class InventoryManager : MonoBehaviour
{
    public Dictionary<string, ITEM> inventory = new Dictionary<string, ITEM>();

    public ITEM_OBJ ITEMDATA(string itemName)
    {
        return Resources.Load<ITEM_OBJ>("Scriptable/" + itemName);
    }

    public async void AddItem(string itemName, int amount)
    {
        if(inventory.ContainsKey(itemName))
        {
            inventory[itemName].count += amount;
        }
        else
        {
            inventory[itemName] = new ITEM(ITEMDATA(itemName), amount);
        }

        DocumentReference itemRef = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID)
            .Collection("INVENTORY").Document(itemName);
        Dictionary<string, object> itemData = new Dictionary<string, object>
         {
             {"name", itemName },
             {"count", inventory[itemName].count },
             {"acquiredAt", FieldValue.ServerTimestamp}
         };
        // MergeAll - 기존 데이터는 유지하고, 새로운 필드만 업데이트
        // MergeFields - (string) <- 매개변수에 대한 값만 변경
        await itemRef.SetAsync(itemData, SetOptions.MergeAll);
        Debug.Log($"아이템 {itemName} ({amount}개) Firestore에 저장됨!");
    }

    public async void RemoveItem(string itemName, int amount)
    {
        if(inventory.ContainsKey(itemName))
        {
            inventory[itemName].count -= amount;
            if (inventory[itemName].count <= 0)
            {
                inventory.Remove(itemName);

                await BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID)
                    .Collection("INVENTORY").Document(itemName).DeleteAsync();
                Debug.Log($"아이템 {itemName} Firestore에서 삭제됨!");
            }
            else
            {
                DocumentReference itemRef = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID)
                    .Collection("INVENTORY").Document(itemName);

                await itemRef.UpdateAsync("count", inventory[itemName].count);
                Debug.Log($"아이템 {itemName} Firestore에서 업데이트됨! 현재 개수 : {inventory[itemName].count}");
            }
        }
    }

    public async void LoadInventory(Action action = null)
    {
        Query inventoryQuery = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID)
            .Collection("INVENTORY");

        QuerySnapshot snapshot = await inventoryQuery.GetSnapshotAsync();

        foreach(DocumentSnapshot doc in snapshot.Documents)
        {
            string itemName = doc.GetValue<string>("name");
            int count = doc.GetValue<int>("count");

            inventory[itemName] = new ITEM(ITEMDATA(itemName), count);
        }

        if(action != null)
            action?.Invoke();

        Debug.Log("FireStore에서 인벤토리 로드 완료!");
    }
}
