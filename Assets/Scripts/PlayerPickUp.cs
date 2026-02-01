using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public LayerMask pickupLayer;
    public Transform rayOrigin; // usually camera
    private InputSystem_Actions interactAction;

    private void Start()
    {
        interactAction = new InputSystem_Actions();
        interactAction.Enable();
    }

    private void Update()
    {
        if(interactAction.Player.Interact.WasPressedThisFrame())
        {
            OnInteract();
        }
    }
    public void OnInteract()
    {
        //if (!context.performed)
           // return;
        Debug.Log("Interact action performed");
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayer))
        {
            Destroy(hit.collider.gameObject);
        }
        
    }

    void OnDrawGizmos()
    {
        if (rayOrigin == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            rayOrigin.position,
            rayOrigin.position + rayOrigin.forward * pickupRange
        );
    }
}



