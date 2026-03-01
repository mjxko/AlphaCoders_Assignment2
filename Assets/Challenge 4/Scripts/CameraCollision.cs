using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [SerializeField] Transform target;          // Ball
    [SerializeField] float minDistance = 2f;    // closest camera can get
    [SerializeField] float maxDistance = 10f;   // normal distance
    [SerializeField] float sphereRadius = 0.3f; // camera "size"
    [SerializeField] LayerMask collisionMask;   // set to Walls/Ground layer

    Vector3 dir;

    void Start()
    {
        dir = (transform.localPosition).normalized;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 pivot = transform.parent.position; // Focal Point position
        Vector3 desiredPos = pivot + dir * maxDistance;

        if (Physics.SphereCast(pivot, sphereRadius, (desiredPos - pivot).normalized,
            out RaycastHit hit, maxDistance, collisionMask))
        {
            float d = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            transform.position = pivot + (desiredPos - pivot).normalized * d;
        }
        else
        {
            transform.position = desiredPos;
        }

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}