using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyAfterDelay))]
public class BallBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject hitbox;
    [SerializeField] private GameObject hurtbox;
    [SerializeField] private CircleCollider2D collisionBox;
    
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private GameObject bigExplosionPrefab;

    [SerializeField] private int maxBounces;

    [SerializeField] private int bounceCount;

    //handler for if we want the shots to hit other shots of the same type
    private bool isSelfColliding;
    
    // Start is called before the first frame update
    void Start()
    {
        bounceCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSetup(int maxBounces, float hitboxDelay)
    {
        this.maxBounces = maxBounces;
        DelayCollision(hitboxDelay);
    }

    private void DelayCollision(float timeInSeconds)
    {
        StartCoroutine(DoDelayCollision(timeInSeconds));
    }

    private IEnumerator DoDelayCollision(float timeInSeconds)
    {
        yield return new WaitForSeconds(timeInSeconds);
        hitbox.SetActive(true);
        hurtbox.SetActive(true);
        collisionBox.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit something not geo");
        if (other.CompareTag("Hitbox") || other.CompareTag("Hurtbox"))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("Hit player");
                doSmallExplosion();
                return;
            }

            if (other.GetComponent<BallBehaviour>() && !isSelfColliding)
            {
                return;
            }
            doBigExplosion();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Geo"))
        {
            bounceCount++;
            if (bounceCount >= maxBounces)
            {
                doSmallExplosion();
            }
        }
    }

    public void doSmallExplosion()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void doBigExplosion()
    {
        Instantiate(bigExplosionPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
