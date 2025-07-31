using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    IInteractable interactableItem = null;
    [SerializeField] GameObject interactionSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (interactionSprite)
            interactionSprite.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable item = collision.GetComponent<IInteractable>();
        if(item != null && item.IsInteractable())
        {
            interactionSprite.SetActive(true);
            interactableItem = item;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable item = collision.GetComponent<IInteractable>();
        if (item == interactableItem)
        {
            ResetInteractor();
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if(interactableItem != null && interactableItem.IsInteractable())
        {
            interactableItem.Interact();
            ResetInteractor();
        }
    }

    private void ResetInteractor()
    {
        if(interactionSprite)
            interactionSprite.SetActive(false);
        interactableItem = null;
    }
}
