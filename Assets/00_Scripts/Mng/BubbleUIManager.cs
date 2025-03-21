using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
public class BubbleUIManager : MonoBehaviourPunCallbacks
{
    public static BubbleUIManager instance = null;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public GameObject bubblePrefab;
    private Dictionary<int, SpeechBubble> playerBubbles = new Dictionary<int, SpeechBubble>();

    public void InitalzieBubble()
    {
        //자기자신을 호출
        CreateBubbleForPlayer(PhotonNetwork.LocalPlayer.ActorNumber);

        //자기자신이 들어왔을 때 다른 플레이어보다 늦게 들어왔을 때 대비 처리
        //본인이 늦게 들어와도 기존에 있던 다른 플레이어들의 말풍선을 미리 만들 수 있음
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if ((player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber))
            {
                CreateBubbleForPlayer(player.ActorNumber);
            }
        }
    }

    //플레이어가 방에 들어왔을 때 호출 (자기 자신은 호출하지 않음)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreateBubbleForPlayer(newPlayer.ActorNumber);
    }
    //플레이어가 방에 떠났을 때 호출
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveBubbleForPlayer(otherPlayer.ActorNumber);
    }
    
    private void CreateBubbleForPlayer(int actorNumber)
    {
        if(!playerBubbles.ContainsKey(actorNumber))
        {
            GameObject bubble = Instantiate(bubblePrefab, transform);
            bubble.SetActive(false);
            SpeechBubble speech = bubble.GetComponent<SpeechBubble>();
            StartCoroutine(BubbleByActorNumberDelay(speech, actorNumber));
            playerBubbles[actorNumber] = speech;
        }
    }

    IEnumerator BubbleByActorNumberDelay(SpeechBubble speech, int actorNumber)
    {
        yield return new WaitForSeconds(0.3f);
        speech.Initalize(actorNumber);
    }

    private void RemoveBubbleForPlayer(int actorNumber)
    {
        if(playerBubbles.ContainsKey(actorNumber))
        {
            Destroy(playerBubbles[actorNumber]);
            playerBubbles.Remove(actorNumber);
        }
    }

    public void ShowBubbleForPlayer(int actorNumber, string message)
    {
        if(playerBubbles.TryGetValue(actorNumber, out SpeechBubble bubble))
        {
            bubble.gameObject.SetActive(true);
            bubble.GetComponent<SpeechBubble>().SetText(message);
        }
    }
}
