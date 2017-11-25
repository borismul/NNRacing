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

    Ray lastRay;

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


    }

    // Update is called once per frame
    void Update ()
    {
        if(!inMenu && SceneManager.GetActiveScene().name != "MainScene")
            UpdateTexture();
	}

    void UpdateTexture()
    {
        if (!Input.GetKey(KeyCode.Mouse0))
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

                        tex.SetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y), Color.gray);
                    }
                }
                tex.Apply();
                lastRay = rayNow;
                if (tex.GetPixel(Mathf.FloorToInt(pixel.x), Mathf.FloorToInt(pixel.y)) != Color.white)
                {
                    trackpoints.Add(hit.point*30);
                    tex.SetPixel(Mathf.FloorToInt(pixel.x), Mathf.FloorToInt(pixel.y), Color.white);

                }

            }
        }

    }


}
