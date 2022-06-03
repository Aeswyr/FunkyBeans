using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using System;

public class ServerCombatManager : NetworkBehaviour
{
    //Combat data
    private CombatEntity currEntity;
    private List<CombatEntity> combatEntities;
    private PriorityQueue<CombatEntity> turnOrder;

    private List<CombatEntity> playerEntities = new List<CombatEntity>();
    private List<CombatEntity> enemyEntities = new List<CombatEntity>();

    [SerializeField] private float speedMultiplier = 50;
    [SerializeField] private float timeToShowOnBar;

    private int numActionsLeft;
    private int numMaxActions;

    /// <summary>
    /// Sets which entities should be in this combat, and generates the turn order
    /// </summary>
    /// <param name="newEntities"> list of entities to be in combat </param>
    [Server]
    public void SetCombatEntities(List<CombatEntity> newEntities)
    {
        combatEntities = newEntities;
        foreach (var entity in combatEntities)
            if (entity.team == CombatEntity.EntityType.player)
                CombatUIController.Instance?.RegisterNewResource(entity);
        GenerateTurnOrder();
    }

    /// <summary>
    /// Generates a turn order of CombatEntities, and then starts the first turn
    /// </summary>
    [Server]
    public void GenerateTurnOrder()
    {
        turnOrder = new PriorityQueue<CombatEntity>();

        foreach (CombatEntity entity in combatEntities)
        {
            entity.SetCombatManager(this);

            switch (entity.team)
            {
                case CombatEntity.EntityType.player:
                    {
                        playerEntities.Add(entity);
                        break;
                    }
                case CombatEntity.EntityType.enemy:
                    {
                        enemyEntities.Add(entity);
                        break;
                    }
            }

            float posOnBar = speedMultiplier / entity.Stats.speed;

            turnOrder.Put(entity, posOnBar);
            posOnBar += speedMultiplier / entity.Stats.speed;

            while (posOnBar <= timeToShowOnBar)
            {
                turnOrder.Put(entity, posOnBar);
                posOnBar += speedMultiplier / entity.Stats.speed;
            }
        }

        StartNextTurn();
    }

    [Server]
    private void StartNextTurn()
    {
        if (currEntity != null)
        {
            //If it's currently someone's turn, end it before starting the next one

            EndCurrentTurn();
        }

        //find how much to shift the current "time" and set new currEneity
        float timeChange = turnOrder.GetLowestPriority();

        //Set current Entity
        currEntity = turnOrder.Pop();

        //keeps track of how many copies of each entity show up on the turn bar indicator
        int numCopies = turnOrder.GetNumCopies(currEntity) + 1;

        //Get all entities in the current turn order, and make a new priority queue
        List<KeyValuePair<float, CombatEntity>> currElements = turnOrder.GetElements();
        PriorityQueue<CombatEntity> newTurnOrder = new PriorityQueue<CombatEntity>();

        //Add each entity to the new priority queue, but with the updated time till their turn
        foreach (KeyValuePair<float, CombatEntity> element in currElements)
        {
            newTurnOrder.Put(element.Value, element.Key - timeChange);
        }
        //Finally, add the current entity back into the new priority queue
        newTurnOrder.Put(currEntity, (speedMultiplier / currEntity.Stats.speed) * numCopies);

        turnOrder = newTurnOrder;

        SetTurnOrderUIOnClients();

        ServerOnTurnStarted();
    }

    [Server]
    private void ServerOnTurnStarted()
    {
        CombatEntity.EntityType type = currEntity.team;

        //Set number of actions
        numMaxActions = currEntity.Stats.actions;
        numActionsLeft = numMaxActions;

        Debug.Log("Turn started! Current Entity: " + currEntity.transform.parent.name + ", team: " + type);

        switch (type)
        {
            case CombatEntity.EntityType.player:
                {
                    LocalPlayerOnTurnStarted(numActionsLeft, numMaxActions);
                    break;
                }
            case CombatEntity.EntityType.enemy:
                {
                    StartCoroutine(CalculateEnemyMove());

                    break;
                }
        }
    }
}