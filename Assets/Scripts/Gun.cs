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
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject bulletPoint;
    private int cAmmo;
    private Vector3 originalPos;
    private Vector3 originalEuler;
    [SerializeField]
    private AudioClip shootSound;
    private AudioSource audioSource;
    [SerializeField]
    private int damage = 100;

    // Recoil settings
    [SerializeField] private float recoilDistance = 0.08f;
    [SerializeField] private float recoilReturnSpeed = 8f;
    [SerializeField] private float recoilKickAngle = 6f;
    [SerializeField] private float recoilRotReturnSpeed = 8f;

    // Runtime recoil state
    private Vector3 recoilOffset = Vector3.zero;
    private Vector3 recoilRotOffset = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        inputActions.Enable();
        audioSource = GetComponent<AudioSource>();
        FullAmmo.text = maxAmmo.ToString();
        CurrentAmmo.text = maxAmmo.ToString();
        cAmmo = maxAmmo;

        // cache original local transform for recoil math
        originalPos = transform.localPosition;
        originalEuler = transform.localEulerAngles;
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

        // Small bob while moving
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 bobOffset = Vector3.zero;
        if (moveInput != Vector2.zero)
        {
            bobOffset = new Vector3(0f, Mathf.Sin(Time.time * 10f) * 0.01f, 0f);
        }

        // Decay recoil offsets back to zero smoothly
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
        recoilRotOffset = Vector3.Lerp(recoilRotOffset, Vector3.zero, Time.deltaTime * recoilRotReturnSpeed);

        // Apply final local transform: original + bob + recoil
        transform.localPosition = originalPos + bobOffset + recoilOffset;
        transform.localEulerAngles = originalEuler + recoilRotOffset;
    }

    void Shoot()
    {
        Debug.Log("Bang!");
        Debug.DrawRay(shotPoint.transform.position, shotPoint.transform.forward, Color.red, 999f);
        audioSource.PlayOneShot(shootSound);
        if (Physics.Raycast(shotPoint.transform.position, shotPoint.transform.forward, out RaycastHit hitInfo))
        {
            Debug.Log("Hit: " + hitInfo.collider.name);
            if(hitInfo.transform.GetComponent<enemyHealthSystem>() != null)
            {
                hitInfo.transform.GetComponent<enemyHealthSystem>().OnHit(damage);
            }
        }

        // Instantiate bullet WITHOUT parenting it to the gun (so it won't move with the camera)
        Instantiate(bulletPrefab, bulletPoint.transform.position, bulletPoint.transform.rotation);

        // Apply positional recoil along local Z (kickback)
        // Using local Z axis: negative Z to move the gun back
        recoilOffset += new Vector3(0f, 0f, -recoilDistance);

        // Apply rotation recoil (kick the muzzle up and add a slight random yaw)
        float yawRandom = Random.Range(-recoilKickAngle * 0.2f, recoilKickAngle * 0.2f);
        recoilRotOffset += new Vector3(-recoilKickAngle, yawRandom, 0f);
    }  
}
