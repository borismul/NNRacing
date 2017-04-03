using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePainter : MonoBehaviour {

    Texture2D texture;
    Renderer ren;
    int textureWidth = 500;
    int textureHeight = 500;

    public int brushSize = 5;

    public static TexturePainter instance;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        ren = GetComponent<Renderer>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, true);

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

    }

    // Update is called once per frame
    void Update () {
        UpdateTexture();
	}


    void UpdateTexture()
    {
        if (!Input.GetKey(KeyCode.Mouse0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Texture2D tex = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture;
            Vector2 pixel = new Vector2(hit.textureCoord.x * textureWidth, hit.textureCoord.y * textureHeight);

            for(int i = 0; i < brushSize; i++)
            {
                for(int j = 0; j < brushSize; j++)
                {
                    Vector2 currentPixel = new Vector2(pixel.x - brushSize / 2 + i, pixel.y - brushSize / 2 + j);
                    if (Vector2.Distance(pixel, currentPixel) * 2 > brushSize || currentPixel.x > textureWidth || currentPixel.y > textureHeight
                        || currentPixel.x < 0 || currentPixel.y < 0)
                        continue;


                    tex.SetPixel(Mathf.FloorToInt(currentPixel.x), Mathf.FloorToInt(currentPixel.y), Color.red);
                }
            }
            tex.Apply();
        }
    }
}
