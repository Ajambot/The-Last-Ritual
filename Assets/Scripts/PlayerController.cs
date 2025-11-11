using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 6f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = true;
    private bool isFacingRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Hello World");
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Move character
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput != 0) {
            anim.SetBool("isWalking", true);
        } else {
            anim.SetBool("isWalking", false);
        }

        // Flip character if needed
        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();


        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            Debug.Log("Jumping");
            isGrounded = false;
        }

        // Attack
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("attack"))
        {
            Debug.Log("attacking");
            anim.SetTrigger("attack");
        }

        // Optional: Casting / Victory / Hurt triggers
        if (Input.GetKeyDown(KeyCode.C) && !anim.GetBool("casting")) {
            Debug.Log("casting");
            anim.SetTrigger("casting");
        }

        if (Input.GetKeyDown(KeyCode.V) && !anim.GetBool("victory")) {
            Debug.Log("victory");
            anim.SetTrigger("victory");
        }
    }

    // Detect ground (simple version)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            anim.ResetTrigger("Jump");
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
