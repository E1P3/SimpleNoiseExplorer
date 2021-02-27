//Simple character controller for exploring the created map

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public CharacterController controller; 
    public Transform cam; 
    public float speed = 6f;
    public float g = 10f;

    Vector3 playerInput;
    Vector3 direction;
    Vector3 moveDir;
    float angle;
    float targetAngle;
    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // Update is called once per frame
    void Update()
    {

        direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (controller.detectCollisions)
            controller.Move(Vector3.down * g);

        Debug.Log(Vector3.down * g * Time.deltaTime);

        if (direction.magnitude >= 0.1f) {

            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;



            controller.Move( moveDir.normalized * speed * Time.deltaTime);

        }

    }
}
