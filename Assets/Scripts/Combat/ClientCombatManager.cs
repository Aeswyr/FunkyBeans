using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using System;

public class ClientCombatManager : CombatManager
{
    //funny fortnite code go here :)

    [Client]
    private void LocalPlayerOnTurnStarted(int newNumActionsLeft, int newNumMaxActions)
    {

    }
}