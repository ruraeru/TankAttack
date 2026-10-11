using UnityEngine;

public class CannonCtrl : MonoBehaviour
{
    Transform tr;
    public float rotSpeed = 100.0f;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        float angle = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * rotSpeed;
        tr.Rotate(angle, 0, 0);
    }
}
