using UnityEngine;

public class EnemyX : MonoBehaviour
{
    public float speed = 3f;

    private Rigidbody enemyRb;
    private Transform playerGoal;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        GameObject goalObj = GameObject.Find("Player Goal");
        if (goalObj != null) playerGoal = goalObj.transform;
    }

    void FixedUpdate()
    {
        if (playerGoal == null) return;

        Vector3 dir = (playerGoal.position - transform.position).normalized;

        //Smooth consistent physics movement
        enemyRb.AddForce(dir * speed, ForceMode.Force);

        //clamp max velocity so it never becomes insane
        float maxSpeed = 8f;
        if (enemyRb.linearVelocity.magnitude > maxSpeed)
            enemyRb.linearVelocity = enemyRb.linearVelocity.normalized * maxSpeed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Enemy Goal" || other.gameObject.name == "Player Goal")
            Destroy(gameObject);
    }
}