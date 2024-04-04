using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Controls")] 
    [Space] 
    public KeyCode left;
    public KeyCode right, up, down, dash, jump, attack;

    public float deadZone;

    [Header("Parameters")]
    [Space] 
    public float moveSpeed;

    [FormerlySerializedAs("grounded")] 
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool dJump;
    [SerializeField] private bool onJumpCd;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float rayLength;

    [Header("Dash stuff")] 
    [SerializeField] private CapsuleCollider2D hurtBox;

    [SerializeField] private float dashLength;

    [SerializeField] private float dashSpeed;

    [SerializeField] private float recovery;

    [SerializeField] private bool hasAirDash;

    [SerializeField] private int facingDirection = 1;
    
    private Rigidbody2D thisBody;

    private GameObject playerVis;

    private SpriteRenderer playerSprite;
    //using this to store what keys were pressed from update to fixed update
    private Vector2 movement;

    //player state control - attacking will be needed later
    private playerState currentState;

    private enum playerState
    {
        Idle,
        Attacking,
        Dashing
    }
    
    void Start()
    {
        thisBody = GetComponent<Rigidbody2D>();
        foreach (var col in gameObject.GetComponentsInChildren<CapsuleCollider2D>())
        {
            if (col.gameObject.CompareTag("Hurtbox"))
            {
                hurtBox = col;
                break;
            }
        }

        currentState = playerState.Idle;
        playerVis = GameObject.FindWithTag("Player Sprite");
        playerSprite = playerVis.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = Vector2.zero;
        if (Input.GetAxis("Horizontal") > deadZone)
        {
            movement.x++;
            //Debug.Log("Right");
        }

        if (Input.GetAxis("Horizontal") < deadZone)
        {
            movement.x--;
            //Debug.Log("Left");
        }

        if (Input.GetAxis("Vertical") > deadZone)
        {
            movement.y++;
            //Debug.Log("Up");
        }

        if (Input.GetAxis("Vertical") < deadZone)
        {
            movement.y--;
            //Debug.Log("Down");
        }

        if (Input.GetKeyDown(dash) && currentState == playerState.Idle)
        {
            StartCoroutine(Dash());
        }

        if (Time.timeScale == 1)
        {
            if (currentState == playerState.Idle)
            {
                if (movement.x != 0)
                {
                    Move(movement);
                }
                else
                {
                    thisBody.velocity = new Vector2(0f, thisBody.velocity.y);
                }

                if (Input.GetAxis("Jump") > 0 && (isGrounded || dJump))
                {
                    if (!onJumpCd)
                    {
                        StartCoroutine(jumpCooldown());
                        Jump();
                    }
                }
            }

            if (Input.GetAxis("Jump") == 0)
            {
                StopCoroutine(jumpCooldown());
                onJumpCd = false;
            }
        }
    }

    private void FixedUpdate()
    {
        int mask = 1 << 11;
        RaycastHit2D hit = Physics2D.Raycast(thisBody.transform.position, Vector2.down, rayLength, mask);
        Debug.DrawLine(thisBody.transform.position, thisBody.transform.position + (Vector3.down*rayLength), Color.green);
        if (hit.collider)
        {
            if (hit.collider.CompareTag("Geo"))
            {
                //Debug.Log("Ground");
                isGrounded = true;
                dJump = true;
                hasAirDash = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private IEnumerator Dash()
    {
        if (!isGrounded && !hasAirDash) yield break;
        if (!isGrounded) hasAirDash = false;
        float gravScale = thisBody.gravityScale;
        currentState = playerState.Dashing;
        thisBody.gravityScale = 0;
        
        thisBody.velocity = new Vector2(dashSpeed * facingDirection, 0f);
        hurtBox.enabled = false;
        //placeholder for animation
        playerSprite.color = Color.cyan;
        yield return new WaitForSeconds(dashLength);
        playerSprite.color = Color.white;
        thisBody.velocity = Vector2.zero;
        hurtBox.enabled = true;
        
        thisBody.gravityScale = gravScale;
        yield return new WaitForSeconds(recovery);
        currentState = playerState.Idle;
    }

    private void Jump()
    {
        if (!isGrounded)
        {
            dJump = false;
        }
        Debug.Log("Jumped");
        thisBody.velocity = new Vector2(thisBody.velocity.x,  1 * jumpHeight);
    }

    private IEnumerator jumpCooldown()
    {
        onJumpCd = true;
        yield return new WaitForSeconds(1f);
        onJumpCd = false;
    }

    private void Move(Vector2 input)
    {
        float addMove = 0;
        
        if (movement.x > 0)
        {
            //thisBody.MovePosition(transform.position + new Vector3(1f,0f));
            facingDirection = 1;
            addMove = 1f;
        }

        if (movement.x < 0)
        {
            facingDirection = -1;
            addMove = -1f;
        }
        
        //thisBody.AddForce(addMove * (Time.deltaTime * moveSpeed));
        thisBody.velocity = new Vector2(addMove * moveSpeed, thisBody.velocity.y);
        movement = Vector2.zero;
    }
}
