using UnityEngine;

public class boolet : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 2f;
    [SerializeField]
    private float speed = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;

        // Move forward along the bullet's forward (so it follows the rotation you gave at spawn)
        transform.position += transform.forward * speed * Time.deltaTime;

        if(lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
