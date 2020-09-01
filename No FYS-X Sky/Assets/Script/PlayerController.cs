using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float mouseSensitivity = 100f;

    public GameObject plane;



    private PlayerMotor motor;

    void Start()
    {
      
        motor = GetComponent<PlayerMotor>();
        Cursor.lockState = CursorLockMode.Locked;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;


    }

    void Update()
    {
        //Calculate movement velocity as 3D vector
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        // Movement vector
        Vector3 _velocity = (_movHorizontal + _movVertical).normalized * speed;


        // Apply Movement
        motor.Move(_velocity);


        //Calculate Rotation as a 3D vector (turning around)
        float _yRot = Input.GetAxis("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * mouseSensitivity;

        //Apply rotation

        motor.Rotate(_rotation); 


        //Calculate Camera rotation as a 3D vector (turning around)
        float _xRot = Input.GetAxis("Mouse Y");

        Vector3 _CameraRotation = new Vector3(_xRot, 0f, 0f) * mouseSensitivity;

        //Apply rotation

        motor.CameraRotate(_CameraRotation);

 

        

    }

   


}