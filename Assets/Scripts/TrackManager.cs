using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public static TrackManager trackManager;
    public GameObject wallPrefab;

    public string trackName;
    Track track;

    int iPixels;
    int jPixels;
    Vector2 pixelSize;

    List<List<Color>> colors = new List<List<Color>>();
    List<List<Vector3>> pixelPositions = new List<List<Vector3>>();

    List<GameObject> walls = new List<GameObject>();
    List<GameObject> finalWalls = new List<GameObject>();

    void Start()
    {
        trackManager = this;

        if (trackName == "")
            return;

        LoadTrack(trackName);
    }

    public void BuildTrack()
    {
        GetComponent<Renderer>().material.SetTexture("_MainTex", track.texture);
        Initialize();
        GetColorsAndPositions();
        BuildWalls();
        OptimizeWalls();
    }

    void Initialize()
    {
        // Get the number of pixels in the texture and their size
        iPixels = track.texture.width;
        jPixels = track.texture.height;
        pixelSize = new Vector2(transform.localScale.x * 10 / iPixels, transform.localScale.z * 10 / jPixels);
    }

    // Save all of the pixel colors and position in matrices
    void GetColorsAndPositions()
    {
        // Loop through the texture and add the colors and positions of the pixels to the matrices
        for (int i = 0; i < iPixels; i++)
        {
            colors.Add(new List<Color>());
            pixelPositions.Add(new List<Vector3>());
            for (int j = 0; j < jPixels; j++)
            {
                Vector3 position = new Vector3(pixelSize.x / 2 + pixelSize.x * i, 1, pixelSize.y / 2 + pixelSize.y * j);
                pixelPositions[i].Add(position - Vector3.down);
                colors[i].Add(track.texture.GetPixel(i, j));
            }
        }
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

            if (currentPixel.r - Color.green.r < 0.2f && currentPixel.g - Color.green.g < 0.2f && currentPixel.b - Color.green.b < 0.2f && upPixel.r - 0.5f < 0.3f && upPixel.g - 0.5f < 0.3f && upPixel.b - 0.5f < 0.3f)
                return true;
        }

        if (0 < j - 1)
        {
            Color downPixel = colors[i][j - 1];

            if (currentPixel.r - Color.green.r < 0.2f && currentPixel.g - Color.green.g < 0.2f && currentPixel.b - Color.green.b < 0.2f && downPixel.r - 0.5f < 0.3f && downPixel.g - 0.5f < 0.3f && downPixel.b - 0.5f < 0.3f)
                return true;
        }

        if (colors.Count > i + 1)
        {
            Color rightPixel = colors[i + 1][j];

            if (currentPixel.r - Color.green.r < 0.2f && currentPixel.g - Color.green.g < 0.2f && currentPixel.b - Color.green.b < 0.2f && rightPixel.r - 0.5f < 0.3f && rightPixel.g - 0.5f < 0.3f && rightPixel.b - 0.5f < 0.3f)
                return true;
        }

        if (0 < i - 1)
        {
            Color leftPixel = colors[i - 1][j];

            if (currentPixel.r - Color.green.r < 0.2f && currentPixel.g - Color.green.g < 0.2f && currentPixel.b - Color.green.b < 0.2f && leftPixel.r - 0.5f < 0.3f && leftPixel.g - 0.5f < 0.3f && leftPixel.b - 0.5f < 0.3f)
                return true;
        }

        return diff > 0.1f;
    }

    void BuildWalls()
    {
        for (int i = 0; i < iPixels; i++)
        {
            for (int j = 0; j < jPixels; j++)
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
            wall.transform.position += new Vector3(300, 0, 300);
            wall.transform.rotation = Quaternion.Euler(0, 180, 0);
            finalWalls.Add(wall);
        }
        for (int i = 0; i < walls.Count; i++)
            Destroy(walls[i]);
    }

    Texture2D AntiAlias(Texture2D tex, int aliasSize)
    {
        float r = 0;
        float g = 0;
        float b = 0;

        int count = 0;

        Texture2D newTex = new Texture2D(tex.width, tex.height);

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                count = 0;
                r = 0;
                g = 0;
                b = 0;

                for (int k = i - aliasSize / 2; k < i + aliasSize / 2; k++)
                {
                    if (k < 0 || k > tex.width)
                        continue;

                    for (int l = j - aliasSize / 2; l < j + aliasSize / 2; l++)
                    {
                        if (l < 0 || l > tex.height)
                            continue;

                        count++;
                        Color curPixColor = tex.GetPixel(k, l);

                        if (curPixColor == Color.white)
                        {
                            curPixColor = Color.gray;
                        }

                        r += curPixColor.r;
                        g += curPixColor.g;
                        b += curPixColor.b;
                    }
                }

                newTex.SetPixel(i, j, new Color(r / count, g / count, b / count));

            }
        }
        newTex.Apply();
        return newTex;
    }

    public bool SaveTrack(string trackName)
    {
        List<Vector3> trackPoints = new List<Vector3>();
        track = new Track(trackName, AntiAlias((Texture2D)GetComponent<Renderer>().material.GetTexture("_MainTex"), 3), TexturePainter.instance.trackpoints);
        return SaveableObjects.SaveTrack(track);
    }

    public bool LoadTrack(string trackName)
    {
        track = SaveableObjects.LoadTrack(trackName);
        if (track == null)
            return false;
        else
            BuildTrack();

        return true;

    }

    public Track GetTrack()
    {
        List<Vector3> trackPointsCopy = new List<Vector3>();

        for (int i = 0; i < track.trackPoints.Count; i++)
            trackPointsCopy.Add(track.trackPoints[i].position);

        Track trackCopy = new Track(trackName, track.texture, trackPointsCopy);

        return trackCopy;
    }
}
