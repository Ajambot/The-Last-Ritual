using UnityEngine;
using UnityEngine.Tilemaps; // <-- needed for Tilemap access

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 6f;

    private int health = 100;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = true;
    private bool isFacingRight = false;

    [Header("Wood Burn Settings")]
    public Tilemap targetTilemap;      // The Tilemap you painted wood on (Decor/Foreground/etc.)
    public TileBase woodTile;          // Wood_Tile
    public TileBase fireAnimatedTile;  // Fire_AnimatedTile

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    public float burnSeconds = 1.2f;   // Lifetime of fire before clearing
    public KeyCode interactKey = KeyCode.E;

    // Control
    public int maxTilesAhead = 2;      // Scan up to N tiles in front; burns the first wood found
    public int burnRadius = 0;         // 0 = only that tile; 1 = also its neighbors, etc.
    public float probeFromFeet = 0.2f; // Vertical offset up from feet when probing

    void Update()
    {
        if (health <= 0)
        {
            anim.SetTrigger("die");
            //Debug.Log("Im dead");
        }
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        anim.SetBool("isWalking", moveInput != 0);

        // Flip character if needed
        if (moveInput > 0)
        {
            isFacingRight = true;
            Vector3 localScale = transform.localScale;
            localScale.x = -1f;
            transform.localScale = localScale;
        }
        else if (moveInput < 0)
        {
            isFacingRight = false;
            Vector3 localScale = transform.localScale;
            localScale.x = 1f;
            transform.localScale = localScale;
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }


        // Optional casting / victory
        if (Input.GetKeyDown(KeyCode.C) && !anim.GetBool("casting"))
            anim.SetTrigger("casting");
        if (Input.GetKeyDown(KeyCode.V) && !anim.GetBool("victory"))
            anim.SetTrigger("victory");

        // 🔥 Interact key for burning wood
        if (Input.GetKeyDown(interactKey))
            TryBurnWood();
    }

    // -----------------------------
    // 🔥 Tilemap burn interaction
    // -----------------------------
    void TryBurnWood()
    {
        if (targetTilemap == null || woodTile == null || fireAnimatedTile == null) return;

        // Use player collider center and tile size to probe precisely one tile at a time
        var col = GetComponent<Collider2D>();
        Vector3 origin = (col ? col.bounds.center : transform.position);
        origin.y = (col ? col.bounds.min.y + probeFromFeet : origin.y); // probe near feet

        var grid = targetTilemap.layoutGrid != null ? targetTilemap.layoutGrid : targetTilemap.GetComponentInParent<Grid>();
        float tileW = (grid != null ? grid.cellSize.x : 1f);

        float dir = isFacingRight ? 1f : -1f;

        // Look forward up to N tiles and pick the FIRST wood we find
        for (int i = 0; i < maxTilesAhead; i++)
        {
            // probe at the center of each tile in front: 0.5, 1.5, 2.5, ...
            Vector3 probe = origin + new Vector3(dir * ((i + 0.5f) * tileW), 0f, 0f);
            Vector3Int cell = targetTilemap.WorldToCell(probe);

            if (targetTilemap.GetTile(cell) == woodTile)
            {
                BurnAt(cell);                    // burn exactly that tile
                if (burnRadius > 0) BurnNeighbors(cell, burnRadius); // optional chain radius
                break;                           // stop after first wood in front
            }
        }
    }

    void BurnAt(Vector3Int cell)
    {
        targetTilemap.SetTile(cell, fireAnimatedTile);
        StartCoroutine(RemoveAfter(cell, burnSeconds));
    }

    // Optional: burn neighbors within a Chebyshev radius (squares around the center)
    void BurnNeighbors(Vector3Int center, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var c = new Vector3Int(center.x + dx, center.y + dy, center.z);
                if (targetTilemap.GetTile(c) == woodTile) BurnAt(c);
            }
        }
    }



    System.Collections.IEnumerator RemoveAfter(Vector3Int cell, float delay)
    {
        yield return new WaitForSeconds(delay);
        targetTilemap.SetTile(cell, null); // clear tile
    }

    // Detect ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    private void Flip()
    {
        Debug.Log("Flip");
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void Damage(int dmg)
    {
        health -= dmg;
        Debug.Log(health);
    }
}
