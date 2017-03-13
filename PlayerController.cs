using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static float speed;
    public static bool allowMovement = true;

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftShift)) {
            speed = 5;
        } else {
            speed = 3;
        }

        if (allowMovement) {
            float pushX = transform.position.x + Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
            float pushY = transform.position.y + Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
            transform.position = new Vector3(pushX, pushY, transform.position.z);
        }
    }
}
