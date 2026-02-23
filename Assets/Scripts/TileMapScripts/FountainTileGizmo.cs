using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class FountainTileGizmo : MonoBehaviour
{
    [Header("Tile (optional)")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase fountainTile; // drag your Animated Tile here
    [SerializeField] private bool autoFindTilePosition;
    [SerializeField] private Vector3Int tilePosition;

    [Header("Gizmo")]
    [SerializeField] private bool useCircleCollider2D = true;
    [SerializeField] private float radius = 1f;
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Healing")]
    [SerializeField] private bool enableHealing;
    [SerializeField] private int healPerSecond = 2;
    [SerializeField] private float tickSeconds = 0.5f;

    private float _nextHealTime;

    private void OnEnable()
    {
        if (autoFindTilePosition)
        {
            AutoFindIfNeeded();
        }
    }

    private void Reset()
    {
        if (tilemap == null)
        {
            tilemap = FindFirstObjectByType<Tilemap>();
        }
    }

    private void OnValidate()
    {
        if (tickSeconds <= 0f) tickSeconds = 0.1f;
        if (radius < 0f) radius = 0f;

        if (autoFindTilePosition)
        {
            AutoFindIfNeeded();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        var (center, r) = GetGizmoCircle();
        Gizmos.DrawWireSphere(center, r);
    }

    private (Vector3 center, float radius) GetGizmoCircle()
    {
        if (useCircleCollider2D && TryGetComponent<CircleCollider2D>(out var circle))
        {
            var center = transform.TransformPoint(circle.offset);
            var scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            var r = Mathf.Abs(circle.radius * scale);
            return (center, r);
        }

        if (tilemap != null)
        {
            return (tilemap.GetCellCenterWorld(tilePosition), Mathf.Max(0f, radius));
        }

        return (transform.position, Mathf.Max(0f, radius));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!enableHealing || healPerSecond <= 0 || tickSeconds <= 0f)
        {
            return;
        }

        if (Time.time < _nextHealTime)
        {
            return;
        }

        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        var amount = Mathf.Max(1, Mathf.CeilToInt(healPerSecond * tickSeconds));
        playerHealth.ChangeHealth(amount);
        _nextHealTime = Time.time + tickSeconds;
    }

    private static bool TryFindFirstTilePosition(Tilemap map, TileBase tile, out Vector3Int position)
    {
        foreach (var pos in map.cellBounds.allPositionsWithin)
        {
            if (map.GetTile(pos) == tile)
            {
                position = pos;
                return true;
            }
        }

        position = default;
        return false;
    }

    private void AutoFindIfNeeded()
    {
        if (fountainTile == null)
        {
            return;
        }

        if (tilemap != null && TryFindFirstTilePosition(tilemap, fountainTile, out var foundInAssigned))
        {
            tilePosition = foundInAssigned;
            return;
        }

        if (TryFindFirstTileInAnyTilemap(fountainTile, out var foundMap, out var foundPos))
        {
            tilemap = foundMap;
            tilePosition = foundPos;
        }
    }

    private static bool TryFindFirstTileInAnyTilemap(TileBase tile, out Tilemap foundMap, out Vector3Int foundPos)
    {
        var maps = FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var map in maps)
        {
            if (map == null) continue;

            if (TryFindFirstTilePosition(map, tile, out var pos))
            {
                foundMap = map;
                foundPos = pos;
                return true;
            }
        }

        foundMap = null;
        foundPos = default;
        return false;
    }
}