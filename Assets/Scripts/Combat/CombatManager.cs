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

    private List<CombatEntity> combatEntities;
    private CombatEntity currEntity;

    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();
        entityGrid = combatOverlay.transform.Find("EntityGrid").GetComponent<Tilemap>();

        moveCost.SetActive(false);
        moveText = moveCost.transform.Find("Text").GetComponent<TextMeshPro>();
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

    public void SetCombatEntities(List<CombatEntity> newEntities)
    {
        combatEntities = newEntities;
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

    public void GenerateTurnOrder()
    {
        float highestSpeed = -1000;
        CombatEntity fastestEntity = null;

        //Find the fastest entity, make them go first
        foreach(CombatEntity entity in combatEntities)
        {
            if(entity.Speed > highestSpeed)
            {
                highestSpeed = entity.Speed;
                fastestEntity = entity;
            }
        }
        currEntity = fastestEntity;
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
