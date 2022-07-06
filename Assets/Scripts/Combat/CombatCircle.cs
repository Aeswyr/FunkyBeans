using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCircle : MonoBehaviour
{
    private long combatID;
    [SerializeField] private Interactable interactable;

    public void SetCombatID(long newID)
    {
        combatID = newID;
    }

    public void JoinCombat()
    {
        PlayerController player = interactable.Player;

        player.RemoveInteractable(interactable);

        GameHandler.Instance.AddPlayerToExistingCombat(player.transform.GetComponent<CombatID>().CID, combatID);

        Destroy(gameObject);
    }
}
