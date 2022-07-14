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
    public CombatEntity CombatEntity => playerEntity;
    [SerializeField] private Inventory inventory;
    private bool freeMove = true;

    private long? currCombatID;

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
                Debug.Log("Interact!");
                interactables.Peek().Interacted(this);
            }
        }

        if (InputHandler.Instance.action.pressed && inputsFree)
            ServerAttack(Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos));

        if (InputHandler.Instance.back.pressed && freeMove) {
            GameHandler.Instance.TogglePlayerMenu();
            if (GameHandler.Instance.PlayerMenuState)
                inventory.RedrawInventory();
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
    [ClientRpc] public void EnterCombat() 
    {
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

    public void SetCombatID(long id)
    {
        currCombatID = id;
    }

    /**
    * removes this player from the combat state while disabling associated UI
    */
    [ClientRpc] public void ExitCombat(CombatReward reward) {
        int output = 0;
        foreach (var item in reward.items) {
            Debug.Log($"You just picked up a(n) {item.Name}");
            inventory.Insert(item);
            var tm = Instantiate(combatTextPrefab, transform.position + (Vector3)(output * Vector2.up), Quaternion.identity).GetComponent<TextMeshPro>();
            tm.text = item.Name;
            tm.color = Color.gray;
            output++;
        }

        {
            var tm = Instantiate(combatTextPrefab, transform.position + (Vector3)(output * Vector2.up), Quaternion.identity).GetComponent<TextMeshPro>();
            tm.text = $"+{reward.exp} EXP";
            tm.color = Color.yellow;
        }

        freeMove = true;

        if (!isLocalPlayer)
            return;

        GameHandler.Instance.DisableCombatObjects();

        Destroy(combatInterface.clientCombat.gameObject);

        currCombatID = null;
    }

    public bool IsInCombat() 
    {
        return !freeMove;
    }

    [ClientRpc]
    public void SpawnCircle(long id, Vector3 averageEntityPos, float maxDist)
    {
        if (!isLocalPlayer)
            return;

        if ((currCombatID == null) || (currCombatID != id))
        {
            GameHandler.Instance.LocalPlayerSpawnCombatCircle(averageEntityPos, id, maxDist);
        }
    }

    [Client]
    public void AddInteractable(Interactable newInteractable)
    {
        if (currInteractables.Contains(newInteractable))
            Debug.LogError("Cannot add duplicate interactable: " + newInteractable.gameObject.name);

        currInteractables.Add(newInteractable);
    }

    [Client]
    public void RemoveInteractable(Interactable newInteractable)
    {
        if (!currInteractables.Contains(newInteractable))
        {
            Debug.Log("Cannot remove nonexistant interactable: " + newInteractable.gameObject.name);
            return;
        }

        currInteractables.Remove(newInteractable);
    }
}
