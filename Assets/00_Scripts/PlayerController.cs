using Photon.Pun;
using UnityEngine;

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
            view.RPC("SetActorNumber", RpcTarget.AllBuffered, actorNumber);
        }
    }

    public bool isMinePhoton()
    {
        return view.IsMine;
    }

    // RPC : Remote Procedure Call -  ��Ʈ��ũ ���� �ٸ� �÷��̾ ���� ���� Ư�� �޼��带 ȣ��
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
