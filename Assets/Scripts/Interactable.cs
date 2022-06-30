using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent interactEvent;
    [SerializeField] private float priority;
    public float Priority => priority;

    private PlayerController player;
    public PlayerController Player => player;

    public void Interacted(PlayerController newPlayer)
    {
        player = newPlayer;

        interactEvent.Invoke();
    }
}