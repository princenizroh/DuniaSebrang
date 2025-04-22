using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Compilation;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public CapsuleCollider capsule;

    public float speed = 5f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    public float jumpPower = 5f;
    public float gravity = 9.81f;
    private Vector3 moveDirection;
    private bool canMove = true;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
        capsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        Shader.SetGlobalVector("_Player", transform.position + Vector3.up * capsule.radius);
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(move * speed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            moveDirection.y = 0f; 

            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }
}
