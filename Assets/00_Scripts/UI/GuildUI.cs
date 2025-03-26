using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class GuildUI : BasePopUP
{
    public static GuildUI instance = null;

    [SerializeField] private GameObject NoneGuildObject;
    [SerializeField] private GameObject GuildObjecet;

    public GameObject GuildCreateObject;
    public InputField CreateField;
    public Button CreateButton;

    [Space(20f)]
    [Header("## None Guild PopUP")]
    [SerializeField] private GameObject GuildsObject;
    [SerializeField] private Transform NonePopUpContent;
    [SerializeField] private InputField SearchField;
    List<Dictionary<string, object>> GuildList = new List<Dictionary<string, object>>();
    Dictionary<string, object> NowGuild = new Dictionary<string, object>();
    List<GameObject> MarkObjects = new List<GameObject>();

    [Space(20f)]
    [Header("## Guild PopUP")]
    [SerializeField] private Text GuildNameText;
    [SerializeField] private Text GuildMemberCountText;
    [SerializeField] private Text GuildDeleteText;
    [SerializeField] private Button GuildDeleteBtn;
    [SerializeField] private InputField AnnouncementText;
    [SerializeField] private GameObject GuildPersoneelObject;
    [SerializeField] private Transform GuildPopUpContent;
    List<GameObject> GuildGorvage = new List<GameObject>();
    List<string> MemberList = new List<string>();

    public override void Awake()
    {
        if (instance == null) instance = this;
        CreateButton.onClick.AddListener(() => MakeGuild());
        base.Awake();
    }

    private async void OnEnable()
    {
        string value = await BaseManager.Guild.GetUserGuild();

        if(string.IsNullOrEmpty(value))
        {
            SetAllGuildData();
        }
        else
        {
            SetGuildData();
        }
    }

    public async void SetAllGuildData()
    {
        var guilds = await BaseManager.Guild.GetAllGuilds();
        GuildList = new List<Dictionary<string, object>>(guilds);

        MarkObjects.Clear();
        foreach (Transform child in NonePopUpContent)
        {
            Destroy(child.gameObject);
        }
        int value = 0;
        foreach(var guild in guilds)
        {
            GameObject guildItem = Instantiate(GuildsObject, NonePopUpContent);
            guildItem.SetActive(true);
            Transform Horizontal = guildItem.transform.GetChild(1);

            GameObject Mark = guildItem.transform.Find("Mark").gameObject;
            Mark.GetComponent<Image>().enabled = true;
            MarkObjects.Add(Mark);
            
            Horizontal.Find("GuildName").GetComponent<Text>().text = guild["guildName"].ToString();
            Horizontal.Find("GuildMaster").GetComponent<Text>().text = guild["guildMaster"].ToString();

            List<object> membersObj = guild["members"] as List<object>;
            List<string> members = membersObj.Select(m => m.ToString()).ToList();
            Horizontal.Find("GuildPersonnel").GetComponent<Text>().text =
                string.Format("({0}/25)", members.Count);

            Horizontal.Find("Announcement").GetComponent<Text>().text = guild["announcement"].ToString();
            int index = value;

            Button button = guildItem.transform.Find("Button").GetComponent<Button>();
            button.onClick.AddListener(() => SetGuild(index));
            
            button.enabled = true;
            button.transform.GetComponent<Image>().enabled = true;

            value++;
        }
        NoneGuildObject.SetActive(true);
    }

    public void SearchGuilds()
    {
        string guildName = SearchField.text;
        int value = 0;
        foreach(var guild in GuildList)
        {
            GameObject parentObject = MarkObjects[value].transform.parent.gameObject;
            if(guildName =="")
            {
                parentObject.SetActive(true);
            }
            else if(guildName == guild["guildName"].ToString())
            {
                parentObject.SetActive(true);
            }
            else if(guildName != guild["guildName"].ToString())
            {
                parentObject.SetActive(false);
            }
            
            value++;
        }
    }

    private void SetGuild(int index)
    {
        NowGuild = GuildList[index];
        for (int i = 0; i < MarkObjects.Count; i++) MarkObjects[i].SetActive(false);
        MarkObjects[index].gameObject.SetActive(true);
    }

    public async void JoinGuild()
    {
        if(NowGuild == null)
        {
            Debug.LogError("���õ� ��尡 �����ϴ�.");
            return;
        }
        await BaseManager.Guild.JoinGuild(NowGuild["guildID"].ToString());
        ResetGuildUI();
      
    }

    public void SetAnnouncement()
    {
        BaseManager.Guild.SetAnnouncement(AnnouncementText.text.Trim());

        ToastPopUPManager.instance.Initalize("공지사항이 업데이트 되었습니다.");
    }

    private async void SetGuildData()
    {
        if(GuildGorvage.Count > 0)
        {
            for (int i = 0; i < GuildGorvage.Count; i++) Destroy(GuildGorvage[i]);
            GuildGorvage.Clear();
        }

        var guildData = await BaseManager.Guild.GetUserGuildInfo();
       
        GuildNameText.text = guildData["guildName"].ToString();
        AnnouncementText.text = guildData["announcement"].ToString();
        List<object> membersObj = guildData["members"] as List<object>;
        List<string> members = membersObj.Select(m => m.ToString()).ToList();
        GuildMemberCountText.text = string.Format("���� ���� ({0}/25)", members.Count);

        MemberList = new List<string>(members);
        for (int i = 0; i < members.Count; i++)
        {
            var go = Instantiate(GuildPersoneelObject, GuildPopUpContent);
            go.gameObject.SetActive(true);

            go.transform.Find("PersonName").GetComponent<Text>().text = members[i];

            bool GM = members[i] == guildData["guildMaster"].ToString();
            go.transform.Find("Mark").gameObject.SetActive(GM);
            int index = i;
            go.transform.Find("Button").GetComponent<Button>()
                .onClick.AddListener(() => KickMember(members[index], go));
            GuildGorvage.Add(go);
        }

        GuildDeleteBtn.onClick.RemoveAllListeners();

        bool GuildMaster = BaseManager.Firebase.NickName == guildData["guildMaster"].ToString();
        GuildDeleteText.text = 
            GuildMaster ?  "길드 해체" : "길드 탈퇴";

        if (GuildMaster) GuildDeleteBtn.onClick.AddListener(() => DisabandGuild());
        else GuildDeleteBtn.onClick.AddListener(() => LeaveGuild());

        GuildObjecet.SetActive(true);
    }

    private async void KickMember(string memberID, GameObject MemberUI)
    {
        bool Kick = await BaseManager.Guild.KickMemberFromGuild(memberID);

        if (Kick)
        {
            Destroy(MemberUI);
            ToastPopUPManager.instance.Initalize("������ �߹��Ͽ����ϴ�.");
        }
    }

    public async void LeaveGuild()
    {
        await BaseManager.Guild.LeaveGuild();
        ResetGuildUI();
        ToastPopUPManager.instance.Initalize("길드를 탈퇴하였습니다.");
    }

    public async void DisabandGuild()
    {
        await BaseManager.Guild.DisbandGuild();
        ResetGuildUI();
        ToastPopUPManager.instance.Initalize("길드가 해제되었습니다.");

    }

    public void MakeGuild()
    {
        BaseManager.Guild.CreateGuild(CreateField.text.Trim(), BaseManager.Firebase.NickName);
    }
    public void ResetGuildUI()
    {
        CreateField.text = "";
        NowGuild = null;
        GuildCreateObject.SetActive(false);
        NoneGuildObject.SetActive(false);
        GuildObjecet.SetActive(false);
        GetComponent<Animator>().SetTrigger("Hide");
    }
}
