using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}