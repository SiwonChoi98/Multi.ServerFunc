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

    private void Update()
    {
        chatClient?.Service();
    }

    public void SendMeesageToChat(string message)
    {
        if(!string.IsNullOrEmpty(message))
        {
            chatClient.PublishMessage(chatChannel, message);
        }
    }

    #region ChatClient_Interface
    // Photon.Chat Ŭ���̾�Ʈ���� �߻��ϴ� ����� �޽����� ó���մϴ�.
    // �Ű����� - level(Error, Warning, Info), message(����� �޽���)
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
    
    // Photon.Chat Ŭ���̾�Ʈ�� ���°� ����� �� ȣ��˴ϴ�.
    // �Ű����� : state(ChatState ������ �� (ENUM), Ŭ���̾�Ʈ�� ���� ���� (Connected, Connecting, Disconnected)
    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Chat State Changed: {state}");
        ///
        /// ConnectedToNameServer : Name Server���� ������ �Ϸ�� ����
        /// Authenticated : ������ �Ϸ�Ǿ� ä�� ������ ������ �غ� �� ����
        /// Disconnected : ������ ���� ����
        /// ConnectedToFrontEnd : Front-End ������ ����� ����
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

    // Photon.Chat ������ ������ �Ǿ��� �� ȣ��˴ϴ�.
    public void OnConnected()
    {
        Debug.Log("Photon Connected!");

        chatClient.Subscribe(new string[] { chatChannel });
    }

    // Photon.Chat ������ ������ �������� �� ȣ��˴ϴ�.
    public void OnDisconnected()
    {
        Debug.Log("Photon Disconnected!");
    }
    // Ư�� ä�ο��� �޽����� �������� �� ȣ��˴ϴ�.
    // channelName : �޽����� ���ŵ� ä�� �̸� , senders : �޽����� ���� ����� �̸� �迭 , messages : ���ŵ� �޽��� �迭
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for(int i = 0; i < senders.Length; i++)
        {
            string receivedMessage = $"{senders[i]}: {messages[i]}";
            Debug.Log($"[{channelName}] {receivedMessage}");

            ChatUIManager.instance.DisplayMessage(receivedMessage);

            Player senderPlayer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == senders[i]);
            if (senderPlayer != null)
            {
                int actorNumber = senderPlayer.ActorNumber;
                string message = messages[i].ToString();

                BubbleUIManager.instance.ShowBubbleForPlayer(actorNumber, message);
            }
        }
    }

    // �ٸ� �÷��̾ ���� ���� �޽����� �������� �� ȣ��˴ϴ�.
    // sender : �޽����� ���� ����� �̸� , meesage : �޽��� ����, channelName : �޽����� ���� ä�� �̸�
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    // Ư�� ������� ���°� ����Ǿ��� �� ȣ��˴ϴ�.
    // user : ���°� ����� ����� , status : ���ο� ���� �ڵ� (�¶���, ��������, �ڸ� ���, �ٻ�),
    // gotMessage : ���� ���� �� �߰� �޽��� ����, message : ���� ����� �Բ� ���޵� �޽���.
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    // ä�� ���� ��û�� ���������� ó���Ǿ��� �� ȣ��˴ϴ�.
    // channels:������ ä�� �̸� �迭, results : �� ä���� ���� ���� ���� (true, false)
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

    // ä�� ���� ���� ��û�� ó���Ǿ��� �� ȣ��˴ϴ�.
    // channels : ���� ������ ä�� �̸� �迭
    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }
    // Ư�� ����ڰ� ä�ο� �������� �� ȣ��˴ϴ�.
    // channel : ����ڰ� ������ ä�� �̸�, user : ������ ����� �̸�
    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    // Ư�� ����ڰ� ä�� ������ �������� �� ȣ��˴ϴ�.
    // channel : ����ڰ� ���� ������ ä�� �̸�, user: ���� ������ ����� �̸�
    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
