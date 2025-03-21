using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonHelper
{
    public static bool IsPlayerInRoom(string recipientNickName)
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(player.NickName == recipientNickName)
            {
                return true;
            }
        }
        return false;
    }
    public static string GetPlayerNickName(int actorNumber)
    {
        if(PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players.ContainsKey(actorNumber))
        {
            return PhotonNetwork.CurrentRoom.Players[actorNumber].NickName;
        }
        return "Unknow Player!";
    }

    public static Player GetPlayer(int actorNumber)
    {
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorNumber)
                return player;
        }
        return null;
    }

    public static Player GetPlayerByNickName(string nickName)
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(player.NickName == nickName)
            {
                return player;
            }
        }
        return null;
    }
}
