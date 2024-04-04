using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class HiveMind : MonoBehaviour
{
    //might not be good because I want them to be interchangeable (ball, shotgun and shotgun, ball)
    //public UnityEvent[] attacks;
    [SerializeField] private BossBehaviour mind1, mind2;

    [Serializable]
    public enum AttackType
    {
        Ball,
        Shotgun,
        Tesla,
        Beam
    }
    
    [Serializable]
    public struct JointAttack
    {
        [SerializeField] private AttackType atck1;
        [SerializeField] private AttackType atck2;

        public AttackType Atck1
        {
            get => atck1;
            set => atck1 = value;
        }

        public AttackType Atck2
        {
            get => atck2;
            set => atck2 = value;
        }
    }

    [FormerlySerializedAs("AttacksArray")] [SerializeField] private JointAttack[] attacksArray; 

    private Coroutine patterns;
    
    // Start is called before the first frame update
    void Start()
    {
        patterns = StartCoroutine(AttackPatterns());
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Enemy");
        mind1 = temp[0].GetComponent<BossBehaviour>();
        mind2 = temp[1].GetComponent<BossBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pause()
    {
        StopCoroutine(patterns);
    }

    public void UnPause()
    {
        patterns = StartCoroutine(AttackPatterns());
    }

    private IEnumerator AttackPatterns()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (mind1.currentState != BossState.Attacking && mind2.currentState != BossState.Attacking)
            {
                int chosen = Random.Range(0, attacksArray.Length);
                if (Random.Range(0, 2) == 1)
                {
                    mind1.DoAttack(attacksArray[chosen].Atck1);
                    mind2.DoAttack(attacksArray[chosen].Atck2);
                }
                else
                {
                    mind2.DoAttack(attacksArray[chosen].Atck1);
                    mind1.DoAttack(attacksArray[chosen].Atck2);
                }
            }
        }
    }
}
