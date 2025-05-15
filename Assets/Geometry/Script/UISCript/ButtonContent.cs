using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ButtonContent : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    void Update()
    {
        if (transform.parent == null || text == null) return;

        string richText = transform.parent.name;
        if (text.text != richText)
            text.text = richText;
    }
}
