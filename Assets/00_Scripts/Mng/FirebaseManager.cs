using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using System;

public class FirebaseManager : MonoBehaviourPunCallbacks
{
    public GameObject NickNameSetUI;
    public InputField inputField;

    private FirebaseAuth auth;
    private FirebaseUser user;
    public FirebaseFirestore db;

    public string NickName;
    public string UserID;
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if(task.Result == DependencyStatus.Available)
            {
                AppOptions appOptions = new AppOptions
                {
                    ApiKey = "AIzaSyDti4aZhQeVNWEd6JR-UsSZsf7aLe6j7VM",
                    AppId = "1:1068883920654:web:e272823298c4c12e12f95f",
                    ProjectId = "unitymultifunc",
                    DatabaseUrl = new System.Uri("https://unitymultifunc-default-rtdb.firebaseio.com/"),
                };

                FirebaseApp APP = FirebaseApp.Create(appOptions);
                db = FirebaseFirestore.DefaultInstance;
                FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;
                auth = FirebaseAuth.DefaultInstance;
                GuestLogin();
                Debug.Log("Firebase 연결에 성공하였습니다.");
            }
            else
            {
                Debug.LogError("Firebase 초기화에 실패하였습니다.");
            }
        });
    }

    public void GuestLogin()
    {
        string savedUID = PlayerPrefs.GetString("FirebaseUID", "");
        Debug.Log(savedUID);
        if(!string.IsNullOrEmpty(savedUID))
        {
            CheckOrCreateUser(savedUID);
            UserID = savedUID;
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                user = task.Result.User;
                UserID = user.UserId;

                PlayerPrefs.SetString("FirebaseUID", user.UserId);
                PlayerPrefs.Save();

                CheckOrCreateUser(user.UserId);
            }
            else
            {
                Debug.LogError("Guest Login Failed");
            }
        });
    }

    public void CheckOrCreateUser(string userID)
    {
        DocumentReference userRef = db.Collection("USERS").Document(userID);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                if (task.Result.ContainsField("NICKNAME"))
                {
                    NickName = task.Result.GetValue<string>("NICKNAME");
                }
                Debug.Log($"기존 사용자 로그인: {userID}, 닉네임: {NickName}");
                
                SceneManager.LoadScene("Main");
            }
            else
            {
                NickNameSetUI.SetActive(true);
            }
        });
    }

    public void SetNickName()
    {
        string nickName = inputField.text;
        
        if(string.IsNullOrWhiteSpace(nickName))
        {
            Debug.LogError("닉네임을 입력하세요!");
            return;
        }
        if(nickName.Length < 3 || nickName.Length > 10)
        {
            Debug.LogError("닉네임은 3자 이상 10자 이하여야 합니다!");
            return;
        }
        if(!System.Text.RegularExpressions.Regex.IsMatch(nickName, "^[a-zA-Z0-9가-힣]+$"))
        {
            Debug.LogError("닉네임에 특수 문자는 사용할 수 없습니다!");
            return;
        }

        CheckNicknameExists(nickName, exists =>
        {
            if (exists)
            {
                Debug.LogError("이미 존재하는 닉네임입니다.");
            }
            else
            {
                SaveNicknameToFirestore(nickName);
            }
        });
    }
    // FireStore에서 닉네임 중복 확인
    private void CheckNicknameExists(string nickName, System.Action<bool> callback)
    {
        db.Collection("USERS") // Firestore에서 "USERS"컬렉션을 참조합니다.
            .WhereEqualTo("NICKNAME", nickName) // "NICKNAME"필드의 값이 nickName과 완전히 일치하는 문서만 검색 WHERE NICKNAME = "Alice"
            .Limit(1) // 검색 결과에서 최대 1개의 문서만 가져옴
            .GetSnapshotAsync() // 비동기적으로 결과를 가져온다.
            .ContinueWithOnMainThread(task => // 메인 스레드에서 후속 작업을 실행한다.
            {
                if(task.IsCompleted && task.Result.Count > 0)
                {
                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }
            });
    }

    private void SaveNicknameToFirestore(string nickName)
    {
        DocumentReference userRef = db.Collection("USERS").Document(UserID);
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "NICKNAME", nickName },
            { "CREATED_AT", FieldValue.ServerTimestamp },
            { "GUILDID", "" }
        };

        userRef.SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"닉네임 설정 완료: {nickName}");
                NickName = nickName;
                SceneManager.LoadScene("Main");
            }
            else
            {
                Debug.LogError("닉네임 설정 실패!");
            }
        });
    }

    public void m_SendMessage(string recipient, string message, string sender)
    {
        if(string.IsNullOrEmpty(recipient))
        {
            Debug.LogError("메시지를 받는 사람을 입력해주세요.");
            return;
        }
        //if(recipient == NickName)
        //{
        //    Debug.LogError("자기자신에게는 메시지를 보낼 수 없습니다.");
        //    return;
        //}

        db.Collection("USERS")
            .WhereEqualTo("NICKNAME", recipient)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Count > 0)
                {
                    foreach(var doc in task.Result.Documents)
                    {
                        string recipientID = doc.Id;

                        Dictionary<string, object> messageData = new Dictionary<string, object>
                        {
                            { "sender", sender },
                            {"message", message },
                            {"timestamp", FieldValue.ServerTimestamp },
                            {"read", false }
                        };

                        db.Collection("USERS").Document(recipientID)
                        .Collection("MESSAGES")
                        .AddAsync(messageData)
                        .ContinueWithOnMainThread(sendTask =>
                        {
                            if (sendTask.IsCompleted)
                            {
                                Debug.Log($"메시지 전송 완료! 받는이: {recipient}");
                                if (PhotonHelper.IsPlayerInRoom(recipient))
                                {
                                    BaseManager.instance.SendPostMessage(recipient);
                                }
                                ToastPopUPManager.instance.Initalize("편지를 성공적으로 보냈습니다.");
                            }
                            else
                            {
                                Debug.LogError("메시지 전송 실패!");
                            }
                        });
                    }
                }
                else
                {
                    Debug.LogError("받는이를 찾을 수 없습니다!");
                }
            });
    }

    public void LoadInBox(System.Action action)
    {
        db.Collection("USERS").Document(UserID)
            .Collection("MESSAGES")
            .OrderByDescending("timestamp")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    BaseManager.instance.postList.Clear();

                    foreach(var doc in task.Result.Documents)
                    {
                        Post post = new Post();
                        post.messageID = doc.Id;
                        post.sender = doc.GetValue<string>("sender");
                        post.message = doc.GetValue<string>("message");
                        post.isRead = doc.GetValue<bool>("read");

                        BaseManager.instance.postList.Add(post);
                    }

                    action?.Invoke();
                }
                else
                {
                    Debug.LogError("우편함 불러오기 실패!");
                }
            });
    }

    public void UpdateMessageReadStatus(string messageId)
    {
        db.Collection("USERS").Document(UserID)
            .Collection("MESSAGES").Document(messageId)
            .UpdateAsync(new Dictionary<string, object> { { "read", true } })
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("메시지를 읽었습니다.");
                }
                else
                {
                    Debug.LogError("메시지 읽기에 실패하였습니다.");
                }
            });
    }

    public void DeleteMessage(string messageId)
    {
        db.Collection("USERS").Document(UserID)
            .Collection("MESSAGES").Document(messageId)
            .DeleteAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"메시지 {messageId} 삭제 완료!");
                }
                else
                {
                    Debug.LogError($"메시지 {messageId} 삭제 실패: {task.Exception}");
                }
            });
    }

    public void SendPostMessage(string userID)
    {
        photonView.RPC("SendNotification", PhotonHelper.GetPlayerByNickName(userID));
    }
}
