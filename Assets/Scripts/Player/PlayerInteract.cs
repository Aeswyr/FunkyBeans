using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("triggerEnter wow");
        playerController.AddInteractable(other.GetComponent<Interactable>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("triggerExit y7eah");
        playerController.RemoveInteractable(other.GetComponent<Interactable>());
    }
}
