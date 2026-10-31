using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void Interact(GameObject player)
    {
        
    }

    public virtual void ResetInteractable()
    {
        gameObject.SetActive(true);
    }
}