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

    [Header("Tiles for battle map overlay")]
    [SerializeField] private RuleTile selectTile;
    [SerializeField] private RuleTile pointerTile;

    [Header("UI Components")]
    [SerializeField] private GameObject moveCost;
    private TextMeshPro moveText;

    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();

        moveCost.SetActive(false);
        moveText = moveCost.transform.Find("Text").GetComponent<TextMeshPro>();
    }

    public void DrawSelect(GameObject src, int dist) {
        Vector3Int gridPos = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);
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
