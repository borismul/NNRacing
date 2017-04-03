using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateWalls : MonoBehaviour {

    Mesh wallMesh;
    Texture2D texture;

	void Awake()
    {
        texture = GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
    }

    void Analyse()
    {
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {

            }
        }
    }
}
