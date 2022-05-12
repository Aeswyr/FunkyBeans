using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameHandler : Singleton<GameHandler>
{
    [Header("Gameplay grids")]
    [SerializeField] private Grid currentLevel;
    public Tilemap floorGrid {
        get;
        private set;
    }
    public Tilemap wallGrid {
        get;
        private set;
    }
    public Grid CurrentLevel {
        get {return currentLevel;}
    }
    [SerializeField] private Grid combatOverlay;
    public Tilemap selectGrid {
        get;
        private set;
    }
    
    public Tilemap moveGrid {
        get;
        private set;
    }

    [Header("Tiles for battle map overlay")]
    [SerializeField] private RuleTile selectTile;
    [SerializeField] private RuleTile pointerTile;

    void Start() {
        floorGrid = currentLevel.transform.Find("Collision").GetComponent<Tilemap>();
        wallGrid = currentLevel.transform.Find("Walls").GetComponent<Tilemap>();

        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();
    }

    public void SnapToLevelGrid(GameObject entity) {
        entity.transform.position = new Vector3(0.5f, 0.5f, 0) + currentLevel.CellToWorld(currentLevel.WorldToCell(entity.transform.position));
    }

    public void DrawSelect(GameObject src, int dist) {
        Vector3Int gridPos = currentLevel.WorldToCell(src.transform.position);
        dist++;
        DrawSelectRecursive(gridPos, dist);
    }

    public void ClearSelect() {
        selectGrid.ClearAllTiles();
    }

    private void DrawSelectRecursive(Vector3Int pos, int dist) {
        if (dist <= 0 || Utils.GridUtil.IsCellFilled(pos))
            return;
        dist--;

        selectGrid.SetTile(pos, selectTile);

        DrawSelectRecursive(pos + Vector3Int.right, dist);
        DrawSelectRecursive(pos + Vector3Int.left, dist);
        DrawSelectRecursive(pos + Vector3Int.up, dist);
        DrawSelectRecursive(pos + Vector3Int.down, dist);
    }

    public void DrawMove(GameObject src, Vector3Int dst) {
        Vector3Int srcPos = currentLevel.WorldToCell(src.transform.position);

        List<Vector3Int> positions = Utils.Pathfinding.GetPathInSelectRange(srcPos, dst);
        foreach (var pos in positions)
            moveGrid.SetTile(pos, pointerTile);
        if (positions.Count > 0)
            moveGrid.SetTile(srcPos, pointerTile);
    }



    public void ClearMove() {
        moveGrid.ClearAllTiles();
    }


}
