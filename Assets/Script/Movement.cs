using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{

    //bewegig im Unity
    public float speed;
    public float jump;
    float moveVelocity;

    //Grounded Vars
    bool isGrounded = true;

    void Update () 
    {
        //Springe
        if (Input.GetKeyDown (KeyCode.Space)) 
        {
            if(isGrounded)
            {
                GetComponent<Rigidbody2D> ().linearVelocity = new Vector2 (GetComponent<Rigidbody2D> ().linearVelocity.x, jump);
                isGrounded = false;
            }
        }

        moveVelocity = 0;

        //Links und Rechts
        if (Input.GetKey (KeyCode.A)) 
        {
            moveVelocity = -speed;
        }
        if (Input.GetKey (KeyCode.D)) 
        {
            moveVelocity = speed;
        }

        GetComponent<Rigidbody2D> ().linearVelocity = new Vector2 (moveVelocity, GetComponent<Rigidbody2D> ().linearVelocity.y);

    }
        void OnTriggerEnter2D()
    {
        isGrounded = true;
    }
}