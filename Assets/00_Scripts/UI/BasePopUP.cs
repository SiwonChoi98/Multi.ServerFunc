using UnityEngine;

public class BasePopUP : MonoBehaviour
{
    public virtual void Awake()
    {
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
}
