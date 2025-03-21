using UnityEngine;
using UnityEngine.UI;

public class AuctionPart : MonoBehaviour
{
    AuctionUI parentAuction;
    AuctionData m_Data;
    [SerializeField] private GameObject Mark;
    [SerializeField] private Image ItemImage;
    [SerializeField] private Text SellerText;
    [SerializeField] private Text StartingPriceText;
    [SerializeField] private Text CurrentPriceText;
    [SerializeField] private Text BuyOutText;
    [SerializeField] private Text EndTimeText;

    public void Initalize(AuctionData data)
    {
        parentAuction = AuctionUI.instance;
        m_Data = data;
        ItemImage.sprite = ActionHolder.GetAtlas(data.itemName);
        SellerText.text = data.sellerNickName;
        StartingPriceText.text = data.startingPrice;
        CurrentPriceText.text = data.currentPrice;
        BuyOutText.text = data.buyOutPrice;
        EndTimeText.text = data.EndTime;
    }

    public void SetClickButton()
    {
        parentAuction.ResetParts();
        MarkActive(true);
        parentAuction.GetData(m_Data);
    }

    public void MarkActive(bool isActive)
    {
        Mark.SetActive(isActive);
    }
}
