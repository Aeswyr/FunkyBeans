using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameHandler : NetworkSingleton<GameHandler>
{
    public ItemHelper ItemHelper;
    [SerializeField] private List<GameObject> thingsToEnableForCombat;
    [SerializeField] private List<GameObject> playerMenuObjects;
    private bool playerMenuState = false;

    [Header("Gameplay grids")]
    [SerializeField] private Grid m_currentLevel;
    public Tilemap floorGrid {
        get;
        private set;
    }
    public Tilemap wallGrid {
        get;
        private set;
    }
    public Grid currentLevel {
        get {return m_currentLevel;}
    }

    public List<PlayerController> activePlayers { get; set; } = new List<PlayerController>();

    public Dictionary<long, ServerCombatManager> activeCombats = new Dictionary<long, ServerCombatManager>();

    [Header("Prefabs")]
    [SerializeField] private GameObject combatManagerPrefab;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject combatCirclePrefab;
    private ContactFilter2D filter = new ContactFilter2D();

    // Start is called before the first frame update
    void Start() {
        floorGrid = m_currentLevel.transform.Find("Collision").GetComponent<Tilemap>();
        wallGrid = m_currentLevel.transform.Find("Walls").GetComponent<Tilemap>();
        filter.SetLayerMask(LayerMask.GetMask(new []{"Hurtbox"}));
        filter.useTriggers = true;
    }

    List<GameObject> textList = new List<GameObject>();
    public void DrawText(Vector3 pos, string text) {
        GameObject txt = Instantiate(textPrefab, pos, Quaternion.identity);
        txt.GetComponent<TextMeshPro>().text = text;
        textList.Add(txt);
    }

    public void ClearText() {
        foreach (var txt in textList)
            Destroy(txt);
        textList.Clear();
    }
    
    [Server] 
    public void EnterCombat(Vector3 position) 
    { 
        Debug.Log("start battle!");

        var results = new List<RaycastHit2D>();


        Physics2D.CircleCast(position, 5, Vector2.right, filter, results, 0);

        List<CombatEntity> entities = new List<CombatEntity>();

        List<PlayerController> playersEnteringCombat = new List<PlayerController>();

        long id = 0;
        foreach (var hit in results) {
            if (hit.collider.transform.parent.TryGetComponent(out PlayerController player)) {
                if (player.IsInCombat())
                    continue;
                player.EnterCombat();
                playersEnteringCombat.Add(player);
            }


            id += hit.collider.transform.parent.GetComponent<CombatID>().CID; 
            entities.Add(hit.collider.transform.parent.GetComponent<CombatEntity>());
        }
        
        if (activeCombats.ContainsKey(id)) {
            Debug.Log($"Combat manager with ID {id} already exists");
            return;
        }
        
        Debug.Log($"Creating combat manager with ID {id}");
        GameObject combatManager = Instantiate(combatManagerPrefab, Vector3.zero, Quaternion.identity);
        ServerCombatManager currentCombat = combatManager.GetComponent<ServerCombatManager>();
        currentCombat.ID = id;

        foreach (var hit in results) {
            GameObject hitEntity = hit.collider.transform.parent.gameObject;
            if (hitEntity.TryGetComponent(out PlayerCombatInterface player))
                player.serverCombatManager = currentCombat;
        }

        Vector3 averageEntityPos = Vector3.zero;
        foreach (CombatEntity entity in entities)
        {
            averageEntityPos += entity.transform.position;
        }

        averageEntityPos /= entities.Count;

        //Spawn circles for players not in the new combat being created
        foreach (PlayerController activePlayer in activePlayers)
        {
            if (playersEnteringCombat.Contains(activePlayer))
            {
                activePlayer.SetCombatID(id);
                continue;
            }

            activePlayer.SpawnCircle(id, averageEntityPos, Utils.CombatUtil.GetMaxDisctanceOfEntities(entities, averageEntityPos));
        }

        currentCombat.SetCombatEntities(entities);
        activeCombats[id] = currentCombat;
    }

    [Command(requiresAuthority = false)] public void ExitCombat(long id, List<CombatEntity> entities) 
    {

        CombatReward reward = new CombatReward();
        reward.items = new []{ItemHelper.GenerateItem(), ItemHelper.GenerateItem(), ItemHelper.GenerateItem()};
        reward.exp = 5;

        foreach (var entity in entities)
            if (entity.transform.TryGetComponent(out PlayerController player))
                player.ExitCombat(reward);

        string output = "\nDictionary:";
        foreach (var val in activeCombats)
            output += $"\n{val.Key.ToString()}, {val.Value.ToString()}";
        Debug.Log($"Searching for combat manager with ID {id} {output}");
        if (activeCombats.ContainsKey(id)) {
            Debug.Log($"Trying to destroy combat manager with ID {id}");
            Destroy(activeCombats[id].gameObject);
            activeCombats.Remove(id);
        }
    }
    public void EnableCombatObjects()
    {
        foreach (GameObject obj in thingsToEnableForCombat)
            obj.SetActive(true);
    }

    public void DisableCombatObjects()
    {
        foreach (GameObject obj in thingsToEnableForCombat)
            obj.SetActive(false);
    }

    public void DisablePlayerMenu() {
        if (!playerMenuState)
            return;
        foreach (var obj in playerMenuObjects)
            obj.SetActive(false);
        MenuUIController.Instance.DisableMenus();
        playerMenuState = false;
    }

    public void EnablePlayerMenu() {
        if (playerMenuState)
            return;
        foreach (var obj in playerMenuObjects)
            obj.SetActive(true);
        MenuUIController.Instance.EnableMenus();
        playerMenuState = true;
    }

    public bool PlayerMenuState => playerMenuState;

    public void TogglePlayerMenu() {
        if (playerMenuState) {
            DisablePlayerMenu();
        } else {
            EnablePlayerMenu();
        }
    }

    private Dictionary<long, GameObject> combatCircles = new Dictionary<long, GameObject>();

    [Client]
    public void LocalPlayerSpawnCombatCircle(Vector3 pos, long newID, float maxDist)
    { 
        GameObject combatCircleObj = Instantiate(combatCirclePrefab);
        combatCircleObj.transform.position = pos;
        combatCircleObj.transform.localScale = Vector3.one * (maxDist + 4);

        CombatCircle combatCircle = combatCircleObj.GetComponent<CombatCircle>();
        combatCircle.SetCombatID(newID);

        combatCircles.Add(newID, combatCircleObj);
    }

    [ClientRpc]
    public void DestroyCombatCircleWithID(long newId)
    {
        if (combatCircles.TryGetValue(newId, out GameObject circleObj))
        {
            Destroy(circleObj);
            combatCircles.Remove(newId);
        }
    }

    [Client]
    public void AddPlayerToExistingCombat(long playerId, long combatId)
    {
        AddPlayerToExistingCombat_Server(playerId, combatId);
    }

    [Command(requiresAuthority = false)]
    public void AddPlayerToExistingCombat_Server(long playerId, long combatId)
    {
        ServerCombatManager combatManager = activeCombats[combatId];

        foreach (PlayerController activePlayer in activePlayers)
        {
            if(activePlayer.transform.GetComponent<CombatID>().CID == playerId)
            {
                Debug.Log("enter combat!");
                activePlayer.GetComponent<PlayerCombatInterface>().serverCombatManager = combatManager;

                activePlayer.EnterCombat();
                combatManager.AddEntityToCombat(activePlayer.CombatEntity);
                return;
            }
        }
    }
}
