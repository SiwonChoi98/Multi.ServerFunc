using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
public enum Action_State
{
    None = 0,
    InviteParty,
    Trade,
    InviteGuild
}
public class ActionHolder : MonoBehaviourPunCallbacks
{
    public static SpriteAtlas Atlas;
    public static Dictionary<Action_State, Action> Actions = new Dictionary<Action_State, Action>();
    public static PhotonView photonView;
    public static int TargetPlayerIndex;
    public static Sprite GetAtlas(string temp)
    {
        return Atlas.GetSprite(temp);
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        Atlas = Resources.Load<SpriteAtlas>("Atlas");

        Actions[Action_State.InviteParty] = InviteParty;
        Actions[Action_State.Trade] = Trade;
        Actions[Action_State.InviteGuild] = InviteGuild;
    }
    #region Party
    
    //RPC의 RPCTarget.Others는 자기자신을 제외한 모든 플레이어
    public static void InviteParty()
    {
        string Toast = string.Format("<color=#FFFF00>{0}</color>님에게 파티를 초대하였습니다.",
         PhotonHelper.GetPlayerNickName(TargetPlayerIndex));

        ToastPopUPManager.instance.Initalize(Toast);

        photonView.RPC("ReceivePartyInvite", PhotonHelper.GetPlayer(TargetPlayerIndex), 
            PhotonNetwork.LocalPlayer.ActorNumber, TargetPlayerIndex);
    }

    [PunRPC]
    public void ReceivePartyInvite(int inviterID, int targetPlayerID)
    {
        string temp = string.Format(
            "<color=#FFFF00>{0}</color>님께서 파티를 초대하였습니다.\n수락하시겠습니까?", 
            PhotonHelper.GetPlayerNickName(inviterID));

        Action YES = () =>
        {
            Photon.Realtime.Player HOST = PhotonHelper.GetPlayer(inviterID);
            Photon.Realtime.Player CLIENT = PhotonHelper.GetPlayer(targetPlayerID);

            Party party = BaseManager.Party.GetParty(HOST);
            if (party == null)
            {
                BaseManager.Party.RequestCreateParty(HOST);
                party = BaseManager.Party.GetParty(HOST);
            }

            BaseManager.Party.RequestJoinParty(CLIENT, party.PartyID);
        };

        Action NO = () =>
        {
            ToastPopUPManager.instance.Initalize("파티 초대를 거절하였습니다.");
            photonView.RPC("IgnorePartyInvite", PhotonHelper.GetPlayer(inviterID), targetPlayerID);
        };

        PopUPManager.instance.Initalize(temp, YES, NO);
    }

    [PunRPC]
    public void IgnorePartyInvite(int targetPlayerID)
    {
        string temp = string.Format(
            "<color=#FFFF00>{0}</color>님께서 파티 초대를 거절하였습니다.",
            PhotonHelper.GetPlayerNickName(targetPlayerID));


        ToastPopUPManager.instance.Initalize(temp);
    }
    #endregion
    #region Trade
    public static void Trade()
    {
        string Toast = string.Format("<color=#FFFF00>{0}</color>님에게 거래를 요청하였습니다.",
        PhotonHelper.GetPlayerNickName(TargetPlayerIndex));

        ToastPopUPManager.instance.Initalize(Toast);

        photonView.RPC("ReceiveTradeInvite", PhotonHelper.GetPlayer(TargetPlayerIndex),
            PhotonNetwork.LocalPlayer.ActorNumber, TargetPlayerIndex);
    }

    [PunRPC]
    public void ReceiveTradeInvite(int inviterID, int targetPlayerID)
    {
        string temp = string.Format(
           "<color=#FFFF00>{0}</color>님께서 거래를 요청하였습니다.\n 수락하시겠습니까?",
           PhotonHelper.GetPlayerNickName(inviterID));

        BaseManager.Trade.StartTrade(PhotonHelper.GetPlayer(inviterID));

        Action YES = () =>
        {
            Photon.Realtime.Player HOST = PhotonHelper.GetPlayer(inviterID);
            Photon.Realtime.Player CLIENT = PhotonHelper.GetPlayer(targetPlayerID);

            BaseManager.Trade.RequestTrade();
        };

        Action NO = () =>
        {
            ToastPopUPManager.instance.Initalize("거래를 거절하였습니다.");
            photonView.RPC("IgnoreTradeInvite", PhotonHelper.GetPlayer(inviterID), targetPlayerID);
        };

        PopUPManager.instance.Initalize(temp, YES, NO);
    }

    [PunRPC]
    public void IgnoreTradeInvite(int targetPlayerID)
    {
        string temp = string.Format(
            "<color=#FFFF00>{0}</color>님께서 거래 요청을 거절하였습니다.",
            PhotonHelper.GetPlayerNickName(targetPlayerID));
        ToastPopUPManager.instance.Initalize(temp);
    }
    #endregion
    #region Guild
    public static async void InviteGuild()
    {
        string Toast = string.Format("<color=#FFFF00>{0}</color>�Կ��� ��� ������ ��û�Ͽ����ϴ�.",
       PhotonHelper.GetPlayerNickName(TargetPlayerIndex));

        ToastPopUPManager.instance.Initalize(Toast);
        string guildID = await BaseManager.Guild.GetUserGuild();

        photonView.RPC("ReceiveGuildInvite", PhotonHelper.GetPlayer(TargetPlayerIndex),
            PhotonNetwork.LocalPlayer.ActorNumber, TargetPlayerIndex, guildID);
    }
    [PunRPC]
    public void ReceiveGuildInvite(int inviterID, int targetPlayerID, string guildID)
    {
        string temp = string.Format(
           "<color=#FFFF00>{0}</color>�Բ��� ��� ������ ��û�Ͽ����ϴ�.\n�����Ͻðڽ��ϱ�?",
           PhotonHelper.GetPlayerNickName(inviterID));

        Action YES = () =>
        {
            Photon.Realtime.Player HOST = PhotonHelper.GetPlayer(inviterID);
            Photon.Realtime.Player CLIENT = PhotonHelper.GetPlayer(targetPlayerID);

            BaseManager.Guild.JoinGuild(guildID);
        };

        Action NO = () =>
        {
            ToastPopUPManager.instance.Initalize("��� ���� ��û�� �����Ͽ����ϴ�.");
            photonView.RPC("IgnoreGuildInvite", PhotonHelper.GetPlayer(inviterID), targetPlayerID);
        };

        PopUPManager.instance.Initalize(temp, YES, NO);
    }

    [PunRPC]
    public void IgnoreGuildInvite(int targetPlayerID)
    {
        string temp = string.Format(
            "<color=#FFFF00>{0}</color>�Բ��� �ŷ� ��û�� �����Ͽ����ϴ�.",
            PhotonHelper.GetPlayerNickName(targetPlayerID));
        ToastPopUPManager.instance.Initalize(temp);
    }
    #endregion
}
