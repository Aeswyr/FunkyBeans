using TMPro;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float speed;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject combatTextPrefab;
    [SerializeField] private PlayerCombatInterface combatInterface;
    [SerializeField] private GameObject combatPrefab;
    [SerializeField] private CombatEntity playerEntity;
    private bool freeMove = true;

    public List<Interactable> currInteractables = new List<Interactable>();

    void Awake() 
    {
        DontDestroyOnLoad(gameObject);
        GameHandler.Instance.activePlayers.Add(this);
    }

    void OnDestroy()
    {
        GameHandler.Instance.activePlayers.Remove(this);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (inputsFree)
        {
            rbody.velocity = speed * InputHandler.Instance.dir;

            if((InputHandler.Instance.interact.pressed) && (!IsInCombat()) && (currInteractables.Count > 0))
            {
                //Check which interactable to interact with
                PriorityQueue<Interactable> interactables = new PriorityQueue<Interactable>();
                foreach(Interactable interactable in currInteractables)
                {
                    interactables.Put(interactable, interactable.Priority);
                }

                //Do the interaction event 
                interactables.Peek().Interacted(this);
            }
        }

        if (InputHandler.Instance.action.pressed && inputsFree)
            ServerAttack(Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos));

        if (InputHandler.Instance.back.pressed && freeMove) {
            GameHandler.Instance.TogglePlayerMenu();
        }
    }

    private bool inputsFree => freeMove && !GameHandler.Instance.PlayerMenuState;

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
        
        GameHandler.Instance.DisablePlayerMenu();

        rbody.velocity = Vector2.zero;

        if (!isLocalPlayer)
            return;

        GameHandler.Instance.EnableCombatObjects();

        combatInterface.clientCombat = Instantiate(combatPrefab, Vector3.zero, Quaternion.identity).GetComponent<ClientCombatManager>();
        combatInterface.clientCombat.combatInterface = combatInterface;
        CombatUIController.Instance.SetKnownSkills(transform.GetComponent<CombatEntity>().KnownSkills);
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

        Destroy(combatInterface.clientCombat.gameObject);
    }

    public bool IsInCombat() 
    {
        return !freeMove;
    }

    [ClientRpc]
    public void SpawnCircle(Vector3 averageEntityPos, long id)
    {
        if (!isLocalPlayer)
            return;

        GameHandler.Instance.LocalPlayerSpawnCombatCircle(averageEntityPos, id);
    }

    public void AddInteractable(Interactable newInteractable)
    {
        if (currInteractables.Contains(newInteractable))
            Debug.LogError("Cannot add duplicate interactable: " + newInteractable.gameObject.name);

        currInteractables.Add(newInteractable);
    }

    public void RemoveInteractable(Interactable newInteractable)
    {
        if (!currInteractables.Contains(newInteractable))
            Debug.LogError("Cannot remove nonexistant interactable: " + newInteractable.gameObject.name);

        currInteractables.Remove(newInteractable);
    }
}
