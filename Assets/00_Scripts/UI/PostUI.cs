using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

public class PostUI : BasePopUP
{
    public static PostUI instance = null;

    public override void Awake()
    {
        if (instance == null) instance = this;
        BaseManager.Firebase.LoadInBox(() => GetPostData());
        base.Awake();
    }

    [SerializeField] private InputField RecipientField; //받는 이
    [SerializeField] private InputField MessageField;

    [SerializeField] private GameObject Box;
    [SerializeField] private Transform Content;
    List<GameObject> Gorvage = new List<GameObject>();

    [SerializeField] private Text SenderText;
    [SerializeField] private Text MessageText;

    [SerializeField] private GameObject Mark;
    int NowValue;
    public void CloseBtn()
    {
        Invoke("ClosePostObject", 0.2f);
    }
 
    private void ClosePostObject() => this.gameObject.SetActive(false);

    public void OnEnable()
    {
        CheckPost();
        BaseManager.Firebase.LoadInBox(() => GetPostData());
    }

    public void CheckPost()
    {
        bool GetMark = false;
        if (BaseManager.instance.postList.Count > 0)
        {
            for (int i = 0; i < BaseManager.instance.postList.Count; i++)
            {
                if (BaseManager.instance.postList[i].isRead == false)
                {
                    GetMark = true;
                    break;
                }
            }
        }

        Mark.gameObject.SetActive(GetMark);
    }

    public void GetReadText(int value, GameObject obj)
    {
        Post post = BaseManager.instance.postList[value];
        post.isRead = true;
        NowValue = value;
        obj.transform.Find("Mark").gameObject.SetActive(false);
        BaseManager.Firebase.UpdateMessageReadStatus(post.messageID);

        SenderText.text = post.sender;
        MessageText.text = post.message;
        CheckPost();
    }
    public void DeleteMessage()
    {
        BaseManager.Firebase.DeleteMessage(BaseManager.instance.postList[NowValue].messageID);
        BaseManager.Firebase.LoadInBox(() => GetPostData());
    }
    public void GetPostData()
    {
        if (Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }

        for (int i = 0; i < BaseManager.instance.postList.Count; i++)
        {
            var go = Instantiate(Box, Content);
            go.SetActive(true);

            go.transform.Find("NickName").GetComponent<Text>().text = BaseManager.instance.postList[i].sender;
            Button button = go.transform.GetComponentInChildren<Button>();

            go.transform.Find("Mark").gameObject.SetActive(!BaseManager.instance.postList[i].isRead);

            int index = i;

            button.onClick.AddListener(() => GetReadText(index, go));

            Gorvage.Add(go);
        }
        CheckPost();
    }

    public void SendMessage()
    {
        BaseManager.Firebase.m_SendMessage(
            RecipientField.text.Trim(),  //Trim은 인풋필드에 앞뒤 공백을 제거하는 메서드
            MessageField.text.Trim(), 
            BaseManager.Firebase.NickName);
        RecipientField.text = "";
        MessageField.text = "";
    }
}
