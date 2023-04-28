using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TilemapTo3D : MonoBehaviour
{
    [System.Serializable]
    public class Tile3DObject
    {
        public Object obj;
        public float rotAngle;
    }

    [System.Serializable]
    public class Tile3DEntry
    {
        public Sprite targetSprite;
        public bool hasFloor = true;
        public List<Tile3DObject> objects = new List<Tile3DObject>();

    }

    public Tilemap tilemap;
    public Object floorPrefab;
    public List<Tile3DEntry> tileEntries = new List<Tile3DEntry>();

    [ContextMenu("Generate Map")]
    public void SetupMap()
    {
        GameObject mapObj = new GameObject("Level");

        foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(cellPos))
            {
                Vector3 tileWorldPos = tilemap.CellToWorld(cellPos) + tilemap.transform.position;
                Sprite tileSprite = tilemap.GetSprite(cellPos);
                Vector3 tileRotation = tilemap.GetTransformMatrix(cellPos).rotation.eulerAngles;

                var match = tileEntries.Where(x => x.targetSprite == tileSprite);
                // If the tile sprite matches one of our 3d entries, spawn each object for the matching entry
                if (match == null) { continue; }
                
                foreach(Tile3DEntry entryMatch in match) {
                    // Case for floor
                    if(entryMatch.hasFloor) {
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
                        obj.transform.parent = mapObj.transform;
                        obj.transform.position = tileWorldPos;
                    }
                    // Other objects for the entry
                    foreach (Tile3DObject tileObj in entryMatch.objects) {
                        Quaternion objRotation = Quaternion.Euler(0, tileObj.rotAngle - tileRotation.z + 180, 0);
                        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(tileObj.obj);
                        obj.transform.rotation = objRotation;
                        obj.transform.parent = mapObj.transform;
                        obj.transform.position = tileWorldPos;
                    }
                }
            }
        }
    }
}
