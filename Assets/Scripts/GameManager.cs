using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> iasAlive;

    [SerializeField] GameObject iaRed;
    [SerializeField] GameObject iaBlue;

    [SerializeField] GameObject canvasStart;

    [Header("Spawn Points")]
    [SerializeField] Transform spawnRed;
    [SerializeField] Transform spawnBlue;

    void Start()
    {
        
    }

    void Update()
    {
        if (iasAlive.Count <= 1)
        {
            canvasStart.SetActive(true);
        }
        else
        {
            canvasStart.SetActive(false);
        }
    }

    public void SpawnIA()
    {
        foreach (GameObject gm in iasAlive)
        {
            Destroy(gm);
        }

        iasAlive.Clear();

        Instantiate(iaRed, spawnRed);
        Instantiate(iaBlue, spawnBlue);
    }
}
