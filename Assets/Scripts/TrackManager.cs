using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public static TrackManager trackManager;

    public GameObject[] trees;

    public GameObject[] bushes;

    public GameObject[] grass;

    public float treeChance;

    public float bushChance;

    public float grassChance;

    public string trackName;

    Track track;

    int iPixels;
    int jPixels;
    Vector2 pixelSize;

    List<List<Color>> colors = new List<List<Color>>();

    List<GameObject> walls = new List<GameObject>();
    List<GameObject> finalWalls = new List<GameObject>();

    Vector2 localScale = new Vector2();

    bool[,] map;

    List<GameObject> fancyObjects = new List<GameObject>();

    Texture2D curTex;

    void Awake()
    {
        trackManager = this;
    }

    void BuildTrack(bool fancy)
    {
        if (trackName == "")
            return;

        if (curTex != null)
            Destroy(curTex);

        RemoveFancyObjects();

        track = SaveableObjects.LoadTrack(trackName);
        curTex = track.texture;

        localScale.x = transform.localScale.x;
        localScale.y = transform.localScale.z;

        GetComponent<Renderer>().material.SetTexture("_MainTex", track.texture);
        Initialize();
        GetColors();
        CreateMap();

        if (fancy)
            FancyUp();
        //CreateWall();
        //OptimizeWalls();

    }

    void Initialize()
    {
        // Get the number of pixels in the texture and their size
        iPixels = track.texture.width;
        jPixels = track.texture.height;
        pixelSize = new Vector2(transform.localScale.x * 10 / iPixels, transform.localScale.z * 10 / jPixels);
    }

    // Save all of the pixel colors and position in matrices
    void GetColors()
    {
        colors.Clear();
        // Loop through the texture and add the colors and positions of the pixels to the matrices
        for (int i = 0; i < iPixels; i++)
        {
            colors.Add(new List<Color>());
            for (int j = 0; j < jPixels; j++)
            {
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
            Color otherPixel = colors[i][j + 1];
            if (Mathf.Abs(currentPixel.r - Color.green.r) < 0.2f && Mathf.Abs(currentPixel.g - Color.green.g) < 0.2f && Mathf.Abs(currentPixel.b - Color.green.b) < 0.2f && (Mathf.Abs(otherPixel.r - Color.green.r) > 0.2f || Mathf.Abs(otherPixel.g - Color.green.g) > 0.2f || Mathf.Abs(otherPixel.b - Color.green.b) > 0.2f))
                return true;
        }

        if (0 < j - 1)
        {
            Color otherPixel = colors[i][j - 1];
            if (Mathf.Abs(currentPixel.r - Color.green.r) < 0.2f && Mathf.Abs(currentPixel.g - Color.green.g) < 0.2f && Mathf.Abs(currentPixel.b - Color.green.b) < 0.2f && (Mathf.Abs(otherPixel.r - Color.green.r) > 0.2f || Mathf.Abs(otherPixel.g - Color.green.g) > 0.2f || Mathf.Abs(otherPixel.b - Color.green.b) > 0.2f))
                return true;
        }

        if (colors.Count > i + 1)
        {
            Color otherPixel = colors[i + 1][j];

            if (Mathf.Abs(currentPixel.r - Color.green.r) < 0.2f && Mathf.Abs(currentPixel.g - Color.green.g) < 0.2f && Mathf.Abs(currentPixel.b - Color.green.b) < 0.2f && (Mathf.Abs(otherPixel.r - Color.green.r) > 0.2f || Mathf.Abs(otherPixel.g - Color.green.g) > 0.2f || Mathf.Abs(otherPixel.b - Color.green.b) > 0.2f))
                return true;
        }

        if (0 < i - 1)
        {
            Color otherPixel = colors[i - 1][j];

            if (Mathf.Abs(currentPixel.r - Color.green.r) < 0.2f && Mathf.Abs(currentPixel.g - Color.green.g) < 0.2f && Mathf.Abs(currentPixel.b - Color.green.b) < 0.2f && (Mathf.Abs(otherPixel.r - Color.green.r) > 0.2f || Mathf.Abs(otherPixel.g - Color.green.g) > 0.2f || Mathf.Abs(otherPixel.b - Color.green.b) > 0.2f))
                return true;
        }

        return diff > 0.1f;
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
            //wall.transform.position += new Vector3(300, 0, 300);
            //wall.transform.rotation = Quaternion.Euler(0, 180, 0);
            finalWalls.Add(wall);
        }
        for (int i = 0; i < walls.Count; i++)
            Destroy(walls[i]);
    }

    void CreateWall()
    {
        List<List<bool>> donePixel = new List<List<bool>>();

        for (int i = 0; i < iPixels; i++)
        {
            donePixel.Add(new List<bool>());
            for (int j = 0; j < jPixels; j++)
                donePixel[i].Add(false);
        }

        Vector2 curPixel = Vector2.zero;
        for (int i = 0; i < iPixels; i++)
        {
            for (int j = 0; j < jPixels; j++)
            {
                if (hasPixelWall(i, j) && !donePixel[i][j])
                {
                    curPixel = new Vector2(i, j);

                    BuildWallPiece(ref donePixel, curPixel);
                }
            }
        }



    }

    void BuildWallPiece(ref List<List<bool>> donePixel, Vector2 curPixel)
    {
        GameObject obj = new GameObject();
        walls.Add(obj);
        Mesh mesh = obj.AddComponent<MeshFilter>().mesh;
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshCollider>();
        List<Vector3> vert = new List<Vector3>();
        List<int> tris = new List<int>();
        Vector2 nextPixel = new Vector2(-1, -1);

        Vector2 prevPixel = new Vector2(-1, -1);

        vert.Add(Pixel2Position((int)curPixel.x,(int)curPixel.y) - Vector3.down * 15);
        vert.Add(Pixel2Position((int)curPixel.x, (int)curPixel.y) + Vector3.down * 15);

        int zero;
        int one;
        int two;
        int three;

        while (!donePixel[(int)curPixel.x][(int)curPixel.y])
        {
            donePixel[(int)curPixel.x][(int)curPixel.y] = true;

            int i = (int)curPixel.x;
            int j = (int)curPixel.y;

            nextPixel.x = -1;
            nextPixel.y = -1;

            GetNextPixel(ref donePixel,ref nextPixel, i, j);

            if (((int)nextPixel.x == -1 && (int)nextPixel.y == -1) && ((int)prevPixel.x != -1 && (int)prevPixel.y != -1))
            {
                GetNextPixel(ref donePixel, ref nextPixel, (int)prevPixel.x, (int)prevPixel.y);
            }

            if ((int)nextPixel.x == -1 && (int)nextPixel.y == -1)
                break;

            int iNew = (int)nextPixel.x;
            int jNew = (int)nextPixel.y;

            vert.Add(Pixel2Position(iNew, jNew) - Vector3.down * 15);
            vert.Add(Pixel2Position(iNew, jNew) + Vector3.down * 15);

            zero = vert.Count - 4;
            one = vert.Count - 3;
            two = vert.Count - 2;
            three = vert.Count - 1;

            tris.Add(zero);
            tris.Add(one);
            tris.Add(three);

            tris.Add(zero);
            tris.Add(three);
            tris.Add(two);

            tris.Add(zero);
            tris.Add(three);
            tris.Add(one);

            tris.Add(zero);
            tris.Add(two);
            tris.Add(three);

            prevPixel = curPixel;
            curPixel = nextPixel;
        }

        zero = 0;
        one = 1;
        two = vert.Count - 2;
        three = vert.Count - 1;

        tris.Add(zero);
        tris.Add(one);
        tris.Add(three);

        tris.Add(zero);
        tris.Add(three);
        tris.Add(two);

        tris.Add(zero);
        tris.Add(three);
        tris.Add(one);

        tris.Add(zero);
        tris.Add(two);
        tris.Add(three);

        mesh.vertices = vert.ToArray();
        mesh.triangles = tris.ToArray();

    }

    void GetNextPixel(ref List<List<bool>> donePixel, ref Vector2 nextPixel, int i, int j)
    {
        bool done = false;

        for (int a = -1; a < 2; a++)
        {
            for (int b = -1; b < 2; b++)
            {
                if ((a == 0 && b == 0) || donePixel[i + a][j + b])
                    continue;

                if (hasPixelWall(i + a, j + b))
                {
                    nextPixel.x = i + a;
                    nextPixel.y = j + b;
                    done = true;
                    break;
                }

                if (done)
                    break;
            }
        }
    }

    void CreateMap()
    {
        map = new bool[iPixels, jPixels];
        for (int i = 0; i < iPixels; i++)
        {
            for (int j = 0; j < jPixels; j++)
            {
                if (Mathf.Abs(colors[i][j].r - Color.green.r) < 0.1f && Mathf.Abs(colors[i][j].g - Color.green.g) < 0.1f && Mathf.Abs(colors[i][j].b - Color.green.b) < 0.1f)
                    map[i, j] = false;
                else
                    map[i, j] = true;

            }
        }
    }

    bool CanHoldItem(int iPixel, int jPixel)
    {
        for(int i = -2; i < 3; i++)
        {
            for(int j = -2; j < 3; j++)
            {

                if (i + iPixel < 0)
                    continue;

                if (i + iPixel > iPixels - 1)
                    continue;

                if (j + jPixel < 0)
                    continue;

                if (j + jPixel > jPixels - 1)
                    continue;

                if (!HasGrass(new Vector2Int(i + iPixel, j + jPixel)))
                    return false;
                    
            }
        }
        return true;
    }

    void FancyUp()
    {

        for(int i = 0; i < iPixels; i++)
        {
            for(int j = 0; j < jPixels; j++)
            {
                if (CanHoldItem(i,j))
                {
                    float rand = Random.Range(0f, 1f);

                    if(rand < treeChance)
                    {
                        int num = Random.Range(0, trees.Length);
                        float rot = Random.Range(0f, 360f);
                        float scale = Random.Range(0.9f, 1.1f);
                        GameObject obj = Instantiate(trees[num], Pixel2Position(new Vector2Int(i, j)), Quaternion.Euler(0,rot,0));
                        obj.transform.localScale = new Vector3(scale, scale, scale);

                        fancyObjects.Add(obj);

                    }
                    else if(rand - treeChance < bushChance)
                    {
                        int num = Random.Range(0, bushes.Length);
                        float rot = Random.Range(0f, 360f);
                        float scale = Random.Range(0.1f, 0.6f);
                        GameObject obj = Instantiate(bushes[num], Pixel2Position(new Vector2Int(i, j)), Quaternion.Euler(0, rot, 0));

                        obj.transform.localScale = new Vector3(scale, scale, scale);
                        fancyObjects.Add(obj);

                    }
                    else if (rand - treeChance - bushChance < grassChance)
                    {
                        int num = Random.Range(0, grass.Length);
                        float rot = Random.Range(0f, 360f);
                        float scale = Random.Range(1f, 2f);
                        GameObject obj = Instantiate(grass[num], Pixel2Position(new Vector2Int(i, j)), Quaternion.Euler(0, rot, 0));

                        obj.transform.localScale = new Vector3(scale, scale, scale);
                        fancyObjects.Add(obj);

                    }

                }
            }
        }

    }

    Vector3 Pixel2Position(int pixelI, int pixelJ)
    {
        float x = -(pixelSize.x / 2 + pixelSize.x * pixelI) + transform.localScale.x * 10;
        float y = 0;
        float z = -(pixelSize.y / 2 + pixelSize.y * pixelJ) + transform.localScale.z * 10;
        return new Vector3(x, y, z);
    }

    Vector3 Pixel2Position(Vector2Int pixel)
    {
        float x = -(pixelSize.x / 2 + pixelSize.x * pixel.x) + localScale.x * 10;
        float y = 0;
        float z = -(pixelSize.y / 2 + pixelSize.y * pixel.y) + localScale.y * 10;
        return new Vector3(x, y, z);
    }

    Vector2Int Position2Pixel(Vector3 position)
    {
        int x = -Mathf.RoundToInt((position.x - localScale.x * 10 + pixelSize.x / 2) / pixelSize.x);
        int y = -Mathf.RoundToInt((position.z - localScale.y * 10 + pixelSize.y / 2) / pixelSize.y);

        return new Vector2Int(x, y);

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
        track = new Track(trackName, (Texture2D)GetComponent<Renderer>().material.GetTexture("_MainTex"), TexturePainter.instance.trackpoints);
        return SaveableObjects.SaveTrack(track);
    }

    public bool LoadTrack(bool fancy)
    {

        BuildTrack(fancy);
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

    public bool HasGrass(Vector2Int pixel)
    {
        if (pixel.x >= iPixels || pixel.x < 0 || pixel.y >= jPixels || pixel.y < 0)
            return true;

        return !map[pixel.x, pixel.y];
    }

    public bool HasGrass(Vector3 position)
    {
        Vector2Int pixelPos = Position2Pixel(position);

        if (pixelPos.x >= iPixels || pixelPos.x < 0 || pixelPos.y >= jPixels || pixelPos.y < 0)
            return true;


        return !map[pixelPos.x, pixelPos.y];
    }

    public static float WallDistance(Vector3 position, Quaternion objRotation, float angle)
    {
        Vector2Int currentPixel = trackManager.Position2Pixel(position);
        float finalAngle = objRotation.eulerAngles.y + angle - 45;

        Vector2 direction = (new Vector3(Mathf.Cos(finalAngle * Mathf.Deg2Rad) + Mathf.Sin(finalAngle * Mathf.Deg2Rad), Mathf.Cos(finalAngle * Mathf.Deg2Rad) - Mathf.Sin(finalAngle * Mathf.Deg2Rad))).normalized * trackManager.pixelSize.x;

        float slope = Mathf.Tan(finalAngle * Mathf.Deg2Rad);

        Vector2 prevPixel = currentPixel;
        Vector2 intermediate = currentPixel;
        bool hasGrass = false;
        int multiplier = 20;
        int count = 0;

        while(!hasGrass)
        {
            prevPixel = intermediate;

            if (count < 20)
                intermediate -= direction * 2;
            else
                intermediate -= direction * multiplier;

            currentPixel.x = (int)(intermediate.x);
            currentPixel.y = (int)(intermediate.y);

            if (currentPixel.x >= trackManager.iPixels)
                currentPixel.x = trackManager.iPixels - 1;
            else if (currentPixel.x < 0)
                currentPixel.x = 0;

            if (currentPixel.y >= trackManager.jPixels)
                currentPixel.y = trackManager.jPixels - 1;
            else if (currentPixel.y < 0)
                currentPixel.y = 0;
            hasGrass = trackManager.HasGrass(currentPixel);
            if (hasGrass && multiplier > 4 && count >= 20)
            {
                hasGrass = false;
                intermediate = prevPixel;
                multiplier /= 3;
            }
            count += 1;
        }
        //Debug.DrawLine(position, trackManager.Pixel2Position(currentPixel));

        return Vector3.Distance(trackManager.Pixel2Position(currentPixel), position);
    }

    void RemoveFancyObjects()
    {
        for(int i = 0; i < fancyObjects.Count; i++)
        {
            Destroy(fancyObjects[i]);
        }
    }


}
