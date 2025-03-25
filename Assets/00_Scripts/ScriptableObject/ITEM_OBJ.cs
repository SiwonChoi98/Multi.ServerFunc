using UnityEngine;

[CreateAssetMenu(fileName = "ITEM_OBJ", menuName = "Scriptable Objects/ITEM_OBJ")]
public class ITEM_OBJ : ScriptableObject
{
    public string Name; //m_Name 이렇게 하면 스크립터블 오브젝트 만들면 자동으로 정해진 이름으로 들어간다.
}
