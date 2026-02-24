using UnityEngine;

public class RotateCameraX : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 50f;
    public GameObject player;

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalInput * rotateSpeed * Time.deltaTime);

        if (player != null)
            transform.position = player.transform.position;
    }
}