using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using System;

public class ServerCombatManager : CombatManager
{
    //Combat data
    private CombatEntity currEntity;
    private List<CombatEntity> combatEntities;
    private PriorityQueue<CombatEntity> turnOrder;

    private List<CombatEntity> playerEntities = new List<CombatEntity>();
    private List<CombatEntity> enemyEntities = new List<CombatEntity>();

    [SerializeField] private float speedMultiplier = 50;
    [SerializeField] private float timeToShowOnBar;

    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        entityGrid = combatOverlay.transform.Find("EntityGrid").GetComponent<Tilemap>();
    }

    /// <summary>
    /// Sets which entities should be in this combat, and generates the turn order
    /// </summary>
    /// <param name="newEntities"> list of entities to be in combat </param>
    [Server]
    public void SetCombatEntities(List<CombatEntity> newEntities)
    {
        combatEntities = newEntities;
        foreach (var entity in combatEntities) {
            if (entity.team == CombatEntity.EntityType.player)
                CombatUIController.Instance?.RegisterNewResource(entity);

            Utils.GridUtil.SnapToLevelGrid(entity.gameObject, this);
            EntityEnterTile(entity.gameObject);
        }
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
            entity.SetServerCombatManager(this);

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
            //If it's currently someone's turn, end it before starting the next one (pretty much just clear local player's UI)

            CombatEntity.EntityType type = currEntity.team;
            switch (type)
            {
                case CombatEntity.EntityType.player:
                    {
                        if(TryGetPlayerCombatInterface(out var player))
                        {
                            player.NotifyTurnEnd();
                        }

                        break;
                    }
                case CombatEntity.EntityType.enemy:
                    {
                        break;
                    }
            }
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

        //TODO SetTurnOrderUIOnClients();

        ServerOnTurnStarted();
    }

    [Server]
    private void ServerOnTurnStarted()
    {
        CombatEntity.EntityType type = currEntity.team;

        //Set number of actions
        numMaxActions = currEntity.Stats.actions;
        numActionsLeft = numMaxActions;

        Debug.Log("Turn started! Current Entity: " + currEntity.transform.name + ", team: " + type);

        switch (type)
        {
            case CombatEntity.EntityType.player:
                {
                    //Enable UI stuff for the player who owns this unit (also draw movement grid)

                    //TODO LocalPlayerOnTurnStarted();
                    if (TryGetPlayerCombatInterface(out var player))
                    {
                        player.NotifyTurnStart(numMaxActions);
                    }

                    break;
                }
            case CombatEntity.EntityType.enemy:
                {
                    StartCoroutine(CalculateEnemyMove());

                    break;
                }
        }
    }

    private IEnumerator CalculateEnemyMove()
    {
        yield return new WaitForSeconds(0.25f);

        Vector3Int posBeforeMove = GameHandler.Instance.currentLevel.WorldToCell(currEntity.transform.position);

        //Debug.Log("Pos at start of turn: " + posBeforeMove);

        List<MoveHeuristic> movesToDo = GetBestMove(posBeforeMove, numActionsLeft, currEntity);

        int actionsLeft = numMaxActions;

        foreach (MoveHeuristic move in movesToDo)
        {
            if (move.movePosition != posBeforeMove) //entity changed position
            {
                int actionsToUse = Utils.GridUtil.ManhattanDistance(move.movePosition, posBeforeMove);
                actionsLeft -= actionsToUse;
                string actionText = actionsToUse == 1 ? " action" : " actions";
                Debug.Log("Used " + actionsToUse + actionText + ". Moved to " + move.movePosition + ", (change of " + (move.movePosition - posBeforeMove).ToString() + "), " +
                    actionsLeft + "/" + numMaxActions + " actions left");

                GameObject entity = currEntity.transform.gameObject;

                EntityExitTile(entity);

                entity.transform.position = move.movePosition;
                Utils.GridUtil.SnapToLevelGrid(entity, this);

                EntityEnterTile(entity);

                posBeforeMove = move.movePosition;
            }
            else //entity used skill
            {
                if (move.skillId == SkillID.NULL)
                {
                    //Debug.Log("About to use Skill NULL, means turn is ending");
                    continue;
                }

                int actionsToUse = skillList.Get(move.skillId).actionCost;
                actionsLeft -= actionsToUse;
                string actionText = actionsToUse == 1 ? " action" : " actions";
                Debug.Log("Used " + actionsToUse + actionText + ". Used Skill: " + move.skillId + ", " + actionsLeft + "/" +
                    numMaxActions + " actions left");

                if (move.skillId == SkillID.BLOCK)
                    currEntity.UseDefense();
                else
                    currEntity.UseSkillAI(move.skillId, move.skillTargPositions);
            }

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.75f);

        Debug.Log("Turn over");

        //Debug.Log("Rat no have brain, skipping turn");

        StartNextTurn();
    }

    private struct MoveHeuristic
    {
        public float score;
        public Vector3Int movePosition;
        public SkillID skillId;
        public List<Vector3Int> skillTargPositions;
    }

    private List<MoveHeuristic> GetBestMove(Vector3Int currPosition, int actionsLeft, CombatEntity entity)
    {
        if (actionsLeft <= 0)
        {
            //This just means don't move anywhere and don't use a skill
            return new List<MoveHeuristic> { new MoveHeuristic { score = GetPositionHeuristic(currPosition), movePosition = currPosition, skillId = SkillID.NULL } };
        }
        else
        {
            #region calulate value of move that starts with blocking
            List<MoveHeuristic> defendMove = new List<MoveHeuristic> { new MoveHeuristic { score = DefendHeuristic(), movePosition = currPosition, skillId = SkillID.BLOCK } };
            defendMove.AddRange(GetBestMove(currPosition, actionsLeft - 1, entity));

            float defendScore = GetScoreFromMoveHeuristicList(defendMove);
            #endregion


            #region find best move that starts with moving
            //Get all possible positions that can be moved to (including staying stationary)
            List<KeyValuePair<Vector3Int, int>> movePositions = Utils.Pathfinding.GetBFSWithDistances(currPosition, actionsLeft, this, true, false);
            movePositions.Remove(new KeyValuePair<Vector3Int, int>(currPosition, 0)); //don't just stand still, do something!!

            //track best place to walk to
            float bestMovementScore = -100000;
            List<MoveHeuristic> bestMovementMove = new List<MoveHeuristic> { new MoveHeuristic { score = -100000, movePosition = currPosition } };

            int spacesToMoveOnBestMove = 0;

            //iterate through all positions, do heuristic + dynamic programming stuff B)
            foreach (KeyValuePair<Vector3Int, int> possibleMove in movePositions)
            {
                Vector3Int posAfterMove = possibleMove.Key; //where are you after moving

                int actionsAfterMove = actionsLeft - possibleMove.Value;

                //check if the player can use a skill after moving here (prevents infite recusion, I hope...)
                bool canUseSkill = false;
                foreach (SkillID skill in entity.KnownSkills)
                {
                    if (skillList.Get(skill).actionCost <= actionsAfterMove)
                        canUseSkill = true;
                }

                List<MoveHeuristic> currMove;
                if (canUseSkill)
                {
                    //represents moving, then using an attack
                    List<MoveHeuristic> movesAfterMoving = GetBestMove(posAfterMove, actionsAfterMove, entity);
                    movesAfterMoving.Insert(0, new MoveHeuristic { movePosition = posAfterMove, score = 0 });

                    currMove = movesAfterMoving;
                }
                else
                {
                    //represents ending the turn here, and blocking
                    currMove = new List<MoveHeuristic>();
                    for (int i = 0; i < actionsAfterMove; i++)
                    {
                        currMove.Add(new MoveHeuristic { movePosition = posAfterMove, score = DefendHeuristic(), skillId = SkillID.BLOCK });
                    }
                    currMove.AddRange(GetBestMove(posAfterMove, 0, entity));
                    currMove.Insert(0, new MoveHeuristic { movePosition = posAfterMove, score = 0 });
                }

                float moveScore = GetScoreFromMoveHeuristicList(currMove);

                if (moveScore > bestMovementScore)
                {
                    bestMovementScore = moveScore;
                    bestMovementMove = currMove;
                    spacesToMoveOnBestMove = possibleMove.Value;
                }
                else
                {
                    //This just makes it so the AI prioritizes taking multiple 1-tile moves, for better visual clarity
                    if (moveScore == bestMovementScore)
                    {
                        if (possibleMove.Value < spacesToMoveOnBestMove)
                        {
                            bestMovementScore = moveScore;
                            bestMovementMove = currMove;
                            spacesToMoveOnBestMove = possibleMove.Value;
                        }
                    }
                }
            }
            #endregion


            #region find best move that starts with a skill
            float bestSkillScore = -100000;
            List<MoveHeuristic> bestSkillMove = new List<MoveHeuristic> { new MoveHeuristic { score = -100000, movePosition = currPosition } }; ;

            foreach (SkillID skillID in entity.KnownSkills)
            {
                Skill skill = skillList.Get(skillID);

                //Debug.Log("checking skill: " + skillID);

                int actionsAfterSkill = actionsLeft - skill.actionCost;
                if (actionsAfterSkill >= 0)
                {
                    //has actions left to use this skill
                    List<List<Vector3Int>> possibleAttackLocations = Utils.CombatUtil.ValidAttackPositionsForSkill(currPosition, skill, this);
                    //Debug.Log(possibleAttackLocations.Count+" possible locations to cast " + skillID+" from position "+currPosition);

                    foreach (List<Vector3Int> attackLocation in possibleAttackLocations)
                    {
                        float attackScore = AttackHeuristic(attackLocation, entity, skill);
                        //Debug.Log("Score for attacking position: " + attackLocation[0] + ": " + attackScore);
                        if (attackScore > bestSkillScore)
                        {
                            List<MoveHeuristic> movesAfterAttack = GetBestMove(currPosition, actionsAfterSkill, entity);
                            movesAfterAttack.Insert(0, new MoveHeuristic { movePosition = currPosition, score = attackScore, skillId = skillID, skillTargPositions = attackLocation });

                            bestSkillMove = movesAfterAttack;

                            bestSkillScore = GetScoreFromMoveHeuristicList(bestSkillMove);
                        }
                    }
                }
            }
            #endregion

            //Put the UseSkill, Defend, and Walk moves into a priorityqueue
            //PriorityQueue<List<MoveHeuristic>> moveToDoQueue = new PriorityQueue<List<MoveHeuristic>>();

            //Debug.Log("Actions Left: "+actionsLeft+", pos: "+currPosition+", scores: \nskill: " + bestSkillScore + ", defend: " + GetScoreFromMoveHeuristicList(defendMove) + ", move: " + bestMovementScore);

            if ((bestSkillScore >= defendScore) && (bestSkillScore >= bestMovementScore))
            {
                //Debug.Log("Chose to use skill " + bestSkillMove[0].skillId);
                return bestSkillMove;
            }

            if (bestMovementScore >= defendScore)
            {
                //Debug.Log("Chose to move to position: " + bestMovementMove[0].movePosition);
                return bestMovementMove;
            }

            //Debug.Log("Chose to block");
            return defendMove;
        }
    }

    #region Heuristic helper functions
    private float GetPositionHeuristic(Vector3Int position)
    {
        float dist = 0;
        foreach (CombatEntity playerEntity in playerEntities)
        {
            dist += Vector3.Distance(position, playerEntity.transform.position);
        }

        return 10f / dist;
    }

    private float GetScoreFromMoveHeuristicList(List<MoveHeuristic> moves)
    {
        float score = 0;
        foreach (MoveHeuristic move in moves)
        {
            score += move.score;
        }
        return score;
    }

    private float DefendHeuristic()
    {
        return 0.00005f;
    }

    private float AttackHeuristic(List<Vector3Int> attackPositions, CombatEntity attackingEntity, Skill skillUsing)
    {
        float score = 0;

        int dmgToDeal = attackingEntity.GetMagnitudeOfSkill(skillUsing);

        foreach (Vector3Int attackPos in attackPositions)
        {
            EntityReference hitEntityRef = GetEntityInCell(attackPos);
            //Debug.Log("hitentity: "+hitEntityRef);
            if (hitEntityRef != null)
            {
                CombatEntity hitEntity = hitEntityRef.entity;
                switch (hitEntity.team)
                {
                    case CombatEntity.EntityType.player:
                        {
                            score += dmgToDeal * 10;

                            if (hitEntity.HP <= dmgToDeal)
                                score += 5000;
                            break;
                        }
                    case CombatEntity.EntityType.enemy:
                        {
                            score -= dmgToDeal * 15f;

                            if (hitEntity.HP <= dmgToDeal)
                                score -= 1000;
                            break;
                        }
                }
            }
        }

        return score;
    }
    #endregion

    public void RemoveEntity(CombatEntity entity)
    {
        combatEntities.Remove(entity);

        switch (entity.team)
        {
            case CombatEntity.EntityType.player:
                {
                    playerEntities.Remove(entity);
                    break;
                }
            case CombatEntity.EntityType.enemy:
                {
                    enemyEntities.Remove(entity);
                    break;
                }
        }

        // Accumilate rewards for defeated enemies
        if (entity.team == CombatEntity.EntityType.enemy)
        {
            //TODO reward.exp += entity.Reward.exp;
        }

        //Get all entities in the current turn order, and make a new priority queue
        List<KeyValuePair<float, CombatEntity>> currElements = turnOrder.GetElements();
        PriorityQueue<CombatEntity> newTurnOrder = new PriorityQueue<CombatEntity>();

        //Add each entity to the new priority queue, but with the updated time till their turn
        foreach (KeyValuePair<float, CombatEntity> element in currElements)
        {
            if (element.Value.Equals(entity))
            {
                //don't add this entity back into priorityqueue
            }
            else
            {
                newTurnOrder.Put(element.Value, element.Key);
            }
        }

        turnOrder = newTurnOrder;

        // TODO UpdateTurnIndicatorUI();

        if (currEntity.Equals(entity))
        {
            //killed entity's turn, so move to next turn
            StartNextTurn();
        }

        var team = combatEntities[0].team;
        foreach (var centity in combatEntities)
        {
            if (team != centity.team)
                return;
        }
        if (team == CombatEntity.EntityType.player)
        {
            EndCombat();
        }
    }


    public void EntityEnterTile(GameObject entity)
    {
        var pos = entityGrid.WorldToCell(entity.transform.position);
        SetEntityTile(pos, entityTile);
    }

    public void EntityExitTile(GameObject entity)
    {
        var pos = entityGrid.WorldToCell(entity.transform.position);
        SetEntityTile(pos, null);
    }

    private void SetEntityTile(Vector3Int pos, TileBase tile)
    {
        bool entering = tile != null;
        entityGrid.SetTile(pos, tile);
        NotifyMovement(pos, entering);
    }

    public int currentCombo;
    public Skill.Type[] lastComboTypes = new Skill.Type[0];
    public List<SkillID> comboSkillsUsed = new List<SkillID>();
    public void IncrementCombo()
    {
        currentCombo++;
        CombatUIController.Instance?.SetComboCounter(currentCombo);
    }

    public void CashoutCombo()
    {
        comboSkillsUsed.Clear();
        currentCombo = -1;
        CombatUIController.Instance?.SetComboCounter(currentCombo);
    }

    public void TryMovePlayer(Vector3 position, CombatEntity entity)
    {
        if (entity != currEntity)
            return;
        Vector3Int source = combatOverlay.WorldToCell(currEntity.transform.position);
        Vector3Int dest = combatOverlay.WorldToCell(position);

        List<Vector3Int> pathToDest = Utils.Pathfinding.GetPath(source, dest, this, true, false);
        if ((pathToDest.Count <= numActionsLeft) && (pathToDest.Count > 0))
        {
            //have enough actions to move

            //Remove entity from prev tile
            EntityExitTile(currEntity.gameObject);

            //Move that lad to the destination (might need to wait here, idk if next line will work)
            if (TryGetPlayerCombatInterface(out var combatInterface)) {
                currEntity.transform.position = combatOverlay.CellToWorld(pathToDest[0]);
                Utils.GridUtil.SnapToLevelGrid(currEntity.gameObject, this);
                combatInterface.NotifyMovePlayer(currEntity.transform.position);
            }

            //Have entity enter new tile
            EntityEnterTile(currEntity.gameObject);

            //Update remaining actions and check to end turn
            UseActions(pathToDest.Count);
        }
        else
        {
            //Bro stop CHEATIN!
            Debug.LogError("Entity " + currEntity.gameObject.name + " tried to move to invalid location");
        }
    }

    public void TryUseSkill(SkillID skill, Vector3 position, CombatEntity entity)
    {
        if (entity != currEntity)
            return;
        if (!currEntity.KnownSkills.Contains(skill))
            return;

        int cost = skillList.Get(skill).actionCost;
        if (numActionsLeft >= cost)
        {
            currEntity.GetComponent<SkillActions>().mousePos = position;

            currEntity.UseSkill(skill);
        }
    }

    public void TryUseDefend(CombatEntity entity)
    {
        if (entity != currEntity)
            return;
        if(numActionsLeft >= skillList.Get(currEntity.DefendSkill).actionCost)
            currEntity.UseDefense();
    }

    public void UseActions(int actionsToUse)
    {
        if (currEntity.team == CombatEntity.EntityType.enemy)
        {
            Debug.Log("Useactions should not be called on enemies, get outta heeeeeeeeeeeere");
            return;
        }

        //Debug.Log("player used " + actionsToUse + " actions");

        numActionsLeft -= actionsToUse;

        //update turn ui for local player
        if (TryGetPlayerCombatInterface(out var player))
        {
            player.NotifyResourceChange(player.transform.GetComponent<CombatID>().CID, ResourceType.ACTIONS, actionsToUse);
        }

        //check if should end turn
        if (numActionsLeft <= 0)
        {
            StartNextTurn();
        }
    }

    private CombatReward reward;

    public void EndCombat()
    {
        GameHandler.Instance.ExitCombat(id);
    }

    public void NotifyResourceChange(long id, ResourceType type, int delta) {
        foreach (CombatEntity entity in combatEntities)
            if (entity.transform.TryGetComponent(out PlayerCombatInterface player))
                player.NotifyResourceChange(id, type, delta);
    }

    public void NotifyMovement(Vector3Int pos, bool entering) {
        foreach (CombatEntity entity in combatEntities)
            if (entity.transform.TryGetComponent(out PlayerCombatInterface player))
                player.NotifyMovement(pos, entering);
    }

    public bool TryGetPlayerCombatInterface(out PlayerCombatInterface player)
    {
        return currEntity.transform.TryGetComponent(out player);
    }
}