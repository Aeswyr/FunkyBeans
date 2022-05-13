using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{

    private PlayerController source;
    public void OnEnd() {
        Destroy(gameObject);
    }

    public void SetSource(PlayerController source) {
        this.source = source;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        source.StartBattle();
    }
}
