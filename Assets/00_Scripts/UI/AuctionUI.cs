using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using WebSocketSharp;
public class AuctionUI : BasePopUP
{
    public static AuctionUI instance = null;
    List<GameObject> Gorvage = new List<GameObject>();
    List<AuctionPart> Parts = new List<AuctionPart>();
    AuctionData m_Data;
    ITEM_OBJ mainObj = null;
    GameObject SetItemPopUP;
    GameObject BidPopUP;

    [Header("## Auction")]
    [SerializeField] private AuctionPart part;
    [SerializeField] private Transform Content;

    [Header("## SetItem")]
    [SerializeField] private ItemPart ItemObject;
    [SerializeField] private Transform InventoryContent;
    [SerializeField] private Image itemImage;
    [SerializeField] private Text ItemName;
    [SerializeField] private InputField itemPriceInputField;

    [Header("## Bid")]
    [SerializeField] private Image Bid_ItemImage;
    [SerializeField] private Text Bid_ItemText;
    [SerializeField] private InputField Bid_InputField;

    public override void Awake()
    {
        if (instance == null) instance = this;
        SetItemPopUP = transform.Find("SetItem_Auction").gameObject;
        BidPopUP = transform.Find("ItemBid").gameObject;
        Bid_InputField.onEndEdit.AddListener(OnEndEdit);
        ResetPopUP();
        base.Awake();
    }

    private void OnEnable()
    {
        ResetPopUP();
        GetAuctionDatas();
    }

    public void GetData(AuctionData data)
    {
        m_Data = data;
    }

    public void BidButton()
    {
        if(m_Data == null)
        {
            ToastPopUPManager.instance.Initalize("입찰할 품목을 선택해주세요.");
            return;
        }
        BidPopUP.SetActive(true);
        Bid_ItemImage.color = new Color(1, 1, 1, 1);
        Bid_ItemImage.sprite = ActionHolder.GetAtlas(m_Data.itemName);
        Bid_ItemText.text = m_Data.itemName;
        Bid_InputField.text = m_Data.currentPrice;
    }


    public async void Buy()
    {
        if (m_Data == null)
        {
            ToastPopUPManager.instance.Initalize("구매할 품목을 선택해주세요.");
            return;
        }

        bool Set = await BaseManager.Auction.BuyOutItem(
            m_Data.auctionID,
            BaseManager.Firebase.UserID,
            BaseManager.Firebase.NickName);

        if (Set)
        {
            CompletedAuction();
        }
    }

    private async void CompletedAuction()
    {
        bool Completed = await BaseManager.Auction.CompleteAuction(m_Data.auctionID);
        if(Completed)
        {
            GetAuctionDatas();
        }

    }
    public async void SetBid()
    {
        bool Set = await BaseManager.Auction.PlaceBid(
            m_Data.auctionID,
            BaseManager.Firebase.NickName,
            BaseManager.Firebase.UserID,
            int.Parse(Bid_InputField.text));

        if(Set)
        {
            BidPopUP.SetActive(false);
            PhotonManager.NotifyBidPlaced(m_Data.auctionID,
            BaseManager.Firebase.NickName,
            int.Parse(Bid_InputField.text));
            GetAuctionDatas();
        }
    }

    public void OnEndEdit(string temp)
    {
        if(int.Parse(temp) <= int.Parse(m_Data.currentPrice))
        {
            Debug.LogError("경매품목의 물건보다 낮은 금액을 설정할 수 없습니다.");
            Bid_InputField.text = m_Data.currentPrice;
            return;
        }
    }

    public void ResetParts()
    {
        for(int i = 0; i < Parts.Count; i++)
        {
            Parts[i].MarkActive(false);
        }
    }
    private async void GetAuctionDatas()
    {
        ResetGorvage();
        Parts.Clear();
        var datas = await BaseManager.Auction.LoadAuctionItems();

        for(int i = 0; i < datas.Count; i++)
        {
            var go = Instantiate(part, Content);
            go.Initalize(datas[i]);
            Parts.Add(go);
            Gorvage.Add(go.gameObject);
        }
    }

    private void ResetPopUP()
    {
        mainObj = null;
        m_Data = null;
        itemImage.color = new Color(1, 1, 1, 0);
        ItemName.text = "";
        BidPopUP.SetActive(false);
        SetItemPopUP.SetActive(false);
    }
    private void ResetGorvage()
    {
        if (Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }
    }
    public void GetInventoryData()
    {
        ResetGorvage();

        var data = BaseManager.Inventory.inventory;

        foreach (var item in data)
        {
            var go = Instantiate(ItemObject, InventoryContent);

            go.Initalize(item.Value, () => SetItem(item.Value.item));

            Gorvage.Add(go.gameObject);
        }
    }
    public void SetAuctionData()
    {
        if (mainObj == null) return;
        if(string.IsNullOrEmpty(itemPriceInputField.text))
        {
            Debug.LogError("아이템 가격을 입력하세요.");
            return;
        }
        BaseManager.Auction.CreateAuctionItem(
            mainObj.Name, 
            BaseManager.Firebase.NickName,
            BaseManager.Firebase.UserID, 
            int.Parse(itemPriceInputField.text.Trim()));

        SetItemPopUP.SetActive(false);
        GetAuctionDatas();
        BaseManager.Inventory.RemoveItem(mainObj.Name, 1);
        ToastPopUPManager.instance.Initalize("아이템을 등록하였습니다!");
    }

    private void SetItem(ITEM_OBJ obj)
    {
        mainObj = obj;
        itemImage.color = new Color(1, 1, 1, mainObj != null ? 1 : 0);
        itemImage.sprite = ActionHolder.GetAtlas(mainObj.Name);
        ItemName.text = mainObj.Name;
    }
}
