using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCombatInterface : NetworkBehaviour
{
    [ClientRpc] public void NotifyTurnStart() {
        if (!isLocalPlayer)
            return;
    }

    [ClientRpc] public void NotifyTurnEnd() {
        if (!isLocalPlayer)
            return;

    }

    [ClientRpc] public void NotifyTurnOrder() {
        if (!isLocalPlayer)
            return;
    }

    [ClientRpc] public void NotifyResourceChange(long id, ResourceType type, int delta) {
        if (!isLocalPlayer)
            return;
    }

    [Command] public void TryUseSkill(SkillID skill, Vector3 position) {

    }

    [Command] public void TryMove(Vector3 position) {

    }
}
