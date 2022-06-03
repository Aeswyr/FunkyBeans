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
    [SerializeField] private PlayerCombatInterface combatInterface;
    [SerializeField] private GameObject combatPrefab;
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
            if (freeMove)
                ServerAttack(Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos));

        if (InputHandler.Instance.back.pressed)
            GameHandler.Instance.ExitCombat(0);

    }

    [Command] public void ServerAttack(Vector3 mousePos) {
        GameObject attack = Instantiate(attackPrefab, transform);
        attack.transform.rotation = Utils.Rotate(mousePos - transform.position);
        attack.GetComponent<AttackController>().SetSource(this);
        attack.GetComponent<SpriteRenderer>().enabled = false;
        ClientAttack(mousePos);
    }

    [ClientRpc] public void ClientAttack(Vector3 mousePos) {
        GameObject attack = Instantiate(attackPrefab, transform);
        attack.transform.rotation = Utils.Rotate(mousePos - transform.position);
        attack.GetComponent<AttackController>().enabled = false;
        attack.GetComponent<Collider2D>().enabled = false;
    }

    [Server] public void StartBattle() {
        if (freeMove)
            GameHandler.Instance.EnterCombat(transform.position);
    }

    [Server] public void EndBattle(CombatReward reward, long id) {

        var tm = Instantiate(combatTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = $"+{reward.exp} EXP";
        tm.color = Color.yellow;

        freeMove = true;
    }

    /**
    * places this player into the combat state and enables associated UI
    */
    [ClientRpc] public void EnterCombat () {
        freeMove = false;
        
        rbody.velocity = Vector2.zero;

        if (!isLocalPlayer)
            return;

        GameHandler.Instance.EnableCombatObjects();

        combatInterface.clientCombat = Instantiate(combatPrefab, Vector3.zero, Quaternion.identity).GetComponent<ClientCombatManager>();
        combatInterface.clientCombat.combatInterface = combatInterface;
    }

    /**
    * removes this player from the combat state while disabling associated UI
    */
    [ClientRpc] public void ExitCombat(CombatReward reward) {
        var tm = Instantiate(combatTextPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = $"+{reward.exp} EXP";
        tm.color = Color.yellow;

        freeMove = true;

        if (!isLocalPlayer)
            return;

        GameHandler.Instance.DisableCombatObjects();

        Destroy(combatInterface.clientCombat);
    }

    public bool IsInCombat() {
        return !freeMove;
    }
}
