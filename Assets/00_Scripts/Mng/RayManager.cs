using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RayManager : MonoBehaviour
{
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GraphicRaycaster graphicRaycaster; // Canvas�� �߰��� GraphicRaycaster
    [SerializeField] private EventSystem eventSystem;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            interactionUI.DeactiveObject();
        }
        if (Input.GetMouseButtonDown(1) && !IsPointerOverUI())
        {
            FindPlayerClick();
        }
    }
    private bool IsPointerOverUI()
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        return results.Count > 0;
    }
    private void FindPlayerClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            PlayerController player = hit.collider.GetComponent<PlayerController>();
            //if (player.isMinePhoton()) return;

            if (player != null)
            {
                ActionHolder.TargetPlayerIndex = player.OwnerActorNumber;
                interactionUI.gameObject.SetActive(true);
                interactionUI.Initalize(player, Interaction_State.Player);
                Debug.Log(player);
            }
            else
            {
                interactionUI.DeactiveObject();
            }
        }
        else
        {
            interactionUI.DeactiveObject();
        }
    }
}
