using UnityEngine;

public class LightManager : MonoBehaviour
{
    private Light sun, moon;

    private float rotationSpeed = 1f;  // 1f = 6min per cycle

    private float xRotationSun, xRotationMoon;

    void Start()
    {
       /* sun = GameObject.Find("Sun").GetComponent<Light>();
        moon = GameObject.Find("Moon").GetComponent<Light>();

        sun = GameObject.Find("Sun").GetComponent<Light>();
        //moon = GameObject.Find("Moon").GetComponent<Light>();


        // Start position for the sun and moond
        xRotationSun = 20f;
        xRotationMoon = 180f + xRotationSun;*/
    }

    void FixedUpdate()
    {
        // Set new rotations
        /*xRotationSun += rotationSpeed * Time.deltaTime;
        xRotationMoon += rotationSpeed * Time.deltaTime;
        sun.transform.localEulerAngles = new Vector3(xRotationSun, sun.transform.rotation.y, sun.transform.rotation.z);

        moon.transform.localEulerAngles = new Vector3(xRotationMoon, sun.transform.rotation.y, sun.transform.rotation.z);*/

        //moon.transform.localEulerAngles = new Vector3(xRotationMoon, sun.transform.rotation.y, sun.transform.rotation.z);
    }
}
