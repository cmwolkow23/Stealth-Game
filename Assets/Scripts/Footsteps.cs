using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField]
    private AudioSource footstepAudioSource;
    [SerializeField]
    private AudioClip footstepClip;
    [SerializeField]
    private AudioClip ventFootstepClip;
    private InputSystem_Actions inputActions;
    private bool vent = false;
    private bool isPlaying = false;
                
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (footstepAudioSource == null)
            return;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        AudioClip desiredClip = vent ? ventFootstepClip : footstepClip;

        if (moveInput != Vector2.zero)
        {
            if (desiredClip == null)
                return;

            // Only change clip / restart playback when needed
            if (footstepAudioSource.clip != desiredClip)
            {
                footstepAudioSource.clip = desiredClip;
                footstepAudioSource.loop = true;
                footstepAudioSource.Play();
                isPlaying = true;
            }
            else if (!footstepAudioSource.isPlaying)
            {
                // In case it stopped for some reason, resume
                footstepAudioSource.Play();
                isPlaying = true;
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
            isPlaying = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Vent"))
        {
            vent = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Vent"))
        {
            vent = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Vent"))
        {
            vent = false;
        }
    }
}
