using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    
    //-9.81f : UNITY에서 기본이라고 말했음
    private float gravity = -9.81f;
    private Vector3 velocity;

    public int OwnerActorNumber { get; private set; }
    public float speed;

    PhotonView view;

    public void Initalize(int actorNumber)
    {
        if(isMinePhoton())
        {
            OwnerActorNumber = actorNumber;
            //AllBuffered 은 ALL 과 유사하지만 AllBuffered는 아직 방에 참여하지 않은 유저도 보관을 한다.
            //여기서 먼저 호출을 해주면 이후에 들어오는 플레이어도 RPC가 다시 실행됨 
            //ALL은 현재 기점으로 접속중인 플레이어만 호출됨
            view.RPC("SetActorNumber", RpcTarget.AllBuffered, actorNumber);
        }
    }

    public bool isMinePhoton()
    {
        return view.IsMine;
    }

    // RPC : Remote Procedure Call - 네트워크 상의 다른 플레이어가 실행 중인 특정 메서드를 호출
    [PunRPC]
    public void SetActorNumber(int actorNumber)
    {
        OwnerActorNumber = actorNumber;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        view = GetComponent<PhotonView>();
    }
    private void Update()
    {
        //자신이 아니면 리턴
        if (!view.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0, v); //nomarlized를 하면 움직임이 뚝뚝 끊기는 느낌이남

        if(movement.magnitude >= 0.1f)
        {
            //LOOK AT 함수랑 같다 Atan2를 이용한 angle을 만드는게 조금 더 정교하다.
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            characterController.Move(movement * (speed * Time.deltaTime));

            animator.SetFloat("Movement", movement.magnitude);
        }
        else
        {
            animator.SetFloat("Movement", 0);
        }
    }
}
