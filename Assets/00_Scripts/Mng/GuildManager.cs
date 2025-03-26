using Firebase.Extensions;
using Firebase.Firestore;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

public class GuildManager : MonoBehaviour
{

    public void CreateGuild(string guildName, string playerId)
    {
        if (string.IsNullOrWhiteSpace(guildName))
        {
            Debug.LogError("길드명을 입력하세요!");
            return;
        }
        if (guildName.Length < 3 || guildName.Length > 10)
        {
            Debug.LogError("길드명은 3자 이상 10자 이하여야 합니다!");
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(guildName, "^[a-zA-Z0-9가-힣]+$"))
        {
            Debug.LogError("길드명에 특수 문자는 사용할 수 없습니다!");
            return;
        }

        CheckGuildNameExists(guildName, exists =>
        {
            if (exists)
            {
                Debug.LogError("이미 존재하는 닉네임입니다.");
            }
            else
            {
                SaveGuildNameToFirestore(guildName, playerId);
            }
        });
    }

    public async Task JoinGuild(string guildID)
    {
        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);

        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();
        if(!guildSnapshot.Exists)
        {
            Debug.LogError("길드 조회에 실패하였습니다.");
            return;
        }

        List<string> members = guildSnapshot.GetValue<List<string>>("members");
        if(!members.Contains(BaseManager.Firebase.NickName))
        {
            members.Add(BaseManager.Firebase.NickName);

            await guildRef.UpdateAsync(new Dictionary<string, object>
            {
                {"members", members }
            });

            UpdateGuildID(guildID);
        };
        ToastPopUPManager.instance.Initalize("길드에 가입하였습니다.");
    }

    public async Task<bool> KickMemberFromGuild(string targetMemberNickName)
    {
        string guildID = await GetUserGuild();

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        string guildMaster = guildSnapshot.GetValue<string>("guildMaster");
        if(guildMaster != BaseManager.Firebase.NickName)
        {
            ToastPopUPManager.instance.Initalize("길드 마스터만 해당 기능을 이용할 수 있습니다.");
            return false;
        }
        List<string> members = guildSnapshot.GetValue<List<string>>("members");

        if (!members.Contains(targetMemberNickName))
        {
            Debug.Log("해당 유저는 길드에 존재하지 않습니다.");
            return false;
        }

        members.Remove(targetMemberNickName);
        await guildRef.UpdateAsync(new Dictionary<string, object>
        {
            {"members", members }
        });

        UpdateGuildIDByNickName(targetMemberNickName, "");
        return true;
        Debug.Log($"유저 {targetMemberNickName}가 길드에서 추방되었습니다.");
    }

    public async void SaveGuildNameToFirestore(string guildName, string playerId)
    {
        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document();
        Dictionary<string, object> guildData = new Dictionary<string, object>
        {
            {"guildName", guildName },
            {"guildMaster", playerId },
            {"members", new List<string> { playerId } },
            {"announcement", "" },
            {"createdAt", FieldValue.ServerTimestamp }
        };

        await guildRef.SetAsync(guildData);
        
        ToastPopUPManager.instance.Initalize("길드를 성공적으로 생성하였습니다.");
        UpdateGuildID(guildRef.Id);
        GuildUI.instance.ResetGuildUI();
    }

    public async void UpdateGuildID(string guildID)
    {
        DocumentReference userRef = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID);

        await userRef.UpdateAsync(new Dictionary<string, object>
        {
            {"GUILDID", guildID }
        });
    }

    public async void UpdateGuildIDByNickName(string userNickName, string newGuildID)
    {
        CollectionReference usersRef = BaseManager.Firebase.db.Collection("USERS");
        QuerySnapshot snapshot = await usersRef.WhereEqualTo("NICKNAME", userNickName).GetSnapshotAsync();

        foreach(DocumentSnapshot userDoc in snapshot.Documents)
        {
            DocumentReference userRef = usersRef.Document(userDoc.Id);

            await userRef.UpdateAsync(new Dictionary<string, object>
            {
                {"GUILDID", newGuildID }
            });

            Debug.Log($"유저 '{userNickName}'의 길드 ID가 '{newGuildID}'로 업데이트되었습니다.");
        }

    }

    private void CheckGuildNameExists(string guildName, System.Action<bool> callback)
    {
        BaseManager.Firebase.db.Collection("GUILDS") // Firestore에서 "GUILDS"컬렉션을 참조합니다.
            .WhereEqualTo("guildName", guildName) // "guildName"필드의 값이 guildName과 완전히 일치하는 문서만 검색 WHERE NICKNAME = "Alice"
            .Limit(1) // 검색 결과에서 최대 1개의 문서만 가져옴
            .GetSnapshotAsync() // 비동기적으로 결과를 가져온다.
            .ContinueWithOnMainThread(task => // 메인 스레드에서 후속 작업을 실행한다.
            {
                if (task.IsCompleted && task.Result.Count > 0)
                {
                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }
            });
    }

    public async void SetAnnouncement(string announcement)
    {
        string guildID = await GetUserGuild();
        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);

        await guildRef.UpdateAsync(new Dictionary<string, object>
        {
            {"announcement", announcement }
        });
    }

    //post에서는 클래스 형태의 저장형태고, guild는 딕셔너리 형태의 테스크 저장방식으로 사용하였음
    public async Task<Dictionary<string, object>> GetUserGuildInfo()
    {
        string guildID = await GetUserGuild();
        Debug.Log(guildID);
        if (string.IsNullOrEmpty(guildID))
        {
            Debug.Log("User is not in any guild");
            return null;
        }

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot snapshot = await guildRef.GetSnapshotAsync();

        if(snapshot.Exists)
        {
            Dictionary<string, object> guildData = snapshot.ToDictionary();
            return guildData;
        }

        Debug.Log("Guild not Found");
        return null;
    }

    public async Task<string> GetUserGuild()
    {
        DocumentReference userRef = BaseManager.Firebase.db.Collection("USERS").Document(BaseManager.Firebase.UserID);
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (snapshot.Exists && snapshot.ContainsField("GUILDID"))
        {
            string guildID = snapshot.GetValue<string>("GUILDID");
        
            if(!string.IsNullOrEmpty(guildID))
            {
                Debug.Log($"플레이어가 길드에 가입되어있습니다 : {guildID}");
                return guildID;
            }
        }

        Debug.Log("플레이어가 길드에 가입되어있지 않습니다.");
        return "";
    }

    public async Task LeaveGuild()
    {
        string userNickName = BaseManager.Firebase.NickName;
        string guildID = await GetUserGuild();

        if(string.IsNullOrEmpty(guildID))
        {
            Debug.Log("유저가 가입한 길드가 없습니다.");
            return;
        }

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        if(!guildSnapshot.Exists)
        {
            Debug.Log("해당 길드가 존재하지 않습니다.");
            return;
        }

        List<string> members = guildSnapshot.GetValue<List<string>>("members");

        if(!members.Contains(userNickName))
        {
            Debug.Log("유저가 해당 길드의 멤버가 아닙니다.");
            return;
        }

        members.Remove(userNickName);

        if(members.Count == 0)
        {
            await guildRef.DeleteAsync();
            Debug.Log("길드가 삭제되었습니다.");
        }
        else
        {
            await guildRef.UpdateAsync(new Dictionary<string, object> 
            {
                {"members", members }
            });
            Debug.Log("유저가 길드를 탈퇴했습니다.");
        }

        UpdateGuildID("");
    }

    public async Task DisbandGuild()
    {
        string guildID = await GetUserGuild();

        if(string.IsNullOrEmpty(guildID))
        {
            Debug.Log("유저가 속한 길드가 없습니다.");
            return;
        }

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        if(!guildSnapshot.Exists)
        {
            Debug.Log("길드 정보를 찾을 수 없습니다.");
            return;
        }

        string guildMaster = guildSnapshot.GetValue<string>("guildMaster");
        if(guildMaster != BaseManager.Firebase.NickName)
        {
            Debug.Log("길드 마스터만 길드를 해체할 수 있습니다.");
            return;
        }

        List<string> members = guildSnapshot.GetValue<List<string>>("members");

        foreach(string memberId in members)
        {
            UpdateGuildIDByNickName(memberId, "");
        }

        await guildRef.DeleteAsync();
    }

    public async Task<List<Dictionary<string, object>>> GetAllGuilds()
    {
        CollectionReference guildsRef = BaseManager.Firebase.db.Collection("GUILDS");
        QuerySnapshot snapshot = await guildsRef.GetSnapshotAsync();

        var guildList = new List<Dictionary<string, object>>();

        foreach(var doc in snapshot.Documents)
        {
            Dictionary<string, object> guildData = doc.ToDictionary();
            guildData["guildID"] = doc.Id;
            guildList.Add(guildData);
        }

        return guildList;
    }

    public async Task<string> GetGuildName(string guildID)
    {
        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot snapshot = await guildRef.GetSnapshotAsync();

        if(snapshot.Exists && snapshot.ContainsField("guildName"))
        {
            return snapshot.GetValue<string>("guildName");
        }
        else
        {
            Debug.LogWarning($"길드 ID {guildID}값이 존재하지 않습니다.");
            return null;
        }
    }
}
