using UnityEngine;
using static PlayerPickup;

public class PickupItem : MonoBehaviour, IPickup
{
    [Header("Pickup Info")]
    public string itemName;

    public void OnPickup(GameObject player)
    {
        Debug.Log($"Picked up: {itemName}");

        Destroy(gameObject);
    }
}

