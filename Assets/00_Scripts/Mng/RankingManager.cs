using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using Firebase.Extensions;
public class RANK
{
    public string nickname;
    public string guildname;
    public int level;
    public int rank;
}

public class RankingManager : MonoBehaviour
{
    public async Task<List<RANK>> FetchRanking()
    {
        Query query = BaseManager.Firebase.db.Collection("USERS").OrderByDescending("LEVEL").Limit(10);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        var ranklist = new List<RANK>();

        int rank = 1;

        foreach(DocumentSnapshot doc in snapshot.Documents)
        {
            RANK rankClass = new RANK();
            string nickName = doc.GetValue<string>("NICKNAME");
            string guildID = doc.GetValue<string>("GUILDID");
            string guildName = await BaseManager.Guild.GetGuildName(guildID);
            int level = System.Convert.ToInt32(doc.GetValue<string>("LEVEL"));
            
            rankClass.nickname = nickName;
            rankClass.level = level;
            rankClass.rank = rank;
            rankClass.guildname = guildName;

            ranklist.Add(rankClass);

            rank++;
        }

        return ranklist;
    }

    public void UpdatePlayerLevel(int newLevel)
    {
        DocumentReference userRef = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID);
        userRef.UpdateAsync("LEVEL", newLevel).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"유저의 레벨이 {newLevel}로 변경되었습니다.");
            }
            else
            {
                Debug.LogError("유저 레벨 업데이트에 실패하였습니다.");
            }
        });
    }
}
