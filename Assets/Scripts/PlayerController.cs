using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private InputHandler input;
    [SerializeField] private float speed;
    [SerializeField] private GameObject attackPrefab;
    private int maxMove = 4;
    private ContactFilter2D filter = new ContactFilter2D();
    private bool freeMove = true;
    private CombatManager currentCombat = null;


    // Start is called before the first frame update
    void Start()
    {
        filter.SetLayerMask(LayerMask.GetMask(new []{"Hurtbox"}));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (freeMove)
            rbody.velocity = speed * input.dir;

        if (input.action.pressed)
            if (freeMove) {
                GameObject attack = Instantiate(attackPrefab, transform);
                attack.GetComponent<AttackController>().SetSource(this);
                attack.transform.rotation = Utils.Rotate(Camera.main.ScreenToWorldPoint(input.mousePos) - transform.position);
            }
            else
                EndBattle();

        DrawCombatMovement();
    }

    private Vector3Int lastMouseCell;
    
    private void DrawCombatMovement() {
        if (currentCombat == null)
            return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(input.mousePos);
        Vector3Int mouseCell = GameHandler.Instance.currentLevel.WorldToCell(mousePos);

        if (mouseCell != lastMouseCell) {
            currentCombat.ClearMove();
            
            if (Utils.GridUtil.IsPointInSelectRange(mouseCell, currentCombat) && !Utils.GridUtil.IsCellFilled(mouseCell)) {
                currentCombat.DrawMove(gameObject, mouseCell);
            }
        }

        lastMouseCell = mouseCell;
    }


    public void StartBattle() {

        var results = new List<RaycastHit2D>();
        Physics2D.CircleCast(transform.position, maxMove, Vector2.right, filter, results, 0);

        currentCombat = GameHandler.Instance.CreateCombatManager();

        foreach (var hit in results) {
            Utils.GridUtil.SnapToLevelGrid(hit.collider.transform.parent.gameObject);
        }

        freeMove = false;
        rbody.velocity = Vector2.zero;
        currentCombat.DrawSelect(gameObject, maxMove);
    }

    public void EndBattle() {
        freeMove = true;
        currentCombat.ClearMove();
        currentCombat.ClearSelect();
    }
}
