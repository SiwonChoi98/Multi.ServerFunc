using UnityEngine;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

public class TradeUI : BasePopUP
{
    public static TradeUI instance = null;
    public override void Awake()
    {
        if (instance == null) instance = this;
        CancelButton.onClick.AddListener(() => BaseManager.Trade.CancelTrade());
        ConfirmButton.onClick.AddListener(() => BaseManager.Trade.ConfirmTrade());

        base.Awake();
    }
    public Button CancelButton;
    public Button ConfirmButton;
    public ItemPart ItemObject;
    public Transform InventoryContent;
    List<GameObject> Gorvage = new List<GameObject>();

    public Transform[] MyHolders;
    public Transform[] OtherPlayerHolders;
    List<GameObject> HolderGorvage = new List<GameObject>();

    public GameObject MyConfirm;
    public GameObject OtherPlayerConfirm;

    public void GetComfirm(bool itsMe)
    {
        if (itsMe) MyConfirm.SetActive(true);
        else OtherPlayerConfirm.SetActive(true);
    }
    private void OnEnable()
    {
        GetInventoryData();
    }

    public void SetHolderData()
    {
        if (HolderGorvage.Count > 0)
        {
            for (int i = 0; i < HolderGorvage.Count; i++) Destroy(HolderGorvage[i]);
            HolderGorvage.Clear();
        }

        if (BaseManager.Trade.playerItems.Count > 0)
        {
            int value = 0;
            foreach (var item in BaseManager.Trade.playerItems)
            {
                var go = Instantiate(ItemObject, MyHolders[value]);

                ITEM itemData = new ITEM(BaseManager.Inventory.ITEMDATA(item.Key), item.Value);
                go.Initalize(itemData);
                ItemObjectInitalzie(go.gameObject);
                value++;
            }
        }

        if(BaseManager.Trade.otherPlayerItems.Count > 0)
        {
            int value = 0;
            foreach (var item in BaseManager.Trade.otherPlayerItems)
            {
                var go = Instantiate(ItemObject, OtherPlayerHolders[value]);

                ITEM itemData = new ITEM(BaseManager.Inventory.ITEMDATA(item.Key), item.Value);
                go.Initalize(itemData);

                ItemObjectInitalzie(go.gameObject);
                value++;
            }
        }
    }

    private void ItemObjectInitalzie(GameObject go)
    {
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
       
        HolderGorvage.Add(go);
    }

    public void GetInventoryData()
    {
        if (Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }

        var data = BaseManager.Inventory.inventory;

        foreach (var item in data)
        {
            var go = Instantiate(ItemObject, InventoryContent);

            go.Initalize(item.Value, 
                () => 
            BaseManager.Trade.AddItemToTrade(item.Value.item.Name, 1), true);

            Gorvage.Add(go.gameObject);
        }
    }
}
