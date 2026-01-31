using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private GameObject shotPoint;
    private InputSystem_Actions inputActions;
    [SerializeField]
    private float shotCooldown;
    [SerializeField] 
    private float reloadTime;
    [SerializeField]
    private TMP_Text CurrentAmmo;
    [SerializeField] 
    private TMP_Text FullAmmo;
    [SerializeField]
    private int maxAmmo = 12;
    private int cAmmo;
    private Vector3 originalPos;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        inputActions.Enable();
        FullAmmo.text = maxAmmo.ToString();
        CurrentAmmo.text = maxAmmo.ToString();
        cAmmo = maxAmmo;
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        shotCooldown -= Time.deltaTime;
        reloadTime -= Time.deltaTime;
        reloadTime = Mathf.Clamp(reloadTime, 0, Mathf.Infinity);
        shotCooldown = Mathf.Clamp(shotCooldown, 0, Mathf.Infinity);
        if (inputActions.Player.Attack.IsPressed() && shotCooldown <= 0 && cAmmo > 0)
        {
            Shoot();
            shotCooldown = 0.5f;
            cAmmo--;
            CurrentAmmo.text = cAmmo.ToString();
        }
        if(inputActions.Player.Reload.WasPressedThisFrame() && reloadTime <= 0)
        {
            reloadTime = 2f;
            cAmmo = maxAmmo;
            CurrentAmmo.text = cAmmo.ToString();
        }
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput != Vector2.zero)
        {
            transform.localPosition += new Vector3(0f, Mathf.Sin(Time.time * 10f) * 0.001f, 0f);
        }
        else if (transform.position != originalPos)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -0.328f, transform.localPosition.z);
        }
    }

    void Shoot()
    {
        Debug.Log("Bang!");
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward, Color.red, 999f);
        if (Physics.Raycast(shotPoint.transform.position, shotPoint.transform.forward, out RaycastHit hitInfo))
        {
            Debug.Log("Hit: " + hitInfo.collider.name);
        }
    }  
}
