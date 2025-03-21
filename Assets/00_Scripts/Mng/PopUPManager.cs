using UnityEngine;
using UnityEngine.UI;
using System;
public class PopUPManager : BasePopUP
{
    public static PopUPManager instance = null;

    public override void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        transform.SetAsLastSibling();
        base.Awake();
    }
    public Text Description;
    public Button YesBtn;
    public Button NoBtn;

    public void Initalize(string temp, Action Yes, Action NO)
    {
        gameObject.SetActive(true);
        Description.text = temp;

        RemoveAllButtons();

        YesBtn.onClick.AddListener(() => Yes());
        NoBtn.onClick.AddListener(() => NO());

        YesBtn.onClick.AddListener(() => this.gameObject.SetActive(false));
        NoBtn.onClick.AddListener(() => this.gameObject.SetActive(false));
    }

    private void RemoveAllButtons()
    {
        YesBtn.onClick.RemoveAllListeners();
        NoBtn.onClick.RemoveAllListeners();
    }
}
