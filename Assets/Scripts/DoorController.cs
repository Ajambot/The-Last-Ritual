using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorController : MonoBehaviour
{
    [Header("GameObject door (recommended)")]
    public Collider2D solidCollider;     // drag this door’s BoxCollider2D
    public SpriteRenderer sprite;         // drag this door’s SpriteRenderer
    public Animator animator;             // optional if you have an open anim

    [Header("If your door is painted on a Tilemap (optional)")]
    public Tilemap doorTilemap;           // tilemap that contains a blocking door tile
    public Vector3Int doorCell;           // cell to clear

    [Header("One-shot settings")]
    public bool destroyObjectAfterOpen = false;

    public void OpenDoor()
    {
        Debug.Log("[Door] OpenDoor called.");

        if (animator) animator.SetTrigger("open");

        if (solidCollider) solidCollider.enabled = false;
        if (sprite) sprite.enabled = false;

        if (doorTilemap)
        {
            doorTilemap.SetTile(doorCell, null);

            // Ensure its collider updates immediately
            var tmc = doorTilemap.GetComponent<TilemapCollider2D>();
            if (tmc) tmc.ProcessTilemapChanges();
        }

        if (destroyObjectAfterOpen)
            Destroy(gameObject);
    }
}
