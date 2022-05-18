using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private SkillList skillList;

    [Header("Combat grid")]
    [SerializeField] private Grid combatOverlay;
    public Tilemap selectGrid {
        get;
        private set;
    }
    
    public Tilemap moveGrid {
        get;
        private set;
    }

    public Tilemap entityGrid {
        get;
        private set;
    }
    public Tilemap highlightGrid {
        get;
        private set;
    }

    [Header("Tiles for battle map overlay")]
    [SerializeField] private RuleTile selectTile;
    [SerializeField] private RuleTile pointerTile;
    [SerializeField] private RuleTile entityTile;
    [SerializeField] private RuleTile highlightTile;

    [Header("UI Components")]
    [SerializeField] private GameObject moveCost;
    private TextMeshPro moveText;

    [Header("Turn Indicator Stuff")]
    [SerializeField] private GameObject entityTurnIndicatorPrefab;
    [SerializeField] private float timeToShowOnBar;

    private List<CombatEntity> combatEntities;
    private PriorityQueue<CombatEntity> turnOrder;
    private CombatEntity currEntity;
    private List<EntityTurnIndicator> turnIndicators = new List<EntityTurnIndicator>();

    [SerializeField] private float speedMultiplier;

    private int numActionsLeft;
    private int numMaxActions;
    private CombatMode mode = CombatMode.MOVE;
    private SkillID activeSkill;

    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();
        entityGrid = combatOverlay.transform.Find("EntityGrid").GetComponent<Tilemap>();
        highlightGrid = combatOverlay.transform.Find("HighlightGrid").GetComponent<Tilemap>();

        moveCost.SetActive(false);
        moveText = moveCost.transform.Find("Text").GetComponent<TextMeshPro>();

        CombatUIController.Instance.SetCombatManager(this);
    }

    private void FixedUpdate()
    {
        if (currEntity != null)
        {
            switch (currEntity.EntitiyType)
            {
                case CombatEntity.EntityType.player:
                    {
                        if (mode == CombatMode.MOVE)
                        {
                            DrawCombatMovement();

                            if (InputHandler.Instance.action.pressed)
                            {
                                TryMovePlayer();
                            }
                        }
                        else if (mode == CombatMode.SELECT)
                        {
                            DrawCombatHighlight();

                            if (InputHandler.Instance.action.pressed)
                            {
                                TryUseSkill(activeSkill);
                            }
                        }
                        break;
                    }
                case CombatEntity.EntityType.enemy:
                    {
                        if (InputHandler.Instance.action.pressed)
                        {
                            StartNextTurn();
                        }
                        break;
                    }
            }
        }
    }

    private Dictionary<Vector3Int, int> bfsDist = new Dictionary<Vector3Int, int>();
    private Queue<Vector3Int> bfs = new Queue<Vector3Int>();

    public void DrawSelect(GameObject src, int dist) {
        bfs.Clear();
        bfsDist.Clear();

        Vector3Int start = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);

        bfs.Enqueue(start);
        bfsDist[start] = 0;

        while (bfs.Count > 0) {
            var current = bfs.Dequeue();
            
            selectGrid.SetTile(current, selectTile);
            var cost = bfsDist[current] + 1;
            if (cost > dist)
                continue;
            foreach (var next in Utils.GridUtil.GetValidAdjacent(current, this, true)) {
                if (bfsDist.ContainsKey(next))
                    continue;
                
                bfs.Enqueue(next);
                bfsDist[next] = cost;
            }

            foreach (var next in Utils.GridUtil.GetValidAdjacent(current, this)) {
                if (CellHasEntity(next) && selectGrid.GetTile(next) == null)
                    selectGrid.SetTile(next, selectTile);
            }
        }
    }

    public void ClearSelect() {
        selectGrid.ClearAllTiles();
    }

    public void DrawHighlight(List<Vector3Int> positions) {
        if (positions == null)
            return;

        foreach (var pos in positions)
            highlightGrid.SetTile(pos, highlightTile);
    }

    public void ClearHighlight() {
        highlightGrid.ClearAllTiles();
    }

    public bool CellHasEntity(Vector3Int cell) {
        return entityGrid.GetTile(cell) != null;
    }

    public EntityReference GetEntityInCell(Vector3Int cell) {
        GameObject obj = entityGrid.GetInstantiatedObject(cell);
        if (obj != null) {
            return obj.GetComponent<EntityReference>();
        }
        return null;
    }

    public void EntityEnterTile(GameObject entity) {
        var pos = entityGrid.WorldToCell(entity.transform.position);
        entityGrid.SetTile(pos, entityTile);
        StartCoroutine(DelaySetEntityAtTile(entity, pos));
    }

    private IEnumerator DelaySetEntityAtTile(GameObject entity, Vector3Int pos) {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 2);
        GameObject tileObj = entityGrid.GetInstantiatedObject(pos);
        if (tileObj == null)
            yield break;
        EntityReference reference = tileObj.GetComponent<EntityReference>();
        reference.entity = entity.GetComponentInChildren<CombatEntity>();
        reference.entityObj = entity;
    }

    public void EntityExitTile(GameObject entity) {
        var pos = entityGrid.WorldToCell(entity.transform.position);
        entityGrid.SetTile(pos, null);
    }

    public void ClearEntityTiles() {

    }

    public void SetCombatEntities(List<CombatEntity> newEntities)
    {
        combatEntities = newEntities;
        GenerateTurnOrder();
    }

    public void GenerateTurnOrder()
    {
        turnOrder = new PriorityQueue<CombatEntity>();

        foreach (CombatEntity entity in combatEntities)
        {
            entity.SetCombatManager(this);

            float posOnBar = speedMultiplier/entity.Stats.speed;

            turnOrder.Put(entity, posOnBar);
            posOnBar += speedMultiplier / entity.Stats.speed;

            while (posOnBar <= timeToShowOnBar)
            {
                turnOrder.Put(entity, posOnBar);
                posOnBar += speedMultiplier/entity.Stats.speed;
            }
        }

        StartNextTurn();
    }

    public void SetTargetMode(SkillID activeSkill) {
        ClearSelect();
        ClearMove();
        this.activeSkill = activeSkill;
        this.mode = CombatMode.SELECT;
        lastMouseCell = int.MaxValue * Vector3Int.one;
    }

    public void SetMoveMode() {
        this.mode = CombatMode.MOVE;
        ClearHighlight();
    }

    private void StartNextTurn()
    {
        if(currEntity != null)
        {
            //Debug.Log("Skipping current Turn!");

            TurnEnded();

            //Debug.LogError("Tried to start next turn mid-turn for entity: " + currEntity.name);
            //return;
        }

        //Debug.Log("before turn change:");
        //turnOrder.PrintCosts();

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
        newTurnOrder.Put(currEntity, (speedMultiplier/currEntity.Stats.speed) * numCopies);

        turnOrder = newTurnOrder;

        UpdateTurnIndicatorUI();

        //Debug.Log("after turn change:");
        //turnOrder.PrintCosts();

        TurnStarted();
    }

    private void TurnStarted()
    {
        CombatEntity.EntityType type = currEntity.EntitiyType;
        switch (type)
        {
            case CombatEntity.EntityType.player:
                {
                    //Set number of actions
                    numMaxActions = currEntity.Stats.actions;
                    numActionsLeft = numMaxActions;

                    CombatUIController.Instance.SetKnownSkills(currEntity.KnownSkills);

                    ActionUIController.Instance.SetActionUI(numActionsLeft, numMaxActions);

                    //Show movement grid for player's entities
                    DrawSelect(currEntity.gameObject, numActionsLeft);

                    break;
                }
            case CombatEntity.EntityType.enemy:
                {
                    break;
                }
        }
    }

    private Vector3Int lastMouseCell;
    private void DrawCombatMovement()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos);
        Vector3Int mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        if (mouseCell != lastMouseCell)
        {
            ClearMove();

            if (Utils.GridUtil.IsPointInSelectRange(mouseCell, this) && !Utils.GridUtil.IsCellFilled(mouseCell))
            {
                DrawMove(currEntity.transform.parent.gameObject, mouseCell);
            }
        }

        lastMouseCell = mouseCell;
    }
    private void DrawCombatHighlight() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos);
        Vector3Int mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        Vector3Int entityPos = GameHandler.Instance.currentLevel.WorldToCell(currEntity.transform.parent.position);

        if (mouseCell != lastMouseCell)
        {
            ClearHighlight();
            Skill skill = skillList.Get(activeSkill);
            List<Vector3Int> positions = Utils.CombatUtil.GetTilesInAttack(entityPos, mousePos, this, skill.target, skill.range, skill.size); 
            DrawHighlight(positions);
        }

        lastMouseCell = mouseCell;
    }

    private void TryMovePlayer()
    {
        if (lastMouseCell == null)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos);
        Vector3Int mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        if (Utils.GridUtil.IsPointInSelectRange(mouseCell, this) && !Utils.GridUtil.IsCellFilled(mouseCell) && !CellHasEntity(mouseCell))
        {
            Vector3Int srcPos = GameHandler.Instance.currentLevel.WorldToCell(currEntity.transform.parent.position);

            List<Vector3Int> positions = Utils.Pathfinding.GetPath(srcPos, mouseCell, this, true, true);
            if(positions.Count > 0)
            {
                GameObject parentOfEntity = currEntity.transform.parent.gameObject;

                //path is valid, move to destination
                EntityExitTile(parentOfEntity);

                parentOfEntity.transform.position = selectGrid.CellToWorld(positions[0]);
                Utils.GridUtil.SnapToLevelGrid(parentOfEntity, this);

                EntityEnterTile(parentOfEntity);

                UseActions(positions.Count);
            }
        }
    }

    private void TryUseSkill(SkillID skill)
    {
        int cost = skillList.Get(skill).actionCost;
        if (numActionsLeft >= cost)
        {
            currEntity.UseSkill(skill);
            //UseActions(cost);
        }
    }

    public void UseActions(int actionsToUse)
    {
        numActionsLeft -= actionsToUse;
        ActionUIController.Instance.SetActionUI(numActionsLeft, numMaxActions);

        ClearMove();
        ClearSelect();
        ClearHighlight();

        DrawSelect(currEntity.gameObject, numActionsLeft);

        if (numActionsLeft <= 0)
            StartNextTurn();
    }

    private void TurnEnded()
    {
        CombatEntity.EntityType type = currEntity.EntitiyType;
        switch (type)
        {
            case CombatEntity.EntityType.player:
                {
                    //Get rid of UI movement stuff
                    ClearMove();
                    ClearSelect();
                    break;
                }
            case CombatEntity.EntityType.enemy:
                {
                    break;
                }
        }
    }

    public void UpdateTurnIndicatorUI()
    {
        List<KeyValuePair<float, CombatEntity>> currElements = turnOrder.GetElements();

        TurnOrderCanvas.Instance.SetCurrEntitySprite(currEntity.UISprite);

        //delete all current turn indicators
        while(turnIndicators.Count > 0)
        {
            Destroy(turnIndicators[0].gameObject);
            turnIndicators.RemoveAt(0);
        }

        //create and set position for every entity in turnOrder
        foreach (KeyValuePair<float, CombatEntity> element in currElements)
        {
            EntityTurnIndicator newIndicator = Instantiate(entityTurnIndicatorPrefab, TurnOrderCanvas.Instance.transform).GetComponent<EntityTurnIndicator>();
            turnIndicators.Add(newIndicator);

            newIndicator.SetSprite(element.Value.UISprite);

            TurnOrderCanvas.Instance.PlaceTurnEntity(newIndicator.transform, element.Key / timeToShowOnBar);
        }
    }

    public void DrawMove(GameObject src, Vector3Int dst) {
        //ClearText();
        Vector3Int srcPos = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);

        List<Vector3Int> positions = Utils.Pathfinding.GetPath(srcPos, dst, this, true, true);

        
        moveText.text = positions.Count.ToString();

        bool dstHasEntity = CellHasEntity(dst);
        if (dstHasEntity) {
            moveText.text = (positions.Count - 1).ToString();
            positions.Remove(dst);
        }

        if (positions.Count > 0) {
            moveCost.transform.position = new Vector3(0.5f, 0.5f, 0) + GameHandler.Instance.currentLevel.CellToWorld(positions[0]) + (0.25f * Vector3.up);
            moveCost.SetActive(true);
        }

        foreach (var pos in positions)
            moveGrid.SetTile(pos, pointerTile);
            
        if (positions.Count > 0)
            moveGrid.SetTile(srcPos, pointerTile);

        //Utils.Pathfinding.PrintPathCosts();
    }



    public void ClearMove() {
        moveCost.SetActive(false);
        moveGrid.ClearAllTiles();
    }

    private enum CombatMode {
    NONE, MOVE, SELECT
    }
}


