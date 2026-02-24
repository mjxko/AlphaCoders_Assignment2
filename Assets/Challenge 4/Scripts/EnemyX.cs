using UnityEngine;

public class EnemyX : MonoBehaviour
{
    public float speed = 3f;  
    [SerializeField] private float maxVelocity = 12f;

    private Rigidbody enemyRb;
    private Transform playerGoal;

    void Awake()
    {
        enemyRb = GetComponent<Rigidbody>();
    }


    public void InitSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Start()
    {
        GameObject goalObj = GameObject.Find("Player Goal");
        if (goalObj != null) playerGoal = goalObj.transform;
    }

    void FixedUpdate()
    {
        if (playerGoal == null || enemyRb == null) return;

        Vector3 dir = (playerGoal.position - transform.position).normalized;

        // safe movement
        enemyRb.AddForce(dir * speed, ForceMode.Force);

        // prevent big acceleration
        if (enemyRb.linearVelocity.magnitude > maxVelocity)
        {
            enemyRb.linearVelocity = enemyRb.linearVelocity.normalized * maxVelocity;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Enemy Goal" || other.gameObject.name == "Player Goal")
        {
            Destroy(gameObject);
        }
    }
}