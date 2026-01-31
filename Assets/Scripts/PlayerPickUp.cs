using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public LayerMask pickupLayers;
    public Transform cameraTransform;

    private InputSystem_Actions inputActions;

    public interface IPickup
    {
        void OnPickup(GameObject player);
    }

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interact.performed += OnPickupPerformed;
    }

    void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnPickupPerformed;
        inputActions.Disable();
    }

    private void OnPickupPerformed(InputAction.CallbackContext context)
    {
        TryPickup();
    }

    void TryPickup()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("cameraTransform is not assigned!");
            return;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * pickupRange, Color.green, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayers))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");
            IPickup pickup = hit.collider.GetComponent<IPickup>();
            if (pickup != null)
            {
                pickup.OnPickup(gameObject);
            }
            else
            {
                Debug.Log("Hit object does not implement IPickup.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
        }
    }
}

