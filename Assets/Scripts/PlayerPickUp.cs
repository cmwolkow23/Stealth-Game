using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public LayerMask pickupLayer;
    public Transform rayOrigin; // usually camera
    private InputSystem_Actions interactAction;
    [SerializeField]
    private Image Mask1;
    [SerializeField]
    private Image Mask2;
    [SerializeField] 
    private Image Mask3;
    [SerializeField]
    private TextMeshProUGUI collect;
    [SerializeField]
    private TextMeshProUGUI escape;
    [SerializeField]
    private GameObject escapeArea;

    private int masksCollected = 0;

    public static bool allMasksCollected = false;


    private void Start()
    {
        interactAction = new InputSystem_Actions();
        interactAction.Enable();
        allMasksCollected = false;
    }
    private void OnEnable()
    {
        allMasksCollected = false;
    }
    private void OnDisable()
    {
        interactAction.Disable();
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
            if(hit.transform.CompareTag("Mask"))
            {
                MaskPickup(hit);
            }
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
    private void MaskPickup(RaycastHit hit)
    {
        
            if (hit.transform.TryGetComponent<WhichMaskIsIt>(out WhichMaskIsIt mask))
            {
                string maskName = mask.maskName;
                Debug.Log("Picked up mask: " + maskName);
                masksCollected++;
                switch (maskName)
                {
                    case "Mask1":
                        Mask1.enabled = false;
                        //collect.text = "Masks Collected: " + masksCollected;
                        break;
                    case "Mask2":
                        Mask2.enabled = false;
                        //collect.text = "Masks Collected: " + masksCollected;
                        break;
                    case "Mask3":
                        Mask3.enabled = false;
                        //collect.text = "Masks Collected: " + masksCollected;
                        break;
                    default:
                        Debug.LogWarning("Unknown mask name: " + maskName);
                        break;
                }
                if (masksCollected >= 3)
                {
                    allMasksCollected = true;
                    collect.enabled = false;
                    escape.enabled = true;
                    escapeArea.SetActive(true);
                }

            }
        
        Destroy(hit.collider.gameObject);
    }
}



