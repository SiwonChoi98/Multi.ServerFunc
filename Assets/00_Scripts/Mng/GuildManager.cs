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
            Debug.LogError("±æµå¸íÀ» ÀÔ·ÂÇÏ¼¼¿ä!");
            return;
        }
        if (guildName.Length < 3 || guildName.Length > 10)
        {
            Debug.LogError("±æµå¸íÀº 3ÀÚ ÀÌ»ó 10ÀÚ ÀÌÇÏ¿©¾ß ÇÕ´Ï´Ù!");
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(guildName, "^[a-zA-Z0-9°¡-ÆR]+$"))
        {
            Debug.LogError("±æµå¸í¿¡ Æ¯¼ö ¹®ÀÚ´Â »ç¿ëÇÒ ¼ö ¾ø½À´Ï´Ù!");
            return;
        }

        CheckGuildNameExists(guildName, exists =>
        {
            if (exists)
            {
                Debug.LogError("ÀÌ¹Ì Á¸ÀçÇÏ´Â ´Ð³×ÀÓÀÔ´Ï´Ù.");
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
            Debug.LogError("±æµå Á¶È¸¿¡ ½ÇÆÐÇÏ¿´½À´Ï´Ù.");
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
        ToastPopUPManager.instance.Initalize("±æµå¿¡ °¡ÀÔÇÏ¿´½À´Ï´Ù.");
    }

    public async Task<bool> KickMemberFromGuild(string targetMemberNickName)
    {
        string guildID = await GetUserGuild();

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        string guildMaster = guildSnapshot.GetValue<string>("guildMaster");
        if(guildMaster != BaseManager.Firebase.NickName)
        {
            ToastPopUPManager.instance.Initalize("±æµå ¸¶½ºÅÍ¸¸ ÇØ´ç ±â´ÉÀ» ÀÌ¿ëÇÒ ¼ö ÀÖ½À´Ï´Ù.");
            return false;
        }
        List<string> members = guildSnapshot.GetValue<List<string>>("members");

        if (!members.Contains(targetMemberNickName))
        {
            Debug.Log("ÇØ´ç À¯Àú´Â ±æµå¿¡ Á¸ÀçÇÏÁö ¾Ê½À´Ï´Ù.");
            return false;
        }

        members.Remove(targetMemberNickName);
        await guildRef.UpdateAsync(new Dictionary<string, object>
        {
            {"members", members }
        });

        UpdateGuildIDByNickName(targetMemberNickName, "");
        return true;
        Debug.Log($"À¯Àú {targetMemberNickName}°¡ ±æµå¿¡¼­ Ãß¹æµÇ¾ú½À´Ï´Ù.");
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
        
        ToastPopUPManager.instance.Initalize("±æµå¸¦ ¼º°øÀûÀ¸·Î »ý¼ºÇÏ¿´½À´Ï´Ù.");
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

            Debug.Log($"À¯Àú '{userNickName}'ÀÇ ±æµå ID°¡ '{newGuildID}'·Î ¾÷µ¥ÀÌÆ®µÇ¾ú½À´Ï´Ù.");
        }

    }

    private void CheckGuildNameExists(string guildName, System.Action<bool> callback)
    {
        BaseManager.Firebase.db.Collection("GUILDS") // Firestore¿¡¼­ "USERS"ÄÃ·º¼ÇÀ» ÂüÁ¶ÇÕ´Ï´Ù.
            .WhereEqualTo("guildName", guildName) // "NICKNAME"ÇÊµåÀÇ °ªÀÌ nickName°ú ¿ÏÀüÈ÷ ÀÏÄ¡ÇÏ´Â ¹®¼­¸¸ °Ë»ö WHERE NICKNAME = "Alice"
            .Limit(1) // °Ë»ö °á°ú¿¡¼­ ÃÖ´ë 1°³ÀÇ ¹®¼­¸¸ °¡Á®¿È
            .GetSnapshotAsync() // ºñµ¿±âÀûÀ¸·Î °á°ú¸¦ °¡Á®¿Â´Ù.
            .ContinueWithOnMainThread(task => // ¸ÞÀÎ ½º·¹µå¿¡¼­ ÈÄ¼Ó ÀÛ¾÷À» ½ÇÇàÇÑ´Ù.
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
                Debug.Log($"ÇÃ·¹ÀÌ¾î°¡ ±æµå¿¡ °¡ÀÔµÇ¾îÀÖ½À´Ï´Ù : {guildID}");
                return guildID;
            }
        }

        Debug.Log("ÇÃ·¹ÀÌ¾î°¡ ±æµå¿¡ °¡ÀÔµÇ¾îÀÖÁö ¾Ê½À´Ï´Ù.");
        return "";
    }

    public async Task LeaveGuild()
    {
        string userNickName = BaseManager.Firebase.NickName;
        string guildID = await GetUserGuild();

        if(string.IsNullOrEmpty(guildID))
        {
            Debug.Log("À¯Àú°¡ °¡ÀÔÇÑ ±æµå°¡ ¾ø½À´Ï´Ù.");
            return;
        }

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        if(!guildSnapshot.Exists)
        {
            Debug.Log("ÇØ´ç ±æµå°¡ Á¸ÀçÇÏÁö ¾Ê½À´Ï´Ù.");
            return;
        }

        List<string> members = guildSnapshot.GetValue<List<string>>("members");

        if(!members.Contains(userNickName))
        {
            Debug.Log("À¯Àú°¡ ÇØ´ç ±æµåÀÇ ¸â¹ö°¡ ¾Æ´Õ´Ï´Ù.");
            return;
        }

        members.Remove(userNickName);

        if(members.Count == 0)
        {
            await guildRef.DeleteAsync();
            Debug.Log("±æµå°¡ »èÁ¦µÇ¾ú½À´Ï´Ù.");
        }
        else
        {
            await guildRef.UpdateAsync(new Dictionary<string, object> 
            {
                {"members", members }
            });
            Debug.Log("À¯Àú°¡ ±æµå¸¦ Å»ÅðÇß½À´Ï´Ù.");
        }

        UpdateGuildID("");
    }

    public async Task DisbandGuild()
    {
        string guildID = await GetUserGuild();

        if(string.IsNullOrEmpty(guildID))
        {
            Debug.Log("À¯Àú°¡ ¼ÓÇÑ ±æµå°¡ ¾ø½À´Ï´Ù.");
            return;
        }

        DocumentReference guildRef = BaseManager.Firebase.db.Collection("GUILDS").Document(guildID);
        DocumentSnapshot guildSnapshot = await guildRef.GetSnapshotAsync();

        if(!guildSnapshot.Exists)
        {
            Debug.Log("±æµå Á¤º¸¸¦ Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            return;
        }

        string guildMaster = guildSnapshot.GetValue<string>("guildMaster");
        if(guildMaster != BaseManager.Firebase.NickName)
        {
            Debug.Log("±æµå ¸¶½ºÅÍ¸¸ ±æµå¸¦ ÇØÃ¼ÇÒ ¼ö ÀÖ½À´Ï´Ù.");
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
            Debug.LogWarning($"±æµå ID {guildID}°ªÀÌ Á¸ÀçÇÏÁö ¾Ê½À´Ï´Ù.");
            return null;
        }
    }
}
