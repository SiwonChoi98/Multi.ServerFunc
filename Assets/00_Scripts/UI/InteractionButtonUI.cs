using System;
using UnityEngine;
using UnityEngine.UI;
public class InteractionButtonUI : MonoBehaviour
{
    public InteractionUI baseInteraction;
    public Image LineImage;
    public Image IconImage;
    public Text ButtonName;
    Button button;

    Action_State m_Action;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    public void Initalize(Action_State state)
    {
        m_Action = state;
        if(m_Action == Action_State.None)
        {
            GetComponent<Image>().color = new Color(0, 0, 0, GetComponent<Image>().color.a);
            return;
        }
        IconImage.gameObject.SetActive(true);
        IconImage.sprite = ActionHolder.GetAtlas(state.ToString());

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => baseInteraction.DeactiveObject());
        button.onClick.AddListener(() => ActionHolder.Actions[state]());
    }
}
