using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float bulletSpeed = 15.0f;
    [SerializeField] float damage = 20.0f;
    [SerializeField] AudioClip shootClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] GameObject vfxHit;
    [SerializeField] List<IABehaviour> ias;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        transform.parent = null;
        rb.AddForce(transform.up * bulletSpeed);

        AudioSource.PlayClipAtPoint(shootClip, transform.position);

        foreach (GameObject gm in GameObject.FindGameObjectsWithTag("IA"))
        {
            IABehaviour ia = gm.GetComponent<IABehaviour>();
            ias.Add(ia);
        }
    }

    private void Update()
    {
        StartCoroutine(Destroy());
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(2.0f);

        foreach (IABehaviour ia in ias)
        {
            if (ia.bulletsIncoming.Contains(this.gameObject))
            {
                ia.bulletsIncoming.Remove(this.gameObject);
            }
        }
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "IA")
        {
            IABehaviour ia = collision.collider.GetComponent<IABehaviour>();
            ia.TakeDamages(damage);
        }

        AudioSource.PlayClipAtPoint(hitClip, transform.position);
        Instantiate(vfxHit, transform.position, Quaternion.identity);

        foreach (IABehaviour ia in ias)
        {
            if (ia.bulletsIncoming.Contains(this.gameObject))
            {
                ia.bulletsIncoming.Remove(this.gameObject);
            }
        }
        Destroy(this.gameObject);
    }
}