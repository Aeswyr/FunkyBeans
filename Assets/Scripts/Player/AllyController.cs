using TMPro;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AllyController : NetworkBehaviour
{
    //[SerializeField] private float speed;
    private PlayerController playerController;
    private PlayerCombatInterface combatInterface;
    [SerializeField] private PlayerCombatInterface allyCombatInterface;
    [field: SerializeField] public CombatEntity CombatEntity { get; private set; }

    /// <summary>
    /// how many elements in the player pos array are allocated for each ally
    /// </summary>
    [SerializeField] private int entriesNeededForEachAlly;

    private int startingIndex;

    /// <summary>
    /// Called once this ally is summoned and added to the player's party outside of combat.
    /// Sets information to know what positions to follow.
    /// </summary>
    /// <param name="alliesAhead"> How many allies are ahead of this one (0 to #summonAllies-1) </param>
    [Server]
    public void OnSummon(int _alliesAhead, PlayerController _playerController)
    {
        playerController = _playerController;
        combatInterface = playerController.CombatInterface;

        startingIndex = entriesNeededForEachAlly * (_alliesAhead + 1) - 1;

        allyCombatInterface.SetOwner(combatInterface);
    }

    [ClientRpc]
    private void OnSummonClient(int _alliesAhead, PlayerController _playerController)
    {
        playerController = _playerController;
        combatInterface = playerController.CombatInterface;

        startingIndex = entriesNeededForEachAlly * (_alliesAhead + 1) - 1;

        allyCombatInterface.SetOwner(combatInterface);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (combatInterface == null)
            return;

        if (!combatInterface.IsOwnedByMe())
            return;

        if (playerController.LastPositions.Count > startingIndex)
            transform.position = playerController.LastPositions[startingIndex];
    }
}
