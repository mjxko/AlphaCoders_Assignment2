using UnityEngine;

public class RotateCameraX : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] Transform player;

    void LateUpdate()
    {
        if (!player) return;

        // Follow player position exactly
        transform.position = player.position;

        // Rotate pivot around Y
        float h = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, h * rotateSpeed * Time.deltaTime);

        // Make camera always look at player
        Camera.main.transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}