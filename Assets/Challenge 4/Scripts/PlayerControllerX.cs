using System.Collections;
using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    private Rigidbody playerRb;
    private float speed = 280;
    private GameObject focalPoint;

    [Header("Powerups")]
    public bool hasKnockbackPowerup;
    public bool hasSmashPowerup;

    public GameObject powerupIndicator;
    public GameObject smashIndicator;

    public int powerUpDuration = 5;

    [Header("Knockback Settings")]
    [SerializeField] private float normalStrength = 18f;
    [SerializeField] private float powerupStrength = 55f;

    [Header("Smash Settings")]
    [SerializeField] private KeyCode smashKey = KeyCode.Space;
    [SerializeField] private float hopImpulse = 12f;
    [SerializeField] private float slamDownForce = 60f;
    [SerializeField] private float slamRadius = 7f;
    [SerializeField] private float slamMaxForce = 120f;

    [Header("Turbo Boost (Bonus)")]
    [SerializeField] private KeyCode turboKey = KeyCode.Space;
    [SerializeField] private float turboImpulse = 22f;
    [SerializeField] private float turboCooldown = 0.25f; // stops spamming
    [SerializeField] private ParticleSystem turboSmoke;   // Drag Smoke_Particle here

    private bool isSmashing = false;
    private float nextTurboTime = 0f;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");

        if (powerupIndicator != null) powerupIndicator.SetActive(false);
        if (smashIndicator != null) smashIndicator.SetActive(false);

        // Auto-find smoke if you forget to assign it (must be child of Focal Point)
        if (turboSmoke == null && focalPoint != null)
            turboSmoke = focalPoint.GetComponentInChildren<ParticleSystem>(true);

        if (turboSmoke != null)
        {
            turboSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            turboSmoke.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Movement
        float verticalInput = Input.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * verticalInput * speed * Time.deltaTime);

        // Keep indicators under player
        if (powerupIndicator != null)
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);

        if (smashIndicator != null)
            smashIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);

        // Smash activation
        if (hasSmashPowerup && !isSmashing && Input.GetKeyDown(smashKey))
        {
            StartCoroutine(DoSmash());
        }

        // Turbo boost
        if (Input.GetKeyDown(turboKey) && Time.time >= nextTurboTime)
        {
            TurboBoost();
            nextTurboTime = Time.time + turboCooldown;
        }
    }

    private void TurboBoost()
    {
        Vector3 boostDir = focalPoint.transform.forward;
        playerRb.AddForce(boostDir * turboImpulse, ForceMode.Impulse);

        // Smoke 
        if (turboSmoke != null)
        {
            turboSmoke.gameObject.SetActive(true);
            turboSmoke.Play();
            StartCoroutine(StopTurboSmokeAfter(0.35f));
        }
    }

    IEnumerator StopTurboSmokeAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (turboSmoke != null)
        {
            turboSmoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            turboSmoke.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Normal knockback powerup
        if (other.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            ActivateKnockbackPowerup();
        }

        // Smash powerup
        if (other.CompareTag("SmashPowerup"))
        {
            Destroy(other.gameObject);
            ActivateSmashPowerup();
        }
    }

    private void ActivateKnockbackPowerup()
    {
        hasKnockbackPowerup = true;
        if (powerupIndicator != null) powerupIndicator.SetActive(true);
        StartCoroutine(KnockbackCooldown());
    }

    private void ActivateSmashPowerup()
    {
        hasSmashPowerup = true;
        if (smashIndicator != null) smashIndicator.SetActive(true);
        StartCoroutine(SmashCooldown());
    }

    IEnumerator KnockbackCooldown()
    {
        yield return new WaitForSeconds(powerUpDuration);
        hasKnockbackPowerup = false;
        if (powerupIndicator != null) powerupIndicator.SetActive(false);
    }

    IEnumerator SmashCooldown()
    {
        yield return new WaitForSeconds(powerUpDuration);
        hasSmashPowerup = false;
        if (smashIndicator != null) smashIndicator.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRb = other.gameObject.GetComponent<Rigidbody>();
            EnemyX enemyScript = other.gameObject.GetComponent<EnemyX>();

            Vector3 away = (other.transform.position - transform.position).normalized;

            float strength = hasKnockbackPowerup ? powerupStrength : normalStrength;

            // stop its current movement 
            enemyRb.linearVelocity = Vector3.zero;

            // instant knockback
            enemyRb.AddForce(away * strength, ForceMode.VelocityChange);

            // small stun
            if (enemyScript != null) enemyScript.Stun(0.25f);
        }
    }

    IEnumerator DoSmash()
    {
        isSmashing = true;

        playerRb.linearVelocity = new Vector3( 0f,0f,0f);
        // Hop up
        playerRb.AddForce(Vector3.up * hopImpulse, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        // wait until we stop going up
        while (playerRb.linearVelocity.y > 0.1f)
            yield return null;

        // Slam down
        playerRb.AddForce(Vector3.down * slamDownForce, ForceMode.Impulse);

        // wait until grounded
        while (!IsGrounded())
            yield return null;

        SlamBlast();
        isSmashing = false;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void SlamBlast()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
                if (enemyRb == null) continue;

                Vector3 direction = (hit.transform.position - transform.position);
                float distance = direction.magnitude;

                float forceMultiplier = 1f - (distance / slamRadius);
                forceMultiplier = Mathf.Clamp01(forceMultiplier);

                Vector3 force = direction.normalized * slamMaxForce * forceMultiplier + Vector3.up * 6f;
                enemyRb.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}