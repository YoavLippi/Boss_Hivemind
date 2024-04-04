using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float currentTimeActive;

    [SerializeField] private float upTime;

    public UnityEvent OnDestruction;
    // Start is called before the first frame update
    void Start()
    {
        currentTimeActive = 0;
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeActive += Time.deltaTime;
        if (currentTimeActive >= upTime)
        {
            OnDestruction?.Invoke();
            Destroy(this.gameObject);
        }
    }
}
