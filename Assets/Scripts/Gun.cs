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
    [SerializeField]
    private AudioClip reloadSound;
    [SerializeField]
    private AudioClip emptySound;
    [SerializeField]
    private Animator gunAnimator;

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
            gunAnimator.SetTrigger("Shoot");
            shotCooldown = 0.5f;
            cAmmo--;
            CurrentAmmo.text = cAmmo.ToString();
        }
        else if(inputActions.Player.Attack.WasPressedThisFrame()&& shotCooldown <= 0 && cAmmo <= 0)
        {
            audioSource.PlayOneShot(emptySound);
        }
        if (inputActions.Player.Reload.WasPressedThisFrame() && reloadTime <= 0)
        {
            audioSource.PlayOneShot(reloadSound);
            reloadTime = 2f;
            cAmmo = maxAmmo;
            CurrentAmmo.text = cAmmo.ToString();
            gunAnimator.SetTrigger("Reload");
            
        }

        // Small bob while moving
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            gunAnimator.SetBool("Moving", true);
        }
        else
        {
            if (gunAnimator.GetBool("Moving") == true)
            {
                gunAnimator.SetBool("Moving", false);
            }
        }

        
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

    }  
}
