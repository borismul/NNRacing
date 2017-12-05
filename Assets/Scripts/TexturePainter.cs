using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TexturePainter : MonoBehaviour {

    Texture2D texture;
    Renderer ren;
    int textureWidth = 500;
    int textureHeight = 500;

    public int brushSize = 5;
    public static TexturePainter instance;

    public List<Vector3> trackpoints = new List<Vector3>();
    List<Vector2> hitPixels = new List<Vector2>();
    Ray lastRay;
    bool drawnFinish;
    bool isWhite;
    public bool inMenu;
    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void OnEnable ()
    {
        Reset();

    }

    public void Reset()
    {
        lastRay = new Ray();
        ren = GetComponent<Renderer>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);

        // set the pixel values
        for (int i = 0; i < textureWidth; i++)
        {
            for (int j = 0; j < textureHeight; j++)
            {
                texture.SetPixel(i, j, Color.green);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        ren.material.mainTexture = texture;

        trackpoints.Clear();
        hitPixels.Clear();
        drawnFinish = false;

    }

    // Update is called once per frame
    void Update ()
    {
        if(!inMenu && SceneManager.GetActiveScene().name != "MainScene")
            UpdateTexture();
	}

    void UpdateTexture()
    {
        if (!Input.GetKey(KeyCode.Mouse0) || drawnFinish)
            return;

        Ray rayNow = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        List<Ray> rays = new List<Ray>();
        Vector3 originalDist = (rayNow.origin - lastRay.origin);

        if (lastRay.origin != Vector3.zero && (Physics.Raycast(rayNow, Mathf.Infinity)))
        {
            float lastDist = 50;

            while (true)
            {
                if(rays.Count == 0)
                    rays.Add(new Ray(lastRay.origin + originalDist.normalized * ( 10f/textureWidth), rayNow.direction));
                else
                    rays.Add(new Ray(rays[rays.Count - 1].origin + originalDist.normalized * ( 10f/textureWidth), rayNow.direction));

                if (Vector3.Distance(rays[rays.Count - 1].origin, rayNow.origin) > lastDist || lastDist < 0.00001f)
                {
                    rays.RemoveAt(rays.Count - 1);
                    break;
                }

                lastDist = Vector3.Distance(rays[rays.Count - 1].origin, rayNow.origin);
            }
        }
        else
            rays.Add(rayNow);

        for (int k = 0; k < rays.Count; k++)
        {
            Ray ray = rays[k];
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Texture2D tex = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture;
                Vector2 pixel = new Vector2(hit.textureCoord.x * textureWidth, hit.textureCoord.y * textureHeight);


                for (int i = 0; i < brushSize; i++)
                {
                    for (int j = 0; j < brushSize; j++)
                    {
                        Vector2 currentPixel = new Vector2(pixel.x - brushSize / 2 + i, pixel.y - brushSize / 2 + j);
                        if (Vector2.Distance(pixel, currentPixel) * 2 > brushSize || currentPixel.x > textureWidth || currentPixel.y > textureHeight
                            || currentPixel.x < 0 || currentPixel.y < 0)
                            continue;

                        if (tex.GetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y)) == Color.white)
                            continue;

                        if (tex.GetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y)) == Color.gray)
                            continue;

                        if (tex.GetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y)) == Color.black)
                            continue;

                        tex.SetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y), Color.gray);
                    }
                }
                lastRay = rayNow;

                if (hitPixels.Count == 0 || Vector2.Distance(new Vector2(hit.textureCoord.x * textureWidth, hit.textureCoord.y * textureHeight), hitPixels[hitPixels.Count -1]) > 15)
                {
                    trackpoints.Add(hit.point * 30);
                    hitPixels.Add(new Vector2(hit.textureCoord.x * textureWidth, hit.textureCoord.y * textureHeight));
                }
                if(!drawnFinish && Vector3.Distance(trackpoints[0], hit.point * 30) < 5 && trackpoints.Count > 5)
                {
                    trackpoints.Add(hit.point * 30);
                    hitPixels.Add(new Vector2(hit.textureCoord.x * textureWidth, hit.textureCoord.y * textureHeight));

                    Vector2 pixel1 = hitPixels[hitPixels.Count - 3];
                    Vector2 pixel2 = hitPixels[hitPixels.Count - 1];

                    float angle;
                    if(pixel2.x - pixel1.x > 0)
                        angle = Vector2.Angle(pixel2 - pixel1, Vector2.up);
                    else
                        angle = -Vector2.Angle(pixel2 - pixel1, Vector2.up);

                    for (float i = -brushSize / 2 - 2; i < brushSize / 2 + 2; i += 1f)
                    {
                        for (float j = 0; j < 5; j += 1f)
                        {
                            Vector2 finishpixel = hitPixels[hitPixels.Count - 1] + (hitPixels[hitPixels.Count - 1] - hitPixels[hitPixels.Count - 2])/2 + i * new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), -Mathf.Sin(angle * Mathf.Deg2Rad)) + j * new Vector2(Mathf.Cos((angle + 90) * Mathf.Deg2Rad), -Mathf.Sin((angle + 90) * Mathf.Deg2Rad));

                            if(isWhite)
                                tex.SetPixel(Mathf.FloorToInt(finishpixel.x), Mathf.FloorToInt(finishpixel.y), Color.white);
                            else
                                tex.SetPixel(Mathf.FloorToInt(finishpixel.x), Mathf.FloorToInt(finishpixel.y), Color.black);

                            isWhite = !isWhite;
                        }
                    }
                    drawnFinish = true;
                }
                tex.Apply();



            }
        }

    }


}
