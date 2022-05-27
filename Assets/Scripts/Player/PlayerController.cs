using TMPro;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float speed;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject combatTextPrefab;
    private bool freeMove = true;

    void Awake() 
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

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
        if ((freeMove == false) || (isLocalPlayer == false))
            return;
        freeMove = false;
        
        rbody.velocity = Vector2.zero;
        GameHandler.Instance.EnableCombatObjects();
        
        GameHandler.Instance.EnterCombat(transform.position);
    }

    public void EndBattle(CombatReward reward, long id) {
        var tm = Instantiate(combatTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = $"+{reward.exp} EXP";
        tm.color = Color.yellow;

        freeMove = true;

        GameHandler.Instance.ExitCombat(id);
    }
}
