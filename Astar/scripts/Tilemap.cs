using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using Zentronic;

public class Tilemap : MonoBehaviour
{
    public string mapName;

    public int width;
    public int height;

    public float gridWidth;
    public float gridHeight;

    public float z;

    // tile template that will be instantiated
    public GameObject tileTemplate;

    [Serializable]
    public class SpriteDefinitions
    {
        public int code;
        public char stringRep;
        public Sprite sprite;
        public Color color;
    };

    public List<SpriteDefinitions> spriteDefinitions;

    Tile[] tiles;

    public Vector3 TilePosToLocalPos(int X, int Y, float Z)
    {
        Vector3 localPos = Vector3.zero;

        localPos.x = -((width - 1) * gridWidth * 0.5f) + X * gridWidth;
        localPos.y = ((height - 1) * gridHeight * 0.5f) - Y * gridHeight;
        localPos.z = Z;

        return localPos;
    }

    public Vector3 TilePosToLocalPos(float X, float Y, float Z)
    {
        Vector3 localPos = Vector3.zero;

        localPos.x = -((width - 1) * gridWidth * 0.5f) + X * gridWidth;
        localPos.y = ((height - 1) * gridHeight * 0.5f) - Y * gridHeight;
        localPos.z = Z;

        return localPos;
    }


    public Vec3di LocalPosToTilePos(Vector3 local)
    {
        Vec3di tilePos = new Vec3di();

        tilePos.x = (int)((local.x + gridWidth * 0.5f + (width - 1) * gridWidth * 0.5f) / gridWidth);
        tilePos.y = (int)((-local.y + gridHeight * 0.5f + (height - 1) * gridHeight * 0.5f) / gridHeight);
        tilePos.z = 0;

        if (tilePos.x < 0) tilePos.x = 0;
        if (tilePos.x >= width) tilePos.x = width - 1;
        if (tilePos.y < 0) tilePos.y = 0;
        if (tilePos.y >= height) tilePos.y = height - 1;

        return tilePos;
    }

    public Dictionary<char, int> ValueLookup {
        get {
            Dictionary<char, int> dic = new Dictionary<char, int>();

            foreach (SpriteDefinitions def in spriteDefinitions) {
                dic[def.stringRep] = def.code;
            }

            return dic;
        }
    }

    void CreateTiles()
    {
        tiles = new Tile[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                GameObject g = Instantiate(tileTemplate);
                Tile t = g.GetComponent<Tile>();
                tiles[y * width + x] = t;
                t.transform.parent = this.transform;
                t.name = mapName + "_" + x + "_" + y;

                t.transform.localPosition = TilePosToLocalPos(x, y, z);

            }
        }
    }

    public void ApplyMap(Grid3di m, int z)
    {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Tile t = tiles[y * width + x];
                t.code = m.GetAt(x, y, z, m.BorderCode);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        CreateTiles();
    }

    Dictionary<int, SpriteDefinitions> spriteLookup = new Dictionary<int, SpriteDefinitions>();

    void CreateSpriteLookup()
    {
        foreach (SpriteDefinitions def in spriteDefinitions) {
            spriteLookup[def.code] = def;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CreateSpriteLookup();

        for (int y = 0; y < height; y++) {
            int idx0 = y * width;

            for (int x = 0; x < width; x++) {
                int idx = idx0 + x;

                Tile t = tiles[idx];

                if (t != null) {
                    // t.transform.localPosition = TilePosToLocalPos(x,y,z);
                    if (spriteLookup.ContainsKey(t.code)) {
                        SpriteDefinitions def = spriteLookup[t.code];

                        if (t.spriteRenderer.sprite != def.sprite) {
                            t.spriteRenderer.sprite = def.sprite;
                        }

                        if (t.spriteRenderer.color != def.color) {
                            t.spriteRenderer.color = def.color;
                        }

                        if (t.gameObject.activeSelf == false) {
                            t.gameObject.SetActive(true);
                        }
                    } else {
                        if (t.gameObject.activeSelf == true) {
                            t.spriteRenderer.sprite = null;
                            t.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
