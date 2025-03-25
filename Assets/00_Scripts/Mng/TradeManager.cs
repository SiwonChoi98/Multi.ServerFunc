using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class TradeManager : MonoBehaviourPunCallbacks
{
    public Dictionary<string, int> playerItems = new Dictionary<string, int>(); // 내 거래 아이템
    public Dictionary<string, int> otherPlayerItems = new Dictionary<string, int>(); // 상대방의 거래 아이템

    private bool isComfirmed = false;
    private bool otherConfirmed = false;

    private Player otherPlayer;

    public void StartTrade(Player targetPlayer)
    {
        if (targetPlayer == null) return;
        otherPlayer = targetPlayer;

        photonView.RPC("RPC_ReceivePlayerRequest", targetPlayer, PhotonNetwork.LocalPlayer);
    }
    
    //상대방에게 자기자신을 상대방 플레이어라고 지정
    [PunRPC]
    public void RPC_ReceivePlayerRequest(Player requester)
    {
        otherPlayer = requester;
        Debug.Log(otherPlayer);
    }
    public void RequestTrade()
    {
        TradeUI.instance.gameObject.SetActive(true);
        photonView.RPC("RPC_ReceiveTradeRequest", otherPlayer);
    }

    [PunRPC]
    public void RPC_ReceiveTradeRequest()
    {
        TradeUI.instance.gameObject.SetActive(true);
    }

    public void AddItemToTrade(string itemName, int count)
    {
        if (!BaseManager.Inventory.inventory.ContainsKey(itemName))
        {
            Debug.Log("보유하고 있지 않은 아이템입니다.");
        }

        if(!playerItems.ContainsKey(itemName))
            playerItems[itemName] = 0;

        playerItems[itemName] += count;
        TradeUI.instance.SetHolderData();
        photonView.RPC("RPC_UpdateTradeItems", otherPlayer, playerItems);
    }

    public void ConfirmTrade()
    {
        isComfirmed = true;
        photonView.RPC("RPC_ConfirmTrade", otherPlayer);

        CheckTradeCompletion();
    }

    [PunRPC]
    public void RPC_ConfirmTrade()
    {
        otherConfirmed = true;
        CheckTradeCompletion();
    }

    private void CheckTradeCompletion()
    {
        if(isComfirmed)
        {
            TradeUI.instance.GetComfirm(true);
        }
        if(otherConfirmed)
        {
            TradeUI.instance.GetComfirm(false);
        }

        if (isComfirmed && otherConfirmed)
        {
            Debug.Log("교환 완료!");

            ExchangeItems();

            ResetTrade();
        }
    }

    private void ExchangeItems()
    {
        foreach(var item in playerItems)
        {
            BaseManager.Inventory.RemoveItem(item.Key, item.Value);
        }

        foreach(var item in otherPlayerItems)
        {
            BaseManager.Inventory.AddItem(item.Key, item.Value);
        }

        ToastPopUPManager.instance.Initalize("거래를 성공적으로 완료하였습니다!");
    }

    [PunRPC]
    public void RPC_UpdateTradeItems(Dictionary<string, int> updatedItems)
    {
        otherPlayerItems = updatedItems;
        TradeUI.instance.SetHolderData();
        Debug.Log("상대방의 거래 아이템이 업데이트 되었습니다.");
    }

    public void CancelTrade()
    {
        ResetTrade();
        photonView.RPC("RPC_CancelTrade", otherPlayer);
        ToastPopUPManager.instance.Initalize("거래가 취소되었습니다.");
    }

    [PunRPC]
    public void RPC_CancelTrade()
    {
        ToastPopUPManager.instance.Initalize("거래가 취소되었습니다.");
        ResetTrade();
    }

    private void ResetTrade()
    {
        playerItems.Clear();
        otherPlayerItems.Clear();
        isComfirmed = false;
        otherConfirmed = false;
        TradeUI.instance.GetComponent<Animator>().SetTrigger("Hide");
        Debug.Log("거래 초기화 완료!");
    }
}
