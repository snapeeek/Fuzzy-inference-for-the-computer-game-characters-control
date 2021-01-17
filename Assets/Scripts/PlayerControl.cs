using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public float moveSpeed = 5f;
	public Rigidbody2D rb;
	public AudioSource audioControl;

	Vector2 movement;
	bool isMoving = false;

    // Update is called once per frame
    void Update()
    {
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");


		if (movement.x != 0 || movement.y != 0)
			isMoving = true;
		else
			isMoving = false;

		if (isMoving)
		{
			if (!audioControl.isPlaying)
			audioControl.Play();
		}
		else
			audioControl.Stop();
	}

	void FixedUpdate()
	{
		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}
}
