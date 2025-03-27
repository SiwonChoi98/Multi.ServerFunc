using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public const byte BID_EVENT = 1;
    public const byte AUCTION_COMPLETE_EVENT = 3;
    private void Start()
    {
        // Photon 서버 연결
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("포톤 마스터 서버에 연결하였습니다.");
        // 랜덤 룸에 참가하거나 새로운 룸을 생성
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("방 참가에 실패하였습니다. 방을 새로 만듭니다.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("룸에 접속하였습니다.");
        SpawnPlayer();
        ChatManager.instance.Initalize();
        BubbleUIManager.instance.InitalzieBubble();
    }
    
    void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-5.0f, 5.0f), 0.0f, Random.Range(-5.0f, 5.0f));
        //PhotonNetwork.Instantiate는 Resource내부의 프리팹 이름을 가져오게 되어있음
        GameObject playerObject = PhotonNetwork.Instantiate("PlayerPrefab", spawnPosition, Quaternion.identity);

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        playerObject.GetComponent<PlayerController>().Initalize(actorNumber);
        Camera.main.GetComponent<CameraController>().Initalize(playerObject.transform);

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }
    // RPC -> Remote Procedure call 
    // RaiseEvent
    // 서버 -> RPC = 직접 해당 객체의 PhotonView를 통해 호출 //특정 오브젝트의 일부분 동기화가 필요할때 RPC 사용
    // 서버 -> RaiseEvent = 이벤트 기반 ( Photon 서버를 거쳐 전달됨 ) //게임 상태 공유할때는 보통 RaiseEvent 사용
    public static void NotifyAuctionCompleted(string auctionID, string winnberNick)
    {
        object[] content = new object[] { auctionID, winnberNick };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(AUCTION_COMPLETE_EVENT, content, options, SendOptions.SendReliable);
    }

    public static void NotifyBidPlaced(string auctionId, string bidderNick, int currentPrice)
    {
        object[] content = new object[] { auctionId, bidderNick, currentPrice };
        // Photon에서는 RaiseEvent()가 object[] 타입의 데이터를 전달할 수 있음.
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(BID_EVENT, content, options, SendOptions.SendReliable);
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code == BID_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            string auctionID = (string)data[0];
            string bidderNick = (string)data[1];
            string currentPrice = ((int)data[2]).ToString();

            UpdateAuctionUI(auctionID, bidderNick, currentPrice);
        }

        if(photonEvent.Code == AUCTION_COMPLETE_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            string auctionID = (string)data[0];
            string winnerNick = (string)data[1];
            RemoveAuctionFromUI(auctionID);
        }
    }
    private void RemoveAuctionFromUI(string auctionID)
    {
        ToastPopUPManager.instance.Initalize(
            string.Format("'{0}'아이템이 판매되었습니다.", auctionID));
    }

    private void UpdateAuctionUI(string auctionID, string bidderNick, string currentPrice)
    {
        ToastPopUPManager.instance.Initalize(
            string.Format("'{1}'님이 물건을 입찰을 하였습니다. 입찰가:{2}",
            auctionID, bidderNick, currentPrice));
    }
}
