using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ButtonContent : MonoBehaviour
{
    public Image image;
    public Sprite sprite;
    public TextMeshProUGUI text;

    private void Update()
    {
        if (transform.parent == null || text == null) return;

        var richText = transform.parent.name;
        if (text.text != richText)
            text.text = richText;
        
        image.sprite = sprite;
    } 
}