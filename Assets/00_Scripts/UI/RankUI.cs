using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class RankUI : BasePopUP
{
    public static RankUI instance = null;

    public GameObject RankObject;
    public Transform Content;
    List<GameObject> Gorvage = new List<GameObject>();
    
    public override void Awake()
    {
        if (instance == null) instance = this;
        base.Awake();
    }

    private void OnEnable()
    {
        GetRankUI();
    }

    private async void GetRankUI()
    {
        if(Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }
        var ranklist = await BaseManager.Rank.FetchRanking();

        for(int i = 0; i < ranklist.Count; i++)
        {
            var go = Instantiate(RankObject, Content);
            go.SetActive(true);
            Transform horizontal = go.transform.GetChild(0);
            horizontal.transform.Find("NickName").GetComponent<Text>().text = ranklist[i].nickname;
            horizontal.transform.Find("Guild").GetComponent<Text>().text = ranklist[i].guildname;
            horizontal.transform.Find("Level").GetComponent<Text>().text = ranklist[i].level.ToString();
            horizontal.transform.Find("Rank").GetComponent<Text>().text = ranklist[i].rank.ToString();

            Gorvage.Add(go);
        }
    }
}
