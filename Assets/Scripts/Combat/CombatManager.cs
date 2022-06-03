using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;

public class CombatManager : NetworkBehaviour
{
    
    [SerializeField] protected SkillList skillList;
    public SkillList SkillList => skillList;

    [Header("Combat grid")]
    [SerializeField] protected Grid combatOverlay;
    public Tilemap selectGrid
    {
        get;
        protected set;
    }
    public Tilemap entityGrid {
        get;
        protected set;
    }

    [Header("Tiles for battle map overlay")]
    [SerializeField] protected RuleTile entityTile;


    protected int numActionsLeft;
    protected int numMaxActions;

    protected long id;
    public long ID {
        get {return id;}
        set {id = value;}
    }

    public bool CellHasEntity(Vector3Int cell) {
        return entityGrid.GetTile(cell) != null;
    }

    public EntityReference GetEntityInCell(Vector3Int cell) {
        if (CellHasEntity(cell)) {
            Collider2D col = Physics2D.OverlapPoint(entityGrid.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0), LayerMask.GetMask(new string[] {"TileEntity"}));
            if (col == null)
                return null;
            GameObject obj = col.gameObject;
            return obj.GetComponent<EntityReference>();
        }
        return null;
    }

    private void SetEntityTile(Vector3Int pos, TileBase tile) {
        entityGrid.SetTile(pos, tile);
    }

    public void ClearEntityTiles() {
        entityGrid.ClearAllTiles();
    }

}


