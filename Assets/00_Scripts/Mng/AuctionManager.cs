using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AuctionData
{
    public string auctionID;
    public string itemName;
    public string sellerID;
    public string sellerNickName;
    public string startingPrice;
    public string currentPrice;
    public string buyOutPrice;
    public string highestBidder;
    public string highestBidderID;
    public string EndTime;
}
public class AuctionManager : MonoBehaviour
{
    private async void GrantItemToBuyer(string buyerID, string itemName)
    {
        DocumentReference itemRef = BaseManager.Firebase.db.Collection("USERS").Document(buyerID)
              .Collection("INVENTORY").Document(itemName);
        DocumentSnapshot snapshot = await itemRef.GetSnapshotAsync();

        int countValue = 1;
        if (snapshot.Exists)
        {
            countValue = snapshot.GetValue<int>("count");
            countValue++;
        }
        Dictionary<string, object> itemData = new Dictionary<string, object>
         {
             {"name", itemName },
             {"count", countValue},
             {"acquiredAt", FieldValue.ServerTimestamp}
         };
        // MergeAll - 기존 데이터는 유지하고, 새로운 필드만 업데이트
        // MergeFields - (string) <- 매개변수에 대한 값만 변경
        await itemRef.SetAsync(itemData, SetOptions.MergeAll);
    }
    public async Task<bool> CompleteAuction(string auctionID)
    {
        DocumentReference auctionRef = BaseManager.Firebase.db.Collection("AUCTIONS").Document(auctionID);

        return await BaseManager.Firebase.db.RunTransactionAsync(transaction =>
        {
            DocumentSnapshot snapshot = transaction.GetSnapshotAsync(auctionRef).Result;
            if (!snapshot.Exists)
            {
                Debug.LogError("옥션 처리 실패");
                return Task.FromResult(false);
            }
            string sellerID = snapshot.GetValue<string>("seller_id");
            string sellerNick = snapshot.GetValue<string>("seller_nickname");
            string highestBidder = snapshot.GetValue<string>("highest_bidder");
            string highestBidderID = snapshot.GetValue<string>("highest_bidder_ID");
            int finalPrice = snapshot.GetValue<int>("current_price");
            string itemName = snapshot.GetValue<string>("item_name");

            transaction.Delete(auctionRef);

            if (!string.IsNullOrEmpty(highestBidder))
            {
                BaseManager.Firebase.m_SendMessage(
                highestBidder,
                 $"{itemName} 아이템이 {finalPrice}에 판매되었습니다.!",
                    "Admin");

                GrantItemToBuyer(highestBidderID, itemName);

                BaseManager.Firebase.m_SendMessage(
               sellerNick,
               $"{itemName} 아이템을 {finalPrice}에 구매하였습니다.!",
               "Admin");
            }
            else
            {
                BaseManager.Firebase.m_SendMessage(
                          sellerNick,
                          $"{itemName} 아이템이 판매되지 못하였습니다.",
                          "Admin");

                GrantItemToBuyer(sellerID, itemName);
            }


            return Task.FromResult(true);
        });
    }
    public async Task<bool> BuyOutItem(string auctionID, string buyerID, string buyerNick)
    {
        DocumentReference auctionRef = BaseManager.Firebase.db.Collection("AUCTIONS").Document(auctionID);

        return await BaseManager.Firebase.db.RunTransactionAsync(transaction =>
        {
            DocumentSnapshot snapshot = transaction.GetSnapshotAsync(auctionRef).Result;
            if (!snapshot.Exists)
            {
                Debug.LogError("옥션에 Exists가 실행되지 않으셨습니다.");
                return Task.FromResult(false);
            }
            int buyoutPrice = snapshot.GetValue<int>("buyout_price");
            transaction.Update(auctionRef, new Dictionary<string, object>
            {
                {"current_price",  buyoutPrice},
                {"highest_bidder", buyerNick },
                {"highest_bidder_ID" ,buyerID}
            });

            Debug.Log($"옥션 {auctionID}품목이 판매가 완료되었습니다.");
            return Task.FromResult(true);
        });
    }

    public async Task<bool> PlaceBid(string auctionID, string bidderNick, string bidderID,int currentPrice)
    {
        DocumentReference auctionRef = BaseManager.Firebase.db.Collection("AUCTIONS").Document(auctionID);
        // GetSnapshotAsync -> 충돌 가능성이 높다.
        // RunTransactionAsync -> 동시 수정을 방지, 데이터 정합성 유지
        // 경매장은 많은 유저들이 사용하니까 동시 수정 충돌이 발생할 수 있어서 RunTransactionAsync 사용
        return await BaseManager.Firebase.db.RunTransactionAsync(transaction =>
        {
            DocumentSnapshot snapshot = transaction.GetSnapshotAsync(auctionRef).Result;
            if (!snapshot.Exists)
            {
                Debug.LogError("옥션 Exists에 실패하였습니다.");
                return Task.FromResult(false);
            }

            int currentprice = snapshot.GetValue<int>("current_price"); 

            if(currentPrice <= currentprice)
            {
                Debug.Log("입찰가가 최신 입찰가보다 낮습니다.");
                return Task.FromResult(false);
            }

            transaction.Update(auctionRef, new Dictionary<string, object>
            {
                {"current_price", currentPrice },
                {"highest_bidder", bidderNick },
                {"highest_bidder_ID",  bidderID}
            });
            Debug.Log($"성공적으로 {auctionID}품목의 입찰을 완료하였습니다.");
            return Task.FromResult(true);
        });
    }
    public async Task<List<AuctionData>> LoadAuctionItems()
    {
        List<AuctionData> auctionlist = new List<AuctionData>();
        QuerySnapshot snapshot = await BaseManager.Firebase.db.Collection("AUCTIONS").GetSnapshotAsync();

        List<string> expiredAuctions = new List<string>();
        foreach(DocumentSnapshot doc in snapshot.Documents)
        {
            DateTime endTime = doc.GetValue<Timestamp>("end_time").ToDateTime().ToLocalTime();
            DateTime now = DateTime.Now;

            if(endTime <= now)
            {
                expiredAuctions.Add(doc.Id);
                continue;
            }

            AuctionData auction = new AuctionData
            {
                auctionID = doc.Id,
                itemName = doc.GetValue<string>("item_name"),
                sellerID = doc.GetValue<string>("seller_id"),
                sellerNickName = doc.GetValue<string>("seller_nickname"),
                startingPrice = doc.GetValue<int>("starting_price").ToString(),
                currentPrice = doc.GetValue<int>("current_price").ToString(),
                buyOutPrice = doc.GetValue<int>("buyout_price").ToString(),
                highestBidder = doc.GetValue<string>("highest_bidder"),
                highestBidderID = doc.GetValue<string>("highest_bidder_ID"),
                EndTime = doc.GetValue<Timestamp>("end_time").ToDateTime().ToLocalTime().
                ToString("yyyy-MM-dd HH:mm:ss")
            };
            auctionlist.Add(auction);
        }

        if (expiredAuctions.Count > 0)
        {
            await ProcessExpiredAuctions(expiredAuctions);
        }

        return auctionlist;
    }

    private async Task ProcessExpiredAuctions(List<string> expiredAuctions)
    {
        foreach(string auctionId in expiredAuctions)
        {
            bool completed = await CompleteAuction(auctionId);
        }
    }
    public async void CreateAuctionItem(string itemName, string sellerNickName,string sellerID, int buyoutPrice)
    {
        // UTC - 협정 세계시
        // UTC+9 - 우리나라 시간
        string auctionId = Guid.NewGuid().ToString(); //랜덤한 고유 아이디 생성
        DateTime endTime = DateTime.UtcNow.AddSeconds(10); //1440 - 1day 
        int startingPrice = buyoutPrice / 2;
        Dictionary<string, object> auctionData = new Dictionary<string, object>
        {
            {"auction_id",  auctionId},
            {"item_name", itemName },
            {"seller_id", sellerID },
            {"seller_nickname",  sellerNickName},
            {"starting_price", startingPrice },
            {"current_price", startingPrice },
            {"buyout_price",  buyoutPrice},
            {"highest_bidder", "" },
            {"highest_bidder_ID","" },
            {"end_time", Timestamp.FromDateTime(endTime) },
            {"created_at", Timestamp.FromDateTime(DateTime.UtcNow) } //UTCNOW - 국제 표준 //NOW - 나라 기준
        };

        DocumentReference auctionRef = BaseManager.Firebase.db.Collection("AUCTIONS").Document(auctionId);
        await auctionRef.SetAsync(auctionData);

        Debug.Log($"경매장에 아이템이 등록되었습니다.: {auctionId}");
    }
}
