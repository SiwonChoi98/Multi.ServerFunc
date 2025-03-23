using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public static ChatManager instance;
    void Awake()
    {
        if (instance == null) instance = this;
    }

    private ChatClient chatClient;
    private string chatChannel = "GlobalChannel";

    public void Initalize()
    {
        if(string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = BaseManager.Firebase.NickName; 
        }

        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            PhotonNetwork.AppVersion, new Photon.Chat.AuthenticationValues(PhotonNetwork.NickName));
    }

    //프레임 마다 어떤 메세지가 보내졌는지 확인
    private void Update()
    {
        chatClient?.Service();
    }

    //채팅 보내기
    public void SendMeesageToChat(string message)
    {
        if(!string.IsNullOrEmpty(message))
        {
            chatClient.PublishMessage(chatChannel, message);
        }
    }

    #region ChatClient_Interface
    // Photon.Chat 클라이언트에서 발생하는 디버깅 메세지를 처리한다.
    // 매개변수 - level(Error(심각), Warning(경고), Info(정보)), message(디버깅 메세지)
    //전달된 메세지가 있다면 Debug가 노출됨
    public void DebugReturn(DebugLevel level, string message)
    {
        switch(level)
        {
            case DebugLevel.ERROR:
                Debug.LogError($"Photon Chat Error: {message}");
                break;
            case DebugLevel.WARNING:
                Debug.LogWarning($"Photon Chat Warning: {message}");
                break;
            default:
                Debug.Log($"Photon Chat : {message}");
                break;
        }
    }
    
    // Photon.Chat 클라이언트의 상태가 변경될 때 호출된다.
    // 매개변수 : state(ChatState 열거형 값 (ENUM), 클라이언트의 현재 상태 (Connected, Connecting, Disconnected)
    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Chat State Changed: {state}");
        ///
        /// ConnectedToNameServer : Name Server와의 연결이 완료된 상태
        /// Authenticated : 인증이 완료되어 채팅 서버와 연결할 준비가 된 생태
        /// Disconnected : 연결이 끊긴 상태
        /// ConnectedToFrontEnd : Front-End 서버와 연결된 상태
        ///
        switch(state)
        {
            case ChatState.ConnectedToNameServer:
                Debug.Log("Connected to Name Server");
                break;
            case ChatState.Authenticated:
                Debug.Log("Authenticated successfully.");
                break;
            case ChatState.Disconnected:
                Debug.LogWarning("Disconnected from Chat Server");
                break;
            case ChatState.ConnectingToFrontEnd:
                Debug.Log("Connected to Front End Server");
                break;
            default:
                Debug.Log($"Unhandled Chat State: {state}");
                break;
        }
    }

    // Photon.Chat 서버와 연결이 되었을 때 호출
    public void OnConnected()
    {
        Debug.Log("Photon Connected!"); 

        chatClient.Subscribe(new string[] { chatChannel });
    }

    // Photon.Chat 서버와 연결이 끊어졌을 때 호출
    public void OnDisconnected()
    {
        Debug.Log("Photon Disconnected!");

    }

    //RPC로도 처리가능하지만 통일성을 위해 OnGetMessages으로 처리
    // 특정 채널에서 메세지를 수신했을 때 호출된다. (서버가 여러개 일 때) 예) 파티채널, 길드채널 등등
    // channelName : 메세지가 수신된 채널 이름 , senders : 메세지를 보낸 사용자 이름 배열 , messages : 수신된 메세지 배열
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for(int i = 0; i < senders.Length; i++)
        {
            string receivedMessage = $"{senders[i]}: {messages[i]}";
            Debug.Log($"[{channelName}] {receivedMessage}");

            ChatUIManager.instance.DisplayMessage(receivedMessage);

            //현재 이게임에 포함되어 있는 모든 플레이어를 가져와서 조건에 맞는 첫번쨰 플레이어를 반환 (
            //플레이어 목록에서 닉네임이 현재 메세지 발신자인 senders[i]와 일치하는가?)
            Player senderPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == senders[i]);
            if (senderPlayer != null)
            {
                int actorNumber = senderPlayer.ActorNumber;
                string message = messages[i].ToString();

                BubbleUIManager.instance.ShowBubbleForPlayer(actorNumber, message);
            }
        }
    }

    // 다른 플레이어가 보낸 개인 메세지를 수신했을 때 호출 (귓속말 등)
    // sender : 메세지를 보낸 사용자 이름 , meesage : 메세지 내용, channelName : 메세지가 속한 채널 이름
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    // 특정 사용자의 상태가 변경되었을 때 호출된다.
    // user : 상태가 변경된 사용자 , status : 새로운 상태 코드 (온라인, 오프라인, 자리비움, 바쁨 등)
    // gotMessage : 상태 변경 시 추가 메세지 여부, message : 상태 변경과 함께 전달된 메세지.
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    // 채널 구독 요청이 성공적으로 처리되었을 때 호출
    // channels: 구독한 채널 이름 배열, results : 각 채널의 구독 성공 여부 (true, false)
    // 예를 들어 길드를 들어가면 길드채널의 채팅을 사용할 수 있게 구독 하는 방식 
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for(int i = 0; i < channels.Length; i++)
        {
            if (results[i])
            {
                Debug.Log($"Subscribed to channel: {channels[i]}");
            }
            else
            {
                Debug.LogError($"Failed to subscribe to channel: {channels[i]}");
            }
        }
    }

    // 채널 구독 해제 요청이 처리되었을 때 호출된다.
    // channels : 구독 해제된 채널 이름 배열
    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }
    // 특정 사용자가 채널에 구독 했을 때 호출
    // channel : 사용자가 구독한 채널 user : 구독한 사용자 이름
    // OnSubscribed 다른 점 : OnSubscribed는 채널과 성공여부만 가져오는데 이거는 채널과 유저 이름을 가져옴
    // 예) 유저가 길드에 가입했을 때 ??유저가 가입하였습니다. 축하해주세요. 이런거에 사용할 수 있다.
    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    // 특정 사용자가 채널 구독을 해제했을 때 호출된다.
    // channel : 사용자가 구독 해제한 채널 이름, user: 구독 해제한 사용자 이름
    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
