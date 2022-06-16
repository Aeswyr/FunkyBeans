using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;

public class PlayerCombatInterface : NetworkBehaviour
{
    public ClientCombatManager clientCombat {get; set;}

    public ServerCombatManager serverCombatManager { get; set; }

    [ClientRpc] public void NotifyMovement(Vector3Int pos, bool entering) {
        if (!isLocalPlayer)
            return;
        if (clientCombat == null)
            StartCoroutine(MoveAfterClientInit(pos, entering));
        else
            clientCombat.SetEntityTile(pos, entering);
    }

    [ClientRpc] public void NotifyMovePlayer(Vector3 pos) {
        if (!isLocalPlayer)
            return;
        transform.position = pos;
    }

    private IEnumerator MoveAfterClientInit(Vector3Int pos, bool entering) {
        yield return new WaitUntil(() => clientCombat != null);
        clientCombat.SetEntityTile(pos, entering);
    }

    [ClientRpc] public void NotifyTurnStart(int actions) {
        if (!isLocalPlayer)
            return;
        clientCombat.isTurn = true;
        clientCombat.actionsLeft = actions;
        clientCombat.maxActions = actions;
        CombatUIController.Instance.SetActionUI(clientCombat.actionsLeft, clientCombat.maxActions);
    }

    [ClientRpc] public void NotifyTurnEnd() {
        if (!isLocalPlayer)
            return;
        clientCombat.isTurn = false;
        clientCombat.ClearMove();
        clientCombat.ClearSelect();
        clientCombat.ClearHighlight();
    }

    [ClientRpc]
    public void NotifyTurnOrder(List<long> entityIDs, List<float> positions)
    {
        if (!isLocalPlayer)
            return;

        List<CombatEntity> combatEntities = new List<CombatEntity>();

        for (int i = 0; i < entityIDs.Count; i++)
        {
            foreach (var entity in FindObjectsOfType<CombatID>())
            {
                if (entity.CID == entityIDs[i])
                {
                    combatEntities.Add(entity.transform.GetComponent<CombatEntity>());
                    break;
                }
            }
        }

        CombatUIController.Instance.UpdateTurnIndicatorUI(combatEntities, positions);
    }

    [ClientRpc] public void NotifyResourceChange(long id, ResourceType type, int delta) {
        if (!isLocalPlayer)
            return;
        if (type == ResourceType.ACTIONS) {
            clientCombat.actionsLeft = clientCombat.actionsLeft - delta;
            clientCombat.DrawCombatMovement(true);
            CombatUIController.Instance.SetActionUI(clientCombat.actionsLeft, clientCombat.maxActions);
        }

        foreach (var entity in FindObjectsOfType<CombatID>())
            if (entity.CID == id)
                entity.transform.GetComponent<CombatEntity>().UpdateResource(type, delta);
    }

    [Command] public void TryUseSkill(SkillID skill, Vector3 position) {
        serverCombatManager.TryUseSkill(skill, position, GetComponent<CombatEntity>());
    }

    [Command] public void TryMove(Vector3 position) {
        serverCombatManager.TryMovePlayer(position, GetComponent<CombatEntity>());
    }

    [Command] public void TryDefend() {
        serverCombatManager.TryUseDefend(GetComponent<CombatEntity>());
    }

    [Command] public void TryFlee() {
        serverCombatManager.EndCombat();
    }
}
