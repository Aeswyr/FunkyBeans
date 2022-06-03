using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class ClientCombatManager : CombatManager
{
    [SerializeField] private SkillList skillList;
    public SkillList SkillList => skillList;

    [Header("Combat grid")]
    [SerializeField] private Grid combatOverlay;
    public Tilemap selectGrid
    {
        get;
        private set;
    }

    public Tilemap moveGrid
    {
        get;
        private set;
    }

    public Tilemap entityGrid
    {
        get;
        private set;
    }
    public Tilemap highlightGrid
    {
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
    private int numActionsLeft;
    private int numMaxActions;
    public PlayerCombatInterface combatInterface { get; set; }
    public bool isTurn { get; set; }
    private SkillID activeSkill;
    private CombatMode mode = CombatMode.MOVE;
    void Awake()
    {
        selectGrid = combatOverlay.transform.Find("SelectGrid").GetComponent<Tilemap>();
        moveGrid = combatOverlay.transform.Find("MoveGrid").GetComponent<Tilemap>();
        entityGrid = combatOverlay.transform.Find("EntityGrid").GetComponent<Tilemap>();
        highlightGrid = combatOverlay.transform.Find("HighlightGrid").GetComponent<Tilemap>();

        moveCost.SetActive(false);
        moveText = moveCost.transform.Find("Text").GetComponent<TextMeshPro>();

        CombatUIController.Instance?.SetCombatManager(this);
    }

    private Vector3Int lastMouseCell, mouseCell;
    private Vector3 mousePos;
    private void FixedUpdate()
    {
        mousePos = Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos);
        mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        if (mouseCell != lastMouseCell)
            if (CellHasEntity(mouseCell))
                CombatUIController.Instance?.SetDisplayedEntity(GetEntityInCell(mouseCell).entity);
            else
                CombatUIController.Instance?.DisableDisplay();


        if (mode == CombatMode.MOVE)
        {
            DrawCombatMovement();

            if (InputHandler.Instance.action.pressed)
            {
                combatInterface.TryMove(mousePos);
            }
        }
        else if (mode == CombatMode.SELECT)
        {
            DrawCombatHighlight();

            if (InputHandler.Instance.action.pressed)
            {
                combatInterface.TryUseSkill(activeSkill, mousePos);
            }
        }
        else if (mode == CombatMode.GUARD)
        {
            combatInterface.TryDefend();
            SetMoveMode();
        }
        lastMouseCell = mouseCell;
    }

    private void DrawCombatMovement()
    {
        if (mouseCell != lastMouseCell)
        {
            ClearMove();

            if (Utils.GridUtil.IsPointInSelectRange(mouseCell, this) && !Utils.GridUtil.IsCellFilled(mouseCell))
            {
                DrawMove(combatInterface.gameObject, mouseCell);
            }
        }


    }
    private void DrawCombatHighlight()
    {
        Vector3Int entityPos = GameHandler.Instance.currentLevel.WorldToCell(combatInterface.transform.position);

        if (mouseCell != lastMouseCell)
        {
            ClearHighlight();
            Skill skill = skillList.Get(activeSkill);
            List<Vector3Int> positions = Utils.CombatUtil.GetTilesInAttack(entityPos, mousePos, this, skill.target, skill.range, skill.size);
            DrawHighlight(positions);
        }
    }

    private Dictionary<Vector3Int, int> bfsDist = new Dictionary<Vector3Int, int>();
    private Queue<Vector3Int> bfs = new Queue<Vector3Int>();

    public void DrawSelect(GameObject src, int dist)
    {
        bfs.Clear();
        bfsDist.Clear();

        Vector3Int start = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);

        bfs.Enqueue(start);
        bfsDist[start] = 0;

        while (bfs.Count > 0)
        {
            var current = bfs.Dequeue();

            selectGrid.SetTile(current, selectTile);
            var cost = bfsDist[current] + 1;
            if (cost > dist)
                continue;
            foreach (var next in Utils.GridUtil.GetValidAdjacent(current, this, true))
            {
                if (bfsDist.ContainsKey(next))
                    continue;

                bfs.Enqueue(next);
                bfsDist[next] = cost;
            }

            foreach (var next in Utils.GridUtil.GetValidAdjacent(current, this))
            {
                if (CellHasEntity(next) && selectGrid.GetTile(next) == null)
                    selectGrid.SetTile(next, selectTile);
            }
        }
    }

    public void ClearSelect()
    {
        selectGrid.ClearAllTiles();
    }

    public void DrawHighlight(List<Vector3Int> positions)
    {
        if (positions == null)
            return;

        foreach (var pos in positions)
            highlightGrid.SetTile(pos, highlightTile);
    }

    public void ClearHighlight()
    {
        highlightGrid.ClearAllTiles();
    }

    public bool CellHasEntity(Vector3Int cell)
    {
        return entityGrid.GetTile(cell) != null;
    }

    public void SetTargetMode(SkillID activeSkill)
    {
        ClearSelect();
        ClearMove();
        this.activeSkill = activeSkill;
        this.mode = CombatMode.SELECT;
        lastMouseCell = int.MaxValue * Vector3Int.one;
    }

    public void SetMoveMode()
    {
        this.mode = CombatMode.MOVE;
        ClearHighlight();
        DrawSelect(combatInterface.gameObject, numActionsLeft);
    }

    public void SetDefendMode()
    {
        this.mode = CombatMode.GUARD;
        ClearSelect();
        ClearMove();
        ClearHighlight();
    }

    public void SetIdleMode()
    {
        this.mode = CombatMode.NONE;
        ClearSelect();
        ClearMove();
        ClearHighlight();
    }

    public void DrawMove(GameObject src, Vector3Int dst)
    {
        //ClearText();
        Vector3Int srcPos = GameHandler.Instance.currentLevel.WorldToCell(src.transform.position);

        List<Vector3Int> positions = Utils.Pathfinding.GetPath(srcPos, dst, this, true, true);


        moveText.text = positions.Count.ToString();

        bool dstHasEntity = CellHasEntity(dst);
        if (dstHasEntity)
        {
            moveText.text = (positions.Count - 1).ToString();
            positions.Remove(dst);
        }

        if (positions.Count > 0)
        {
            moveCost.transform.position = new Vector3(0.5f, 0.5f, 0) + GameHandler.Instance.currentLevel.CellToWorld(positions[0]) + (0.25f * Vector3.up);
            moveCost.SetActive(true);
        }

        foreach (var pos in positions)
            moveGrid.SetTile(pos, pointerTile);

        if (positions.Count > 0)
            moveGrid.SetTile(srcPos, pointerTile);

        //Utils.Pathfinding.PrintPathCosts();
    }

    public void ClearMove()
    {
        moveCost.SetActive(false);
        moveGrid.ClearAllTiles();
    }

    public EntityReference GetEntityInCell(Vector3Int cell)
    {
        if (CellHasEntity(cell))
        {
            Collider2D col = Physics2D.OverlapPoint(entityGrid.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0), LayerMask.GetMask(new string[] { "TileEntity" }));
            if (col == null)
                return null;
            GameObject obj = col.gameObject;
            return obj.GetComponent<EntityReference>();
        }
        return null;
    }

    public bool IsPlayerTurn()
    {
        return isTurn;
    }

    private enum CombatMode
    {
        NONE, MOVE, SELECT, GUARD,
    }
}