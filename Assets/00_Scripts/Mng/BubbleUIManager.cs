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
        CreateBubbleForPlayer(PhotonNetwork.LocalPlayer.ActorNumber);

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if ((player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber))
            {
                CreateBubbleForPlayer(player.ActorNumber);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreateBubbleForPlayer(newPlayer.ActorNumber);
    }

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
