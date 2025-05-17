using UnityEngine;

public class MaterialMover : MonoBehaviour
{
    public float scrollSpeed = 0.5F;
    public Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void Update()
    {
        var offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}