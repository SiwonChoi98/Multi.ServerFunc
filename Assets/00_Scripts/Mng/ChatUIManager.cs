using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatUIManager : MonoBehaviour
{
    public static ChatUIManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    public RectTransform content;
    public ScrollRect scrollRect;
    public InputField chatInputField;
    public Text chatLogText;
    public int MaxMessages;
    private List<string> chatMessages = new List<string>();
    private void Update()
    {
        if(EventSystem.current.currentSelectedGameObject == chatInputField.gameObject &&
            Input.GetKeyDown(KeyCode.Return)) // KeyCode.Return = Enter
        {
            SendChatMessage();
        }
    }

    private void SendChatMessage()
    {
        string message = chatInputField.text;
        if(!string.IsNullOrEmpty(message))
        {
            ChatManager.instance.SendMeesageToChat(message);

            chatInputField.text = "";

            chatInputField.ActivateInputField();
        }
    }

    public void DisplayMessage(string Message)
    {
        chatMessages.Add(Message);

        if(chatMessages.Count > MaxMessages)
        {
            chatMessages.RemoveAt(0);
        }

        scrollRect.verticalNormalizedPosition = 0.0f; // 스크롤을 아래로 고정
        UpdateChatLog();
    }

    private void UpdateChatLog()
    {
        chatLogText.text = string.Join("\n", chatMessages);
        content.sizeDelta = new Vector2(content.sizeDelta.x, chatLogText.GetComponent<RectTransform>().sizeDelta.y + 100.0f);
    }
}
