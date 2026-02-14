using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyX : MonoBehaviour
{
    public float speed = 3f;   // movement speed (can adjust in Inspector)

    private Rigidbody enemyRb;
    private GameObject playerGoal;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        // Find the Player Goal object in the scene
        playerGoal = GameObject.Find("Player Goal");
    }

    void Update()
    {
        // Only move if playerGoal exists (prevents NullReference error)
        if (playerGoal != null)
        {
            // Direction toward Player Goal
            Vector3 lookDirection = (playerGoal.transform.position - transform.position).normalized;

            // Move enemy toward goal
            enemyRb.AddForce(lookDirection * speed);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // Destroy enemy if it hits either goal
        if (other.gameObject.name == "Enemy Goal" || other.gameObject.name == "Player Goal")
        {
            Destroy(gameObject);
        }
    }
}
