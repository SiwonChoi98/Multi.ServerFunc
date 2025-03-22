using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 파티는 DB를 함께 이용해야 좋지만 RPC로도 보완만 하면 가능하긴 하다.
/// </summary>
public class PartyManager : MonoBehaviourPunCallbacks
{
    private Dictionary<int, Party> activeParties = new Dictionary<int, Party>();

    public bool HasParty(Player player)
    {
        foreach (var party in activeParties.Values)
        {
            if (party.IsMember(player))
                return true;
        }
        return false;
    }

    public Party GetParty(Player player)
    {
        foreach (var party in activeParties.Values)
        {
            if (party.IsMember(player))
                return party;
        }
        return null;
    }
    [PunRPC]
    public void RPC_CreateParty(int partyID, int leaderID)
    {
        if (activeParties.ContainsKey(partyID)) return;

        Player leader = PhotonHelper.GetPlayer(leaderID);
        if (leader == null) return;

        Party newParty = new Party(partyID, leader);
        activeParties.Add(partyID, newParty);
        PartyUI.instance.Initalize();
    }
    public void RequestCreateParty(Player leader)
    {
        if (HasParty(leader)) return;

        int partyID = activeParties.Count + 1;
        photonView.RPC("RPC_CreateParty", RpcTarget.AllBuffered, partyID, leader.ActorNumber);
    }

    [PunRPC]
    public void RPC_JoinParty(int playerID, int partyID)
    {
        if (!activeParties.ContainsKey(partyID)) return;

        Player player = PhotonHelper.GetPlayer(playerID);
        if (player == null || HasParty(player)) return;

        activeParties[partyID].AddMember(player);
        PartyUI.instance.Initalize();
    }

    public void RequestJoinParty(Player player, int partyID)
    {
        photonView.RPC("RPC_JoinParty", RpcTarget.AllBuffered, player.ActorNumber, partyID);
    }

    [PunRPC]
    public void RPC_LeaveParty(Player player)
    {
        if (player == null) return;

        Party party = GetParty(player);
        if(party != null)
        {
            party.RemoveMember(player);
            PartyUI.instance.Initalize();

            if(party.Members.Count == 0)
            {
                activeParties.Remove(party.PartyID);
            }
            else
            {
                for(int i = 0; i < party.Members.Count; i++)
                {
                    photonView.RPC("RPC_NotifyPartyMemberLeft", party.Members[i], player);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_NotifyPartyMemberLeft(Player player)
    {
        string temp = string.Format("{0}님께서 파티를 탈퇴하였습니다.", player.NickName);
        ToastPopUPManager.instance.Initalize(temp);
    }

    public void RequestLeaveParty(Player player)
    {
        photonView.RPC("RPC_LeaveParty", RpcTarget.AllBuffered, player);
    }

    //플레이어가 떠났을 때 호출
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(HasParty(otherPlayer))
        {
            photonView.RPC("RPC_LeaveParty", RpcTarget.AllBuffered, otherPlayer);
        }
    }

}
