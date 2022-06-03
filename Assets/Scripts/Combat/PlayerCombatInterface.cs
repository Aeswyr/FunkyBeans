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
        clientCombat.SetEntityTile(pos, entering);
    }
    [ClientRpc] public void NotifyTurnStart() {
        if (!isLocalPlayer)
            return;
        clientCombat.isTurn = true;
    }

    [ClientRpc] public void NotifyTurnEnd() {
        if (!isLocalPlayer)
            return;
        clientCombat.isTurn = false;
    }

    [ClientRpc] public void NotifyTurnOrder() {
        if (!isLocalPlayer)
            return;
    }

    [ClientRpc] public void NotifyResourceChange(long id, ResourceType type, int delta) {
        if (!isLocalPlayer)
            return;

        foreach (var entity in FindObjectsOfType<CombatID>())
            if (entity.CID == id)
                entity.transform.GetComponent<CombatEntity>().UpdateResource(type, delta);
    }

    [Command] public void TryUseSkill(SkillID skill, Vector3 position) {
        serverCombatManager.TryUseSkill(skill, position);
    }

    [Command] public void TryMove(Vector3 position) {
        serverCombatManager.TryMovePlayer(position);
    }

    [Command] public void TryDefend() {
        serverCombatManager.TryUseDefend();
    }
}
