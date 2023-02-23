using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class IABehaviour : MonoBehaviour
{
    enum State
    {
        FindEnemy,
        ShootingEnemy,
        IncomingProjectile,
        Test,
    }
    [SerializeField] State state;

    NavMeshAgent agent;

    [Range(0.0f, 100.0f)]
    [SerializeField] float health = 100.0f;

    [SerializeField] IABehaviour enemy;

    GameManager gm;

    [Header("Movement")]
    [SerializeField] bool canMove = true;
    [SerializeField] float areaRange = 10.0f;
    [SerializeField] List<Transform> hiddenPoints;
    int hiddenIndex = 0;
    [SerializeField] LayerMask raycastMask;

    [Header("Shooting")]
    [SerializeField] bool canShoot = true;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shootingPoint;
    [SerializeField] int ammoMagazine = 3;
    int ammoMagazineMax;
    [SerializeField] float timeToReload = 3.0f;
    float startTimeToReload;
    [SerializeField] float shootDelay = 0.5f;
    [SerializeField] GameObject vfxShoot;
    public List<GameObject> bulletsIncoming;

    [Header("UI")]
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider reloadingSlider;

    void Start()
    {
        transform.parent = null;

        // SET MAGAZINE
        ammoMagazineMax = ammoMagazine;
        startTimeToReload = timeToReload;

        // SET RELOADING BAR
        reloadingSlider.maxValue = startTimeToReload;
        reloadingSlider.minValue = 0.0f;

        // SET HEALTH BAR
        healthSlider.maxValue = health;
        healthSlider.minValue = 0.0f;

        agent = GetComponent<NavMeshAgent>();

        // SEARCH FOR THE ENEMY
        GameObject[] ia = GameObject.FindGameObjectsWithTag("IA");
        foreach (GameObject gm in ia)
        {
            if (gm != this.gameObject) enemy = gm.GetComponent<IABehaviour>();
        }

        // ADD HIDDEN POINTS
        GameObject[] h = GameObject.FindGameObjectsWithTag("HiddenPoint");
        foreach (GameObject gm in h)
        {
            hiddenPoints.Add(gm.transform);
        }

        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        gm.iasAlive.Add(this.gameObject);
    }

    void Update()
    {
        switch (state)
        {
            default:
            case State.FindEnemy:

                agent.isStopped = false;
                //agent.SetDestination(enemy.transform.position);
                Movement();

                if (EnemyInSight()) state = State.ShootingEnemy;

                if (EnemyInSight() && enemy.state == State.IncomingProjectile) enemy.state = State.FindEnemy;

                break;

            case State.ShootingEnemy:

                agent.isStopped = true;

                if (Vector3.Distance(transform.position, enemy.transform.position) <= 2.0f) state = State.FindEnemy;

                if (ammoMagazine > 0 && canShoot) Shooting();

                if (bulletsIncoming.Count > 0 && enemy.state != State.IncomingProjectile) state = State.IncomingProjectile;

                if (!EnemyInSight()) state = State.FindEnemy;

                break;

            case State.IncomingProjectile:

                this.transform.LookAt(enemy.transform);
                agent.isStopped = false;

                if (EnemyInSight()) GoToHiddenPlace();
                else
                {
                    hiddenIndex = 0;
                    state = State.FindEnemy;
                }

                break;

            case State.Test:

                break;
        }

        healthSlider.value = health; // UPDATE HEALTH BAR
        if (health <= 0)
        {
            gm.iasAlive.Remove(this.gameObject);
            Destroy(this.gameObject);
        }

        if (ammoMagazine <= 0)
        {
            Realoading();
        }
        else
        {
            reloadingSlider.gameObject.SetActive(false);
        }
    }

    void Movement()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(transform.position, areaRange, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.green, 1.0f);
                agent.SetDestination(point);
            }
        }
    }

    /// <summary>
    /// Create a Random Point in a area
    /// </summary>
    /// <param name="center">center of the area</param>
    /// <param name="range">range of the area</param>
    /// <param name="result">the random point in the area</param>
    /// <returns></returns>
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void GoToHiddenPlace()
    {
        Transform destination = null;
        float distance = 0.0f;

        if (hiddenPoints.Count > 0)
        {
            foreach (Transform tr in hiddenPoints)
            {
                if(Vector3.Distance(enemy.transform.position, tr.position) > distance)
                {
                    distance = Vector3.Distance(enemy.transform.position, tr.position);
                    destination = tr;
                }
            }
        }

        agent.SetDestination(destination.position);

        /*if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (hiddenIndex >= hiddenPoints.Count - 1)
            {
                hiddenIndex = 0;
            }

            if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                hiddenIndex++;
            }

            agent.SetDestination(destination.position);
        }*/
    }

    bool EnemyInSight()
    {
        Vector3 direction = enemy.transform.position - transform.position;
        Debug.DrawRay(transform.position, direction);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, raycastMask))
        {
            if (hit.collider.tag == "IA")
            {
                return true;
            }
        }

        return false;
    }

    void Shooting()
    {
        ammoMagazine -= 1;
        this.transform.LookAt(enemy.transform.position + enemy.agent.velocity);
        GameObject shootingBullet = Instantiate(bulletPrefab, shootingPoint);
        enemy.bulletsIncoming.Add(shootingBullet);
        Instantiate(vfxShoot, shootingPoint);

        canShoot = false;
        StartCoroutine(ShootDelay());
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    void Realoading()
    {
        reloadingSlider.gameObject.SetActive(true);
        reloadingSlider.value = timeToReload;

        if (timeToReload > 0)
        {
            timeToReload -= Time.deltaTime;
        }
        else if (timeToReload <= 0)
        {
            ammoMagazine = ammoMagazineMax;
            timeToReload = startTimeToReload;
        }
    }

    public void TakeDamages(float damages)
    {
        health -= damages;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, areaRange);
    }
}