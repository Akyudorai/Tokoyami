using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{

    public PlayerCharacter character;
    public PlayerInput pi;
    public Rigidbody2D rigid;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;

    public float Speed = 3.0f;
    public float JumpForce = 5.0f;
    public bool isGrounded = false;
    public bool canJump = true;
    public Coroutine jumpCR;

    public float deaccelForce = 0.5f;
    public bool canDash = true;
    public float dashForce = 10f;
    public bool isDashing = false;
    public Coroutine dashCR;

    [Header("Collision")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;    
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.12f);



    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        pi = GetComponent<PlayerInput>();
        
        // - Initialize Character
        character = GetComponent<PlayerCharacter>();
        character.pc = this;
    }

    private void Start()
    {
        Debug.Log(pi);
        moveAction = pi.currentActionMap.FindAction("Move");
        moveAction.Enable();

        jumpAction = pi.currentActionMap.FindAction("Jump");
        jumpAction.Enable();

        dashAction = pi.currentActionMap.FindAction("Dash");
        dashAction.Enable();
        
    }

    private void Update()
    {
        HandleMoveInput();
        HandleJumpInput();
        HandleDashInput();

        // - Set scale based on direction of velocity
        if (rigid.linearVelocity.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        } else if (rigid.linearVelocityX < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
    }

    private void HandleMoveInput()
    {
        Vector3 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, 0);
               
        if (rigid.linearVelocityX < Speed || rigid.linearVelocityX > -Speed)
        {   
            // Multiply by Acceleration
            rigid.AddForce(move * Speed * 10f, ForceMode2D.Force);
        }
        
        if (Mathf.Abs(rigid.linearVelocityX) > 0 && input.x == 0 && !isDashing)
        {            
            // - Calculate Deacceleration Force
            Vector3 deaccel = new Vector3(-rigid.linearVelocityX * deaccelForce, 0, 0);

            // - Apply Deacceleration Force
            rigid.AddForce(deaccel, ForceMode2D.Force);
        }
        
        // Max Speed Global
        if (rigid.linearVelocityX > 10f) rigid.linearVelocityX = 10f; 
        if (rigid.linearVelocityX < -10f) rigid.linearVelocityX = -10f;
        //transform.position += move * Speed * Time.deltaTime;
    }

    private void HandleJumpInput()
    {
        // - Check Grounded State
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);
       
        // - Handle Jump Force
        if (jumpAction.IsPressed() && isGrounded && canJump)
        {
            rigid.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            jumpCR = StartCoroutine(TriggerJump());
        }
    }    

    private IEnumerator TriggerJump()
    {
        
        canJump = false;
        yield return new WaitForSeconds(0.25f);
        canJump = true;
    }

    private void HandleDashInput()
    {
        if (dashAction.IsPressed() && canDash)
        {
            Debug.Log("Dashing");
           
            // - Get Mouse Aim Direction
            Vector3 screenPosition = Mouse.current.position.ReadValue();
            screenPosition.z = Camera.main.nearClipPlane; // Or some fixed distance like 10
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            mouseWorldPosition.z = 0;

            Vector3 direction = mouseWorldPosition - transform.position;
            Vector3 dashDir = direction.normalized;
            
            // - Apply Impulsive Force in Aim Direction
            rigid.linearVelocityY = 0f;
            rigid.linearVelocityX = 0f;
            rigid.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
            
            // - Dash Delay
            dashCR = StartCoroutine(TriggerDash());
        }       
    }

    // TODO: Break me into two coroutines: Move isDashing = false and gravity Scale 1 to a separate coroutine timer
    private IEnumerator TriggerDash()
    {
        canDash = false;
        isDashing = true;
        rigid.gravityScale = 0;
        yield return new WaitForSeconds(0.5f);        
        isDashing = false;
        rigid.gravityScale = 1;
        yield return new WaitForSeconds(1.0f);
        canDash = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Absorbable"))
        {
            I_Absorbable target = other.GetComponent<I_Absorbable>();
            character.ProcessAbsorbEffect(target);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.matrix = Matrix4x4.TRS(groundCheck.position, Quaternion.identity, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, groundCheckSize);
        }
    }
}
