using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
public class BaseManager : MonoBehaviourPunCallbacks
{
    public static PartyManager Party;
    public static FirebaseManager Firebase;
    public static InventoryManager Inventory;
    public static TradeManager Trade;
    public static GuildManager Guild;
    public static RankingManager Rank;
    public static AuctionManager Auction;
    public static BaseManager instance = null;

    public List<Post> postList = new List<Post>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        Party = GetComponent<PartyManager>();
        Firebase = GetComponent<FirebaseManager>();
        Inventory = GetComponent<InventoryManager>();
        Trade = GetComponent<TradeManager>();
        Guild= GetComponent<GuildManager>();
        Rank = GetComponent<RankingManager>();
        Auction = GetComponent<AuctionManager>();
    }

    public void SendPostMessage(string userID)
    {
        photonView.RPC("SendNotification", PhotonHelper.GetPlayerByNickName(userID));
    }

    [PunRPC]
    public void SendNotification()
    {
        string message = "편지가 도착하였습니다.";
        ToastPopUPManager.instance.Initalize(message);
        Firebase.LoadInBox(() => PostUI.instance.CheckPost());
    }

}
