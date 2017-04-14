using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LevelBuilder : MonoBehaviour
{
    Texture2D texture;

    int iPixels;
    int jPixels;

    Vector2 pixelSize;

    public GameObject wallPrefab;

    List<List<Color>> colors = new List<List<Color>>();

    List<List<Vector3>> pixelPositions = new List<List<Vector3>>();
    List<GameObject> walls = new List<GameObject>();
    List<GameObject> finalWalls = new List<GameObject>();

    SaveableObjects.SavedTrack currentTrack;

    public void BuildTrack()
    {
        Initialize();
        GetColorsAndPositions();
        BuildWalls();
        OptimizeWalls();
    }

    void Initialize()
    {
        texture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        iPixels = texture.width;
        jPixels = texture.height;
        pixelSize = new Vector2(transform.localScale.x * 10 / iPixels, transform.localScale.z * 10 / jPixels);
    }

    void GetColorsAndPositions()
    {
        for (int i = 0; i < iPixels; i++)
        {
            colors.Add(new List<Color>());
            pixelPositions.Add(new List<Vector3>());
            for (int j = 0; j < jPixels; j++)
            {
                Vector3 position = new Vector3(pixelSize.x / 2 + pixelSize.x * i, 1, pixelSize.y / 2 + pixelSize.y * j);
                RaycastHit hit;
                Physics.Raycast(position, Vector3.down, out hit, 1.5f);
                Vector2 pixel = new Vector2(hit.textureCoord.x * iPixels, hit.textureCoord.y * jPixels);
                pixelPositions[i].Add(hit.point);

                colors[i].Add(texture.GetPixel(Mathf.FloorToInt(pixel.x), Mathf.FloorToInt(pixel.y)));
            }
        }
    }

    void BuildWalls()
    {
        for (int i = 0; i < iPixels; i++)
        {
            for(int j = 0; j < jPixels; j++)
            {
                if (hasPixelWall(i, j))
                {
                    GameObject wall = Instantiate(wallPrefab, pixelPositions[i][j] + new Vector3(0, 0.5f, 0), Quaternion.identity);
                    wall.transform.localScale = new Vector3(pixelSize.x, wall.transform.localScale.y, pixelSize.y);
                    walls.Add(wall);
                }
            }
        }
    }

    void OptimizeWalls()
    {
        List<List<CombineInstance>> combineInstances = new List<List<CombineInstance>>();

        int currentVert = 0;

        combineInstances.Add(new List<CombineInstance>());
        for (int i = 0; i < walls.Count; i++)
        {
            if (currentVert > 60000)
            {
                combineInstances.Add(new List<CombineInstance>());
                currentVert = 0;
            }


            MeshFilter meshFilter = walls[i].GetComponent<MeshFilter>();
            CombineInstance instance = new CombineInstance();
            instance.mesh = meshFilter.sharedMesh;
            instance.transform = meshFilter.transform.localToWorldMatrix;
            combineInstances[combineInstances.Count - 1].Add(instance);

            currentVert += meshFilter.mesh.vertexCount;
        }

        for (int i = 0; i < combineInstances.Count; i++)
        {
            Mesh wallMesh = new Mesh();
            wallMesh.CombineMeshes(combineInstances[i].ToArray());
            GameObject wall = new GameObject();
            wall.AddComponent<MeshFilter>().mesh = wallMesh;
            wall.AddComponent<MeshRenderer>();
            wall.AddComponent<MeshCollider>().sharedMesh = wallMesh;
            wall.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            wall.layer = 8;
            finalWalls.Add(wall);
        }
        for (int i = 0; i < walls.Count; i++)
            Destroy(walls[i]);
    }

    bool hasPixelWall(int i, int j)
    {
        float diff = 0;

        Color currentPixel = colors[i][j];

        if (currentPixel == Color.gray)
            return false;

        if (colors[i].Count > j + 1)
        {
            Color upPixel = colors[i][j + 1];

            if (currentPixel == Color.green && upPixel.r - 0.5f < 0.1f && upPixel.r - 0.5f < 0.1f && upPixel.g - 0.5f < 0.1f && upPixel.b - 0.5f < 0.1f)
                return true;
        }

        if (0 < j - 1)
        {
            Color downPixel = colors[i][j - 1];

            if (currentPixel == Color.green && downPixel.r - 0.5f < 0.1f && downPixel.r - 0.5f < 0.1f && downPixel.g - 0.5f < 0.1f && downPixel.b - 0.5f < 0.1f)
                return true;
        }

        if (colors.Count > i + 1)
        {
            Color rightPixel = colors[i + 1][j];

            if (currentPixel == Color.green && rightPixel.r - 0.5f < 0.1f && rightPixel.r - 0.5f < 0.1f && rightPixel.g - 0.5f < 0.1f && rightPixel.b - 0.5f < 0.1f)
                return true;
        }

        if (0 < i - 1)
        {
            Color leftPixel = colors[i - 1][j];

            if (currentPixel == Color.green && leftPixel.r - 0.5f < 0.1f && leftPixel.r - 0.5f < 0.1f && leftPixel.g - 0.5f < 0.1f && leftPixel.b - 0.5f < 0.1f)
                return true;
        }

        return diff > 0.1f;
    }

    public bool SaveTrack(string name)
    {
        if (TrackPoints.instance == null)
            return false;

        texture = (Texture2D)GetComponent<Renderer>().material.mainTexture;
        SaveableObjects.SavedTrack track = new SaveableObjects.SavedTrack(name, texture, TrackPoints.instance);
        BinaryFormatter bf = new BinaryFormatter();
        Directory.CreateDirectory(Application.persistentDataPath + "/Tracks/");
        FileStream file = File.Create(Application.persistentDataPath + "/Tracks/" + track.name + ".trk");
        bf.Serialize(file, track);
        file.Close();

        return true;
    }

    public Texture2D LoadTrackMenu(string name)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/Tracks/" + name + ".trk", FileMode.Open);

        currentTrack = (SaveableObjects.SavedTrack)bf.Deserialize(file);
        file.Close();


        Initialize();

        Texture2D tex = new Texture2D(currentTrack.width, currentTrack.height);

        for (int i = 0; i < currentTrack.width; i++)
        {
            for (int j = 0; j < currentTrack.height; j++)
            {
                tex.SetPixel(i, j, new Color(currentTrack.textureR[i * currentTrack.width + j], currentTrack.textureG[i * currentTrack.width + j], currentTrack.textureB[i * currentTrack.width + j], currentTrack.textureA[i * currentTrack.width + j]));
            }
        }
        tex.Apply();
        DestroyTrackPoints();
        for (int i = 0; i < currentTrack.trackPoints.Length; i += 10)
        {
            GameObject trackPoint = new GameObject("TrackPoint");
            trackPoint.transform.parent = TrackPoints.instance.transform;
            trackPoint.transform.position = currentTrack.trackPoints[i].GetVector3() * transform.localScale.x;
        }
        return tex;

    }

    void DestroyTrackPoints()
    {
        for (int i = 0; i < TrackPoints.instance.transform.childCount; i++)
        {
            GameObject child = TrackPoints.instance.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    public void LoadTrack(string name)
    {
        texture = LoadTrackMenu(name);
        texture.filterMode = FilterMode.Point;
        GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)texture);
        BuildTrack();
    }

}

