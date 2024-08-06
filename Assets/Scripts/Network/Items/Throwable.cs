using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Throwable : MonoBehaviour, Interactable
{
    public void Interact()
    {
        Pickup();
    }

    public abstract ItemType Pickup();
    public abstract void Throw();
}
