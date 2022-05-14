using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class CombatManager : MonoBehaviour
{

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

    [Header("Tiles for battle map overlay")]
    [SerializeField] private RuleTile selectTile;
    [SerializeField] private RuleTile pointerTile;
    [SerializeField] private RuleTile entityTile;

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

    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();
        entityGrid = combatOverlay.transform.Find("EntityGrid").GetComponent<Tilemap>();

        moveCost.SetActive(false);
        moveText = moveCost.transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void FixedUpdate()
    {
        if(InputHandler.Instance.action.pressed)
        {
            StartNextTurn();
        }
    }

    public void DrawSelect(GameObject src, int dist) {
        Vector3Int gridPos = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);
        dist++;
        DrawSelectRecursive(gridPos, gridPos, dist);
    }

    public void ClearSelect() {
        selectGrid.ClearAllTiles();
    }

    private void DrawSelectRecursive(Vector3Int pos, Vector3Int src, int dist) {
        if (dist <= 0 || Utils.GridUtil.IsCellFilled(pos))
            return;
        dist--;

        selectGrid.SetTile(pos, selectTile);

        DrawSelectRecursive(pos + Vector3Int.right, src, dist);
        DrawSelectRecursive(pos + Vector3Int.left, src, dist);
        DrawSelectRecursive(pos + Vector3Int.up, src, dist);
        DrawSelectRecursive(pos + Vector3Int.down, src, dist);
    }

    public bool CellHasEntity(Vector3Int cell) {
        return entityGrid.GetTile(cell) != null;
    }

    public void EntityEnterTile(GameObject entity) {
        var pos = entityGrid.WorldToCell(entity.transform.position);
        entityGrid.SetTile(pos, entityTile);
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
            float posOnBar = speedMultiplier/entity.Speed;

            while (posOnBar <= timeToShowOnBar)
            {
                turnOrder.Put(entity, posOnBar);
                posOnBar += speedMultiplier/entity.Speed;
            }
        }

        StartNextTurn();
    }

    private void StartNextTurn()
    {
        if(currEntity != null)
        {
            Debug.Log("Skipping current Turn!");

            //Debug.LogError("Tried to start next turn mid-turn for entity: " + currEntity.name);
            //return;
        }

        //Debug.Log("before turn change:");
        turnOrder.PrintCosts();

        //find how much to shift the current "time" and set new currEneity
        float timeChange = turnOrder.GetLowestPriority();
        //Debug.Log("timechange: " + timeChange);
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
        newTurnOrder.Put(currEntity, (speedMultiplier/currEntity.Speed) * numCopies);

        turnOrder = newTurnOrder;

        UpdateTurnIndicatorUI();

        //Debug.Log("after turn change:");
        turnOrder.PrintCosts();
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

    private void DrawCombatMovement()
    {
        /*
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(input.mousePos);
        Vector3Int mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        if (mouseCell != lastMouseCell)
        {
            currentCombat.ClearMove();

            if (Utils.GridUtil.IsPointInSelectRange(mouseCell, currentCombat) && !Utils.GridUtil.IsCellFilled(mouseCell))
            {
                currentCombat.DrawMove(gameObject, mouseCell);
            }
        }

        lastMouseCell = mouseCell;*/
    }


    public void DrawMove(GameObject src, Vector3Int dst) {
        //ClearText();
        Vector3Int srcPos = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);

        List<Vector3Int> positions = Utils.Pathfinding.GetPath(srcPos, dst, this);

        moveCost.SetActive(true);
        moveText.text = positions.Count.ToString();
        moveCost.transform.position = new Vector3(0.5f, 0.5f, 0) + GameHandler.Instance.currentLevel.CellToWorld(dst) + (0.25f * Vector3.up);

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
}
