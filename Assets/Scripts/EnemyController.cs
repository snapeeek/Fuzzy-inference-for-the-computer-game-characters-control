using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using AI.Fuzzy.Library;


public class EnemyController : MonoBehaviour
{
	public float moveSpeed = 3f;
	public Rigidbody2D rb;
	public Rigidbody2D player;
	public float hearingRadius = 25f;
	//public float seeingRadius = 25f;

	bool isPlayerVisible;
	Vector2 movement;

	void Start()
	{
		MamdaniFuzzySystem fuzzy = new MamdaniFuzzySystem();

		FuzzyVariable hearing = new FuzzyVariable("hearing", 0.0, 360.0);
		hearing.Terms.Add(new FuzzyTerm("south", new TriangularMembershipFunction(0, 0, 50)));
		//hearing.Terms.Add(new F)

	}

	// Update is called once per frame
	void Update()
	{
		movement.x = movement.y = 0;
		float distance = Vector3.Distance(rb.position, player.position);
		Vector3 targetDir = player.position - rb.position;
		float angle = Vector3.SignedAngle(targetDir, new Vector3(0,1), new Vector3(0,0,1));
		//0 - południe, 90 - zachód, 180 - północ, 270 - wschód
		//Debug.Log("Angle: " + (angle + 180));
		//Debug.Log("Distance: " + distance);
	}

	void FixedUpdate()
	{
		var rayDirection = player.transform.position - rb.transform.position;
		//Debug.Log("RayDirection: " + rayDirection);
		RaycastHit2D hit = Physics2D.Raycast(rb.transform.position, rayDirection, Mathf.Infinity);
		if (hit.transform == player.transform)
		{
			//Debug.Log("Enemy sees player");
			isPlayerVisible = true;
			if (rayDirection.x < 0)
				movement.x = -1f;
			else if (rayDirection.x > 1)
				movement.x = 1f;

			if (rayDirection.y > 0)
				movement.y = 1f;
			else if (rayDirection.y < 0)
				movement.y = -1f;

			rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
		}
		else
		{
			//Debug.Log("Enemy does not see player");
			isPlayerVisible = false;
		}

		

	}
}
