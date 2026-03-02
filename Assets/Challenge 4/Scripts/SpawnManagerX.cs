using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnManagerX : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    public GameObject smashPowerupPrefab;

    private float spawnRangeX = 10;
    private float spawnZMin = 15;
    private float spawnZMax = 25;

    public int enemyCount;
    public int waveCount = 1;

    public GameObject player;
    public TextMeshProUGUI waveText;

    [Header("Enemy Difficulty")]
    [SerializeField] private float baseEnemySpeed = 3f;
    [SerializeField] private float speedIncreasePerWave = 0.5f;

    void Start()
    {
        UpdateWaveUI();
    }

    void Update()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCount == 0)
        {
            SpawnEnemyWave(waveCount);
        }
    }

    Vector3 GenerateSpawnPosition()
    {
        float xPos = Random.Range(-spawnRangeX, spawnRangeX);
        float zPos = Random.Range(spawnZMin, spawnZMax);
        return new Vector3(xPos, 0, zPos);
    }

    void SpawnEnemyWave(int enemiesToSpawn)
    {
        UpdateWaveUI();

        Vector3 powerupSpawnOffset = new Vector3(0, 0, -15);

        int totalPowerups = GameObject.FindGameObjectsWithTag("Powerup").Length
                         + GameObject.FindGameObjectsWithTag("SmashPowerup").Length;

        if (totalPowerups == 0)
        {
            bool spawnSmash = Random.value < 0.4f;

            GameObject chosen = spawnSmash ? smashPowerupPrefab : powerupPrefab;
            Vector3 pos = GenerateSpawnPosition() + powerupSpawnOffset;

            Instantiate(chosen, pos, chosen.transform.rotation);
        }

        float thisWaveSpeed = baseEnemySpeed + (waveCount - 1) * speedIncreasePerWave;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);

            EnemyX enemyScript = enemyObj.GetComponent<EnemyX>();
            if (enemyScript != null)
            {
                enemyScript.InitSpeed(thisWaveSpeed);
            }
        }

        waveCount++;

        ResetPlayerPosition();
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + waveCount;
        }
    }

    void ResetPlayerPosition()
    {
        player.transform.position = new Vector3(0, 1, -7);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}