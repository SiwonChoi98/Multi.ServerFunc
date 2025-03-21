using Photon.Pun;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform target;

    public float[] CameraPositions;
    public float Speed;


    public void Initalize(Transform T_Object)
    {
        target = T_Object;
    }

    private void Update()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(transform.position,
            new Vector3(
            target.position.x + CameraPositions[0],
            target.position.y + CameraPositions[1],
            target.position.z + CameraPositions[2]), Time.deltaTime * Speed);    
    }
}
