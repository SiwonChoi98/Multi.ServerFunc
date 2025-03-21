using UnityEngine;
using System.Collections.Generic;
public class InventoryUI : BasePopUP
{
    public static InventoryUI instance = null;
    
    public ItemPart ItemObject;
    public Transform Content;
    List<GameObject> Gorvage = new List<GameObject>();

    public override void Awake()
    {
        if(instance == null)
            instance = this;

        BaseManager.Inventory.LoadInventory(() => GetInventoryData());

        base.Awake();
    }

    private void OnEnable()
    {
        BaseManager.Inventory.LoadInventory(() => GetInventoryData());
    }

    private void GetInventoryData()
    {
        if (Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }
        var data = BaseManager.Inventory.inventory;
        foreach(var item in data)
        {
            var go = Instantiate(ItemObject, Content);
            go.Initalize(item.Value);

            Gorvage.Add(go.gameObject);
        }
    }
  
}
