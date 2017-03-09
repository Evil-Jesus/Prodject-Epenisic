using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed = 10;

    //move to this
    public Tile target;

    void Update()
    {

        if (Input.GetAxis("Horizontal") > 0) {
            //move right
        }

        if (Input.GetAxis("Horizontal") < 0) {
            //move left
        }

        if (Input.GetAxis("Vertical") > 0) {
            //move up
        }

        if (Input.GetAxis("Vertical") < 0) {
            //move down
        }

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(0, 0, 0), step);

        /* Free move system
        float newX = transform.position.x + (Input.GetAxis("Horizontal") * speed * Time.deltaTime);
        float newY = transform.position.y + (Input.GetAxis("Vertical") * speed * Time.deltaTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
        */
    }
}
