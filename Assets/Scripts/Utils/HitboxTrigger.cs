using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitboxTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent action;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(gameObject.name + "Collided with " + other.name + " on layer " + other.gameObject.layer);
        action.Invoke();
    }
}
