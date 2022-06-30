using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        playerController.AddInteractable(other.GetComponent<Interactable>());
    }

    private void OnTriggerExit(Collider other)
    {
        playerController.RemoveInteractable(other.GetComponent<Interactable>());
    }
}
