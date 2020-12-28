using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 CameraRotation = Vector3.zero;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();


    }

    public void Move(Vector3 _Velocity)
    {
        velocity = _Velocity;
        
    }

    public void Rotate(Vector3 _Rotate)
    {
        rotation = _Rotate;
    }

    public void CameraRotate(Vector3 _camRotate)
    {
        CameraRotation = _camRotate;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    //Perform movement based on vel
    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    private void PerformRotation()
    {
        if (rotation != Vector3.zero)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
            if (cam != null)
            {
                cam.transform.Rotate(-CameraRotation);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
