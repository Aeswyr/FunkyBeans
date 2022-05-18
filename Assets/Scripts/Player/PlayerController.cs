using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float speed;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject combatTextPrefab;
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
            rbody.velocity = speed * InputHandler.Instance.dir;

        if (InputHandler.Instance.action.pressed)
            if (freeMove) {
                GameObject attack = Instantiate(attackPrefab, transform);
                attack.GetComponent<AttackController>().SetSource(this);
                attack.transform.rotation = Utils.Rotate(Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos) - transform.position);
            }
            else
            {
                //EndBattle();
            }
    }

    public void StartBattle() {
        GameHandler.Instance.EnableCombatObjects();

        var results = new List<RaycastHit2D>();
        Physics2D.CircleCast(transform.position, maxMove, Vector2.right, filter, results, 0);

        currentCombat = GameHandler.Instance.CreateCombatManager();

        freeMove = false;
        rbody.velocity = Vector2.zero;

        //Make list of all entities that will be in combat
        List<CombatEntity> entities = new List<CombatEntity>();

        foreach (var hit in results) {
            GameObject hitEntity = hit.collider.transform.parent.gameObject;

            Utils.GridUtil.SnapToLevelGrid(hitEntity, currentCombat);
            currentCombat.EntityEnterTile(hitEntity);
            
            entities.Add(hitEntity.GetComponentInChildren<CombatEntity>());
        }
        foreach(var obj in entities)
            Debug.Log(entities.ToString());

        currentCombat.SetCombatEntities(entities);

        //currentCombat.DrawSelect(gameObject, maxMove);
    }

    public void EndBattle(CombatReward reward) {
        var tm = Instantiate(combatTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = $"+{reward.exp} EXP";
        tm.color = Color.yellow;

        freeMove = true;
        currentCombat.ClearMove();
        currentCombat.ClearSelect();

        Destroy(currentCombat.gameObject);
    }
}
