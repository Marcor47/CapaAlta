using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GroundTile", menuName = "Capa Alta/Ground Tile (autotile)")]
public class GroundTile : TileBase
{
    // Posición de cada pieza en la hoja (8 columnas x 4 filas, de izquierda a derecha y de arriba abajo)
    public const int Columns = 8;
    public const int SlotCount = 32;
    public const int TL = 0, T0 = 1, T1 = 2, T2 = 3, TR = 4, S = 5, FL = 6, FM = 7,
                     L0 = 8, C0 = 9, C1 = 10, C2 = 11, R0 = 12, VT = 13, FR = 14, ICT2 = 15,
                     L1 = 16, C3 = 17, ICTL = 18, ICTR = 19, R1 = 20, VM = 21,
                     BL = 24, B0 = 25, B1 = 26, ICBL = 27, ICBR = 28, BR = 29, VB = 30;

    public Sprite[] sprites = new Sprite[SlotCount];
    public Tile.ColliderType colliderType = Tile.ColliderType.Grid;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        // Al pintar o borrar, se actualizan también los 8 vecinos
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                tilemap.RefreshTile(position + new Vector3Int(dx, dy, 0));
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = tilemap != null ? PickSprite(position, tilemap) : sprites[C0];
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.colliderType = colliderType;
        tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
    }

    bool Has(ITilemap tm, Vector3Int p, int dx, int dy)
        => tm.GetTile(p + new Vector3Int(dx, dy, 0)) == this;

    Sprite PickSprite(Vector3Int p, ITilemap tm)
    {
        bool u = Has(tm, p, 0, 1), d = Has(tm, p, 0, -1);
        bool l = Has(tm, p, -1, 0), r = Has(tm, p, 1, 0);
        bool ul = Has(tm, p, -1, 1), ur = Has(tm, p, 1, 1);
        bool dl = Has(tm, p, -1, -1), dr = Has(tm, p, 1, -1);

        if (!u && !d)
        {
            if (!l && !r) return sprites[S];
            if (!l) return sprites[FL];
            if (!r) return sprites[FR];
            return sprites[FM];
        }
        if (!u)
        {
            if (!l && !r) return sprites[VT];
            if (!l) return sprites[TL];
            if (!r) return sprites[TR];
            return Choose(p, T0, T1, T2);
        }
        if (!d)
        {
            if (!l && !r) return sprites[VB];
            if (!l) return sprites[BL];
            if (!r) return sprites[BR];
            return Choose(p, B0, B1);
        }
        if (!l && !r) return sprites[VM];
        if (!l) return Choose(p, L0, L1);
        if (!r) return Choose(p, R0, R1);
        if (!ul && !ur) return sprites[ICT2];
        if (!ul) return sprites[ICTL];
        if (!ur) return sprites[ICTR];
        if (!dl) return sprites[ICBL];
        if (!dr) return sprites[ICBR];
        return Choose(p, C0, C1, C2, C3);
    }

    // Variante fija por posición: no cambia al repintar y no parpadea
    Sprite Choose(Vector3Int p, params int[] options)
    {
        unchecked
        {
            int h = (p.x * 73856093) ^ (p.y * 19349663);
            int i = ((h % options.Length) + options.Length) % options.Length;
            return sprites[options[i]];
        }
    }
}