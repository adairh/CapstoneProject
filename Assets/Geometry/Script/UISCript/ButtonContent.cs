using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
[ExecuteInEditMode]
public class ButtonContent : MonoBehaviour {

	public Image image;
	public TextMeshProUGUI text;

	private void Start() { 
	}
	void Update () {
		/*var tool = GetComponentInParent<Tool>();
		if(tool == null) return;
		if(image.sprite != tool.icon) image.sprite = tool.icon;
		var richText = tool.GetRichText();
		if(text.text != richText) text.text = richText;*/
	}
}
