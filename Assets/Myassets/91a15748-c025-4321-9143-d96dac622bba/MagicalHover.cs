using UnityEngine;

public class MagicalHover : MonoBehaviour
{
    // These variables will show up in the Unity Inspector so you can tweak them!
    public float spinSpeed = 100f;
    public float hoverSpeed = 2f;
    public float hoverHeight = 0.5f;

    private Vector3 startPosition;

    void Start()
    {
        // Remember exactly where the powerup spawned
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Spin the object around its Y axis
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);

        // 2. Make it bob up and down using a smooth Sine wave
        float newY = startPosition.y + (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);

        // Apply the new height while keeping the X and Z positions exactly the same
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}