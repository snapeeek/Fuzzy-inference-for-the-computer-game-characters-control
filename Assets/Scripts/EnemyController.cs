using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyLogicApi;

public class EnemyController : MonoBehaviour
{
	public float moveSpeed = 3f;
	public Rigidbody2D rb;

	Vector2 movement;

	// Update is called once per frame
	void Update()
	{
		movement.x = movement.y = 0;
		
	}

	void FixedUpdate()
	{
		//rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}
}
