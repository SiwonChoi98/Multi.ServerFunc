using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;
public class ItemPart : MonoBehaviour
{
    public Button button;
    public Image itemImage;
    public Text itemCountText;
    ITEM mainData;
    public void Initalize(ITEM item , Action action = null, bool isTrade = false)
    {
        mainData = new ITEM(item.item, item.count);
        itemImage.sprite = ActionHolder.GetAtlas(item.item.Name);
        ItemCountCheck();

        button.enabled = action != null;

        if(action != null)
        {
            button.onClick.AddListener(() => action());
            if(isTrade)
                button.onClick.AddListener(() => CountMinus());
        }
    }

    private void ItemCountCheck()
    {
        itemCountText.text = mainData.count.ToString();
    }

    private void CountMinus()
    {
        mainData.count--;
        ItemCountCheck();
        if (mainData.count <= 0)
        {
            button.onClick.RemoveAllListeners();
            itemImage.color = Color.black;
        }
    }
}
