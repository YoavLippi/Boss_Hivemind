using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossBehaviour : MonoBehaviour
{
    [Header("State Control")]
    public BossState currentState;

    public bool debugging;
    
    [Header("Pos Handling")]
    public Transform playerPos;
    public PosInfo chosenPosition;
    private Rigidbody2D rb;
    public Transform shootingPlug;
    public GameObject animObject;
    private int lookDir;
    private bool canTurn;
    
    [Header("Position Plugs (Auto Populating)")]
    public List<PosInfo> shotgunPositions;
    public List<PosInfo> beamPositions;
    public List<PosInfo> ballPositions;
    public List<PosInfo> teslaPositions;
    public List<PosInfo> throwPositions;

    [Header("Prefabs")] 
    public GameObject ballPrefab;
    public GameObject shotgunPrefab;
    public GameObject beamPrefab;
    public GameObject teslaPrefab;
    
    //Attack stuff
    [Header("Attack Handling")] 
    public bool inAnim;

    public float shotgunSpeed;

    public float ballSpeed;

    private void Start()
    {
        //inAnim = false;
        shotgunPositions = InitialisePositions("Shotgun Pos");
        beamPositions = InitialisePositions("Beam Pos");
        ballPositions = InitialisePositions("Ball Pos");
        teslaPositions = InitialisePositions("Tesla Pos Boss");
        throwPositions = InitialisePositions("Tesla Pos Object");
        playerPos = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        canTurn = true;
    }

    private void Update()
    {
        if (canTurn)
        {
            lookTowardPlayer();
        }
    }

    private void lookTowardPlayer()
    {
        float tolerance = 1f;
        if (Math.Abs(playerPos.position.x - transform.position.x) < tolerance) return;
        if (playerPos.position.x - transform.position.x >= 0)
        {
            //To the right
            lookDir = 1;
            animObject.transform.rotation = Quaternion.Euler(0,0,0);
        }
        else
        {
            //To the left
            lookDir = -1;
            animObject.transform.rotation = Quaternion.Euler(0,180,0);
        }
    }

    //returns a list of all posinfo components that are tagged with the inputted string 
    private List<PosInfo> InitialisePositions(string tag)
    {
        List<PosInfo> output = new List<PosInfo>();
        foreach (var OBJECT in GameObject.FindGameObjectsWithTag(tag))
        {
            if (OBJECT.GetComponent<PosInfo>())
            {
                output.Add(OBJECT.GetComponent<PosInfo>());
            }
        }

        return output;
    }

    public void DoAttack(HiveMind.AttackType attackType)
    {
        if (currentState == BossState.Attacking) return;
        currentState = BossState.Attacking;
        
        switch (attackType)
        {
            case HiveMind.AttackType.Ball :
                if (debugging) Debug.Log($"{gameObject} did Ball");
                StartCoroutine(BallAttack());
                break;
            case HiveMind.AttackType.Beam :
                if (debugging) Debug.Log($"{gameObject} did Beam");
                StartCoroutine(BeamAttack());
                break;
            case HiveMind.AttackType.Shotgun :
                if (debugging) Debug.Log($"{gameObject} did Shotgun");
                StartCoroutine(ShotgunAttack());
                break;
            case HiveMind.AttackType.Tesla :
                if (debugging) Debug.Log($"{gameObject} did Tesla");
                StartCoroutine(TeslaAttack());
                break;
        }
    }

    private IEnumerator BallAttack()
    {
        while (true)
        {
            int chosen = Random.Range(0, ballPositions.Count);
            if (ballPositions[chosen].getOccupied() == false)
            {
                chosenPosition = ballPositions[chosen];
                //the method handles adjacencies too
                chosenPosition.setOccupied(true);
                break;
            }
        }
        
        rb.MovePosition(chosenPosition.position);
        inAnim = true;
        StartCoroutine(DoBallAnimation());
        yield return new WaitUntil(() => inAnim == false);
        chosenPosition.setOccupied(false);
        currentState = BossState.Idle;
    }

    private IEnumerator DoBallAnimation()
    {
        //Debug.Log("DOING BALL ATTACK");
        float time = 0;
        //amount of time the phase will last
        //Phase 1 is just a reaction allowance
        float phase1 = 0.5f;
        yield return new WaitForSeconds(phase1);

        //Does shot
        Vector2 shotDir = playerPos.position - shootingPlug.position;
        shotDir.Normalize();
        GameObject ballShot = Instantiate(ballPrefab, shootingPlug.position, Quaternion.identity);
        ballShot.GetComponent<Rigidbody2D>().velocity = shotDir * ballSpeed;
        ballShot.GetComponent<BallBehaviour>().DoSetup(3, 0f);
        
        //Waiting for recovery
        time = 0;
        float phase2 = 2f;
        yield return new WaitForSeconds(phase2);
        
        inAnim = false;
    }

    /*private void Update()
    {
        Vector2 beamDir = playerPos.position - transform.position;
        Debug.DrawRay(transform.position, beamDir);
    }*/

    private IEnumerator BeamAttack()
    {
        while (true)
        {
            int chosen = Random.Range(0, beamPositions.Count);
            if (beamPositions[chosen].getOccupied() == false)
            {
                chosenPosition = beamPositions[chosen];
                chosenPosition.setOccupied(true);
                break;
            }
        }
        
        rb.MovePosition(chosenPosition.position);

        yield return new WaitForSeconds(0.5f);
        
        Vector2 beamDir = (Vector2) playerPos.position - chosenPosition.position;
        int geoMask = 1 << 11;
        
        RaycastHit2D hit = Physics2D.Raycast(chosenPosition.position, beamDir, float.PositiveInfinity, geoMask);
        if (hit)
        {
            //Debug.Log($"The beam from {this.name} would hit {hit.point}");
            GameObject beamPointer = Instantiate(beamPrefab);
            beamPointer.transform.position = chosenPosition.position;
            LineRenderer beamRender = beamPointer.GetComponent<LineRenderer>();
            beamRender.positionCount = 2;
            beamRender.SetPosition(0, shootingPlug.position);
            beamRender.SetPosition(1, hit.point);
            inAnim = true;
            StartCoroutine(DoBeamAnimation(beamRender));
            yield return new WaitUntil(() => inAnim == false);
            Destroy(beamPointer);
        }
        //temporary placeholder
        yield return new WaitForSeconds(0.5f);
        canTurn = true;
        chosenPosition.setOccupied(false);
        currentState = BossState.Idle;
    }

    private IEnumerator DoBeamAnimation(LineRenderer inputBeam)
    {
        float time = 0;
        //amount of time the phase will last
        float phase1 = 1.25f;

        Material tempMat = inputBeam.materials[0];
        inputBeam.materials[0].color = new Color(tempMat.color.r, tempMat.color.g, tempMat.color.b, 0.5f);
        Vector2 beamDir = playerPos.position - shootingPlug.position;
        
        //tracking phase
        canTurn = false;
        inputBeam.widthCurve = AnimationCurve.Constant(0f,0f,0.1f);
        do
        {
            beamDir = playerPos.position - shootingPlug.position;
            int geoMask = 1 << 11;
            RaycastHit2D hit = Physics2D.Raycast(shootingPlug.position, beamDir, float.PositiveInfinity, geoMask);
            if (hit) inputBeam.SetPosition(1, hit.point);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (time < phase1);
        
        //Wind Up to shoot
        inputBeam.materials[0].color = new Color(tempMat.color.r, tempMat.color.g, tempMat.color.b, 1f);
        time = 0;
        float windUp = 0.5f;
        do
        {
            float fadeVal = mapFloat(time, 0, windUp, 0.5f, 0);
            float widthVal = mapFloat(time, 0, windUp, 0.1f, 0);
            inputBeam.startWidth = widthVal;
            inputBeam.materials[0].color = new Color(tempMat.color.r, tempMat.color.g, tempMat.color.b, fadeVal);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (time < windUp);
        
        //Shooting
        bool particle = false;
        ParticleSystem particleSys =  inputBeam.gameObject.GetComponentInChildren<ParticleSystem>();
        float rotationAmount = (float)Mathf.Atan2(beamDir.y, beamDir.x) * Mathf.Rad2Deg;
        particleSys.transform.rotation = Quaternion.Euler(0, 0, rotationAmount);
        
        PolygonCollider2D polyCollider = inputBeam.GetComponentInChildren<PolygonCollider2D>();
        List<Vector2> reversedPoints = new List<Vector2>();
        List<Vector2> normalPoints = new List<Vector2>();

        Transform cam = Camera.main.transform;
        List<Vector2> allPoints = new List<Vector2>();
        allPoints.Clear();
        for (int i = 0; i < inputBeam.positionCount; i++)
        {
            //Vector3 temp = cam.InverseTransformVector(inputBeam.GetPosition(i));
            Vector3 temp = inputBeam.GetPosition(i);
            float tempX = temp.x;
            float tempY = temp.y;
            allPoints.Add(new Vector2(tempX, tempY));
            //Debug.Log($"Added point {allPoints[i]}");
        }
        
        //ending width is 0.4
        float offset = 0.2f;
        for (int i = 0; i < allPoints.Count; i++)
        {
            Vector2 direction = Vector2.zero;
            /*if (i < allPoints.Count - 1)
            {
                direction = (allPoints[i + 1] - allPoints[i]).normalized;
            }
            else
            {
                direction = (allPoints[0] - allPoints[i]).normalized;
            }*/
            direction = (allPoints[1] - allPoints[0]).normalized;


            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * offset;
            normalPoints.Add(allPoints[i] + perpendicular);
            reversedPoints.Add(allPoints[i] - perpendicular);
        }

        for (int i = allPoints.Count - 1; i >= 0; i--)
        {
            //Debug.Log($"Added point {reversedPoints[i]}");
            normalPoints.Add(reversedPoints[i]);
        }

        polyCollider.offset = -allPoints[0];
        polyCollider.points = normalPoints.ToArray();
        
        time = 0;
        float shootTime = 0.5f;
        inputBeam.materials[0].color = new Color(tempMat.color.r, tempMat.color.g, tempMat.color.b, 1);
        do
        {
            float widthVal = mapFloat(time, 0, shootTime/4f, 0f, 0.6f);
            if (time > shootTime / 4f)
            {
                if (!particle)
                {
                    particle = true;
                    particleSys.Play();
                }
                widthVal = mapFloat(time, shootTime/3f, shootTime/2f, 0.6f, 0.4f);
            }
            inputBeam.startWidth = widthVal;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (time < shootTime);

        yield return new WaitForSeconds(shootTime/2f);
        inAnim = false;
        canTurn = true;
    }
    
    private IEnumerator ShotgunAttack()
    {
        while (true)
        {
            int chosen = Random.Range(0, shotgunPositions.Count);
            if (shotgunPositions[chosen].getOccupied() == false)
            {
                chosenPosition = shotgunPositions[chosen];
                chosenPosition.setOccupied(true);
                break;
            }
        }
        
        rb.MovePosition(chosenPosition.position);
        inAnim = true;
        StartCoroutine(DoShotgunAnimation());
        yield return new WaitUntil(() => inAnim == false);
        //temporary placeholder
        yield return new WaitForSeconds(0.5f);
        chosenPosition.setOccupied(false);
        currentState = BossState.Idle;
    }

    private IEnumerator DoShotgunAnimation()
    {
        //reaction time for the player
        float phase1 = 0.5f;
        yield return new WaitForSeconds(phase1);
        
        Vector2 mainDir = playerPos.position - shootingPlug.position;
        mainDir.Normalize();
        //offset amount in radians
        float offsetAngleRad = Mathf.Deg2Rad * 30f;
        float sinOffset = Mathf.Sin(offsetAngleRad);
        float cosOffset = Mathf.Cos(offsetAngleRad);
        Vector2 offset1 = new Vector2(
            mainDir.x * cosOffset - mainDir.y * sinOffset,
            mainDir.x * sinOffset + mainDir.y * cosOffset
        );
        
        Vector2 offset2 = new Vector2(
            mainDir.x * cosOffset + mainDir.y * sinOffset,
            -mainDir.x * sinOffset + mainDir.y * cosOffset
        );
        
        GameObject ballShot1 = Instantiate(ballPrefab, shootingPlug.position, Quaternion.identity);
        GameObject ballShot2 = Instantiate(ballPrefab, shootingPlug.position, Quaternion.identity);
        GameObject ballShot3 = Instantiate(ballPrefab, shootingPlug.position, Quaternion.identity);
        GameObject[] balls = new[] { ballShot1, ballShot2, ballShot3 };
        foreach (var ball in balls)
        {
            ball.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            ball.GetComponent<BallBehaviour>().DoSetup(0, 0.3f);
        }
        ballShot1.GetComponent<Rigidbody2D>().velocity = mainDir * shotgunSpeed;
        ballShot2.GetComponent<Rigidbody2D>().velocity = offset1 * shotgunSpeed;
        ballShot3.GetComponent<Rigidbody2D>().velocity = offset2 * shotgunSpeed;
        yield return new WaitForSeconds(0.5f);
        inAnim = false;
    }
    
    private IEnumerator TeslaAttack()
    {
        while (true)
        {
            int chosen = Random.Range(0, teslaPositions.Count);
            if (teslaPositions[chosen].getOccupied() == false)
            {
                chosenPosition = teslaPositions[chosen];
                chosenPosition.setOccupied(true);
                break;
            }
        }
        
        rb.MovePosition(chosenPosition.position);
        //temporary placeholder
        yield return new WaitForSeconds(2f);
        chosenPosition.setOccupied(false);
        currentState = BossState.Idle;
    }

    public static float mapFloat(float input, float min1, float max1, float min2, float max2)
    {
        float temp = Mathf.InverseLerp(min1, max1, input);
        float output = Mathf.Lerp(min2, max2, temp);
        return output;
    }
    
}