using System.Collections;
using UnityEngine;

public class PlayerControllerX : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 280f;
    private Rigidbody playerRb;
    private Transform focalPoint;
    private float verticalInput;

    [Header("Powerups")]
    public bool hasKnockbackPowerup;
    public bool hasSmashPowerup;
    [SerializeField] private int powerUpDuration = 5;

    public GameObject powerupIndicator;
    public GameObject smashIndicator;

    [Header("Knockback Settings")]
    [SerializeField] private float normalStrength = 18f;
    [SerializeField] private float powerupStrength = 55f;

    [Header("Smash Settings")]
    [SerializeField] private KeyCode smashKey = KeyCode.E;
    [SerializeField] private float hopImpulse = 8f;       // lowered (was 12)
    [SerializeField] private float slamDownImpulse = 18f; // lowered (was 60)
    [SerializeField] private float slamRadius = 7f;
    [SerializeField] private float slamMaxForce = 60f;    // lowered (was 120)
    private bool isSmashing = false;

    [Header("Turbo Boost")]
    [SerializeField] private KeyCode turboKey = KeyCode.Space;
    [SerializeField] private float turboImpulse = 12f;    // lowered (was 22)
    [SerializeField] private float turboCooldown = 0.35f;
    [SerializeField] private ParticleSystem turboSmoke;
    private float nextTurboTime = 0f;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();

        GameObject fp = GameObject.Find("Focal Point");
        if (fp != null) focalPoint = fp.transform;

        if (powerupIndicator != null) powerupIndicator.SetActive(false);
        if (smashIndicator != null) smashIndicator.SetActive(false);

        if (turboSmoke == null && fp != null)
            turboSmoke = fp.GetComponentInChildren<ParticleSystem>(true);

        if (turboSmoke != null)
        {
            turboSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            turboSmoke.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");

        // visuals only
        if (powerupIndicator != null)
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);
        if (smashIndicator != null)
            smashIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);

        // Smash (only when grounded to prevent flying)
        if (hasSmashPowerup && !isSmashing && Input.GetKeyDown(smashKey) && IsGrounded())
            StartCoroutine(DoSmash());

        // Turbo
        if (Input.GetKeyDown(turboKey) && Time.time >= nextTurboTime)
        {
            TurboBoost();
            nextTurboTime = Time.time + turboCooldown;
        }
    }

    void FixedUpdate()
    {
        if (focalPoint == null) return;

        Vector3 forward = focalPoint.forward;
        forward.y = 0f;
        forward.Normalize();

        playerRb.AddForce(forward * verticalInput * speed, ForceMode.Force);
    }

    private void TurboBoost()
    {
        if (focalPoint == null) return;

        Vector3 dir = focalPoint.forward;
        dir.y = 0f;
        dir.Normalize();

        playerRb.AddForce(dir * turboImpulse, ForceMode.Impulse);

        if (turboSmoke != null)
        {
            turboSmoke.gameObject.SetActive(true);
            turboSmoke.Play();
            StartCoroutine(StopTurboSmokeAfter(0.25f));
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

    private bool IsGrounded()
    {
        // slightly longer ray so it works on slopes
        return Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }

    IEnumerator DoSmash()
    {
        isSmashing = true;

        // Hop up
        playerRb.AddForce(Vector3.up * hopImpulse, ForceMode.Impulse);

        // small delay so hop happens
        yield return new WaitForSeconds(0.12f);

        // Slam down (impulse, not crazy)
        playerRb.AddForce(Vector3.down * slamDownImpulse, ForceMode.Impulse);

        // wait until grounded again
        while (!IsGrounded())
            yield return null;

        SlamBlast();
        isSmashing = false;
    }

    private void SlamBlast()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
            if (enemyRb == null) continue;

            Vector3 direction = (hit.transform.position - transform.position);
            float distance = direction.magnitude;

            float mult = 1f - (distance / slamRadius);
            mult = Mathf.Clamp01(mult);

            Vector3 force = direction.normalized * slamMaxForce * mult + Vector3.up * 3f;
            enemyRb.AddForce(force, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            hasKnockbackPowerup = true;
            if (powerupIndicator != null) powerupIndicator.SetActive(true);
            StartCoroutine(KnockbackCooldown());
        }

        if (other.CompareTag("SmashPowerup"))
        {
            Destroy(other.gameObject);
            hasSmashPowerup = true;
            if (smashIndicator != null) smashIndicator.SetActive(true);
            StartCoroutine(SmashCooldown());
        }
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
        if (!other.gameObject.CompareTag("Enemy")) return;

        Rigidbody enemyRb = other.gameObject.GetComponent<Rigidbody>();
        EnemyX enemyScript = other.gameObject.GetComponent<EnemyX>();
        if (enemyRb == null) return;

        Vector3 away = (other.transform.position - transform.position).normalized;
        float strength = hasKnockbackPowerup ? powerupStrength : normalStrength;

        enemyRb.linearVelocity = Vector3.zero;
        enemyRb.AddForce(away * strength, ForceMode.VelocityChange);

        if (enemyScript != null) enemyScript.Stun(0.25f);
    }
}