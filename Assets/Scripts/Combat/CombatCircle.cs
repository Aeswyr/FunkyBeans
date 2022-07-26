using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCircle : MonoBehaviour
{
    private PlayerController player;
    private long combatID;
    [SerializeField] private Interactable interactable;

    public void SetCombatID(long newID)
    {
        combatID = newID;
    }

    public void JoinCombat()
    {
        player = interactable.Player;

        GameHandler.Instance.AddPlayerToExistingCombat(player.transform.GetComponent<CombatID>().CID, combatID);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(player != null)
        {
            player.RemoveInteractable(interactable);
            player.SetCombatPopupActive(false);
        }

        GameHandler.Instance.DestroyCombatCircleWithID(combatID);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.transform.parent.GetComponent<PlayerController>().SetCombatPopupActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other.transform.parent.GetComponent<PlayerController>().SetCombatPopupActive(false);
    }
}
