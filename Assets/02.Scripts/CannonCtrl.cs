using UnityEngine;

public class CannonCtrl : MonoBehaviour
{
    Transform tr;
    public float rotSpeed = 50.0f;
    public float minAngle = -8.0f; //포신을 내릴 수 있는 한계(도)
    public float maxAngle = 25.0f; //포신을 올릴 수 있는 한계(도)
    float pitch;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        //휠을 올리면 포신이 올라가고, 제한 각도 안으로 고정
        pitch = Mathf.Clamp(pitch + Input.GetAxis("Mouse ScrollWheel") * rotSpeed, minAngle, maxAngle);
        tr.localRotation = Quaternion.Euler(-pitch, 0, 0);
    }
}
