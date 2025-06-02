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
        
        //transform.parent.GetComponent<Image>().enabled = false;
        /*transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        image.enabled = true;
        image.sprite = sprite;
        transform.parent.localScale = new Vector3(1f, 1f, 1f); */
        /*
        transform.parent.GetComponent<Image>().enabled = true;
        transform.parent.GetComponent<Image>().sprite = sprite;
        transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.8f);
        transform.parent.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
        image.enabled = false;*/
        
    } 
}