using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    public float speed = 20;
    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartGame()
    {
        gameStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical"); 
        }

    }

    private void FixedUpdate()
    {  
        if (gameStarted)
        {
            rb.velocity = new Vector3(horizontal * speed * Time.fixedDeltaTime, 0, vertical * speed * Time.fixedDeltaTime);
        }
    }
}
