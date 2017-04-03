using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorUIController : MonoBehaviour {


    public InputField brushSizeInput;
    public Slider brushSizeSlider;

    TexturePainter painter;
	// Use this for initialization
	void Start ()
    {
        painter = TexturePainter.instance;
        brushSizeInput.onValueChanged.AddListener(BrushSizeInput);
        brushSizeSlider.onValueChanged.AddListener(BrushSizeSlider);
        brushSizeInput.text = "30";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void BrushSizeInput(string input)
    {
        int par = int.Parse(input);

        if (par > 100)
            par = 100;

        brushSizeSlider.value = par;
        painter.brushSize = par;
    }

    void BrushSizeSlider(float value)
    {
        int par = (int)value;
        brushSizeInput.text = par.ToString();
        painter.brushSize = par;
    }
}
