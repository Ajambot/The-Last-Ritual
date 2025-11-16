using UnityEngine;

public class SkeletonRitualSpawner : MonoBehaviour
{
    [Header("Timer reference")]
    public RitualTimer ritualTimer;   // drag your RitualTimer here

    [Header("Skeleton prefabs")]
    public GameObject swordSkeletonPrefab;
    public GameObject axeSkeletonPrefab;
    public GameObject shieldSkeletonPrefab;

    [Header("Spawn points (optional)")]
    public Transform[] spawnPoints;   // if empty, spawns at this GameObject's position

    [Header("Spawning")]
    public float spawnInterval = 3f;  // how often to spawn during the ritual

    [Header("Buffs applied to ritual skeletons")]
    //public int buffedMaxHealth = 10;  // no longer used
    public float buffedMoveSpeed = 3f;

    [Header("Audio")]
    public AudioClip spawnClip;           // drag your Spawn sound here
    [Range(0f, 1f)]
    public float spawnVolume = 1f;

    float nextSpawnTime;

    void Update()
    {
        if (ritualTimer == null)
            return;

        if (!ritualTimer.IsRunning || ritualTimer.IsFinished)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnBuffedSkeleton();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnBuffedSkeleton()
    {
        GameObject prefab = PickRandomSkeletonPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("SkeletonRitualSpawner: no skeleton prefab assigned.");
            return;
        }

        EnemyPatrolAndAttack template = prefab.GetComponent<EnemyPatrolAndAttack>();
        if (template == null)
        {
            Debug.LogWarning("SkeletonRitualSpawner: prefab has no EnemyPatrolAndAttack component.");
            return;
        }

        // save original prefab values (HP not touched anymore)
        float originalMoveSpeed = template.moveSpeed;
        EnemyPatrolAndAttack.MovementMode originalMode = template.movementMode;

        // buff prefab for this spawn (speed + tracking only)
        template.moveSpeed    = buffedMoveSpeed;
        template.movementMode = EnemyPatrolAndAttack.MovementMode.Tracking;

        // pick spawn position
        Vector3 spawnPos = transform.position;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (p != null)
                spawnPos = p.position;
        }

        // instantiate enemy
        Instantiate(prefab, spawnPos, Quaternion.identity);

        // play spawn sound
        if (spawnClip != null)
            AudioSource.PlayClipAtPoint(spawnClip, spawnPos, spawnVolume);

        // restore prefab values
        template.moveSpeed    = originalMoveSpeed;
        template.movementMode = originalMode;
    }

    GameObject PickRandomSkeletonPrefab()
    {
        int count = 0;
        if (swordSkeletonPrefab != null) count++;
        if (axeSkeletonPrefab != null) count++;
        if (shieldSkeletonPrefab != null) count++;

        if (count == 0)
            return null;

        int index = Random.Range(0, count);

        if (swordSkeletonPrefab != null)
        {
            if (index == 0) return swordSkeletonPrefab;
            index--;
        }
        if (axeSkeletonPrefab != null)
        {
            if (index == 0) return axeSkeletonPrefab;
            index--;
        }

        return shieldSkeletonPrefab;
    }
}


