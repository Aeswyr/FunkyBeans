using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameHandler : Singleton<GameHandler>
{
    [Header("Gameplay grids")]
    [SerializeField] private Grid currentLevel;
    private Tilemap floorGrid;
    private Tilemap wallGrid;
    [SerializeField] private Grid combatOverlay;
    private Tilemap selectGrid;
    private Tilemap moveGrid;

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
        DrawSelectRecursive(gridPos, dist);
    }

    public void ClearSelect() {
        selectGrid.ClearAllTiles();
    }

    private void DrawSelectRecursive(Vector3Int pos, int dist) {
        if (dist <= 0  
        || floorGrid.GetTile(pos) == null 
        || wallGrid.GetTile(pos) != null)
            return;
        dist--;

        selectGrid.SetTile(pos, selectTile);

        DrawSelectRecursive(pos + Vector3Int.right, dist);
        DrawSelectRecursive(pos + Vector3Int.left, dist);
        DrawSelectRecursive(pos + Vector3Int.up, dist);
        DrawSelectRecursive(pos + Vector3Int.down, dist);
    }

    private bool IsPointInRange(GameObject src, Vector3 point, int dist) {
        Vector2Int gridSrc = (Vector2Int)currentLevel.WorldToCell(src.transform.position);
        Vector2Int gridPoint = (Vector2Int)currentLevel.WorldToCell(point);
        return Utils.ManhattanDistance(gridPoint, gridSrc) <= dist;
    }
}
