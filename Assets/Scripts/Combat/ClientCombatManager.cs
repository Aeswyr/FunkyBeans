using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using System;

public class ClientCombatManager : NetworkBehaviour
{
    //funny fortnite code go here :)

    [Client]
    private void LocalPlayerOnTurnStarted(int newNumActionsLeft, int newNumMaxActions)
    {
        CombatEntity localCurrEntity;
        if (localCurrEntity.LocalIsMine == false)
            return;

        numActionsLeft = newNumActionsLeft;
        numMaxActions = newNumMaxActions;

        CombatUIController.Instance?.SetKnownSkills(currEntity.KnownSkills);

        CombatUIController.Instance?.SetActionUI(numActionsLeft, numMaxActions);

        //Show movement grid for player's entities
        DrawSelectForLocalPlayer(currEntity.gameObject, numActionsLeft);

    }
}