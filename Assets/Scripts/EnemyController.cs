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
	MamdaniFuzzySystem fuzzy;
	double angle;

	bool isPlayerVisible;
	Vector2 movement, playerMovement;
	bool isPlayerMoving;

	void Start()
	{
		fuzzy = new MamdaniFuzzySystem();


		//creating input fuzzy variables
		FuzzyVariable hearing = new FuzzyVariable("hearing", 0.0, 360.0);
		hearing.Terms.Add(new FuzzyTerm("south", new TriangularMembershipFunction(0.0, 0.0, 25.0)));
		hearing.Terms.Add(new FuzzyTerm("south", new TriangularMembershipFunction(335.0, 360.0, 360.0)));
		hearing.Terms.Add(new FuzzyTerm("south-west", new TriangularMembershipFunction(0.0, 45.0, 95.0)));
		hearing.Terms.Add(new FuzzyTerm("west", new TriangularMembershipFunction(40.0, 90.0, 140.0)));
		hearing.Terms.Add(new FuzzyTerm("north-west", new TriangularMembershipFunction(85.0, 135.0, 185.0)));
		hearing.Terms.Add(new FuzzyTerm("north", new TriangularMembershipFunction(130.0, 180.0, 230.0)));
		hearing.Terms.Add(new FuzzyTerm("north-east", new TriangularMembershipFunction(175.0, 225.0, 275.0)));
		hearing.Terms.Add(new FuzzyTerm("east", new TriangularMembershipFunction(220.0, 270.0, 320.0)));
		hearing.Terms.Add(new FuzzyTerm("south-east", new TriangularMembershipFunction(265.0, 315.0, 360.0)));
		fuzzy.Input.Add(hearing);
	


		//creating output variables
		FuzzyVariable outputDir = new FuzzyVariable("moveDirection", 0.0, 8.0);
		outputDir.Terms.Add(new FuzzyTerm("S", new TriangularMembershipFunction(0.0, 0.5, 1.0)));
		outputDir.Terms.Add(new FuzzyTerm("SW", new TriangularMembershipFunction(1.0, 1.5, 2.0)));
		outputDir.Terms.Add(new FuzzyTerm("W", new TriangularMembershipFunction(2.0, 2.5, 3.0)));
		outputDir.Terms.Add(new FuzzyTerm("NW", new TriangularMembershipFunction(3.0, 3.5, 4.0)));
		outputDir.Terms.Add(new FuzzyTerm("N", new TriangularMembershipFunction(4.0, 4.5, 5.0)));
		outputDir.Terms.Add(new FuzzyTerm("NE", new TriangularMembershipFunction(5.0, 5.5, 6.0)));
		outputDir.Terms.Add(new FuzzyTerm("E", new TriangularMembershipFunction(6.0, 6.5, 7.0)));
		outputDir.Terms.Add(new FuzzyTerm("SE", new TriangularMembershipFunction(7.0, 7.5, 8.0)));
		fuzzy.Output.Add(outputDir);


		//creating fuzzy rules
		try
		{
			MamdaniFuzzyRule rule1 = fuzzy.ParseRule("if (hearing is south) then moveDirection is S");
			MamdaniFuzzyRule rule2 = fuzzy.ParseRule("if (hearing is south-west) then moveDirection is SW");
			MamdaniFuzzyRule rule3 = fuzzy.ParseRule("if (hearing is west) then moveDirection is W");
			MamdaniFuzzyRule rule4 = fuzzy.ParseRule("if (hearing is north-west) then moveDirection is NW");
			MamdaniFuzzyRule rule5 = fuzzy.ParseRule("if (hearing is north) then moveDirection is N");
			MamdaniFuzzyRule rule6 = fuzzy.ParseRule("if (hearing is north-east) then moveDirection is NE");
			MamdaniFuzzyRule rule7 = fuzzy.ParseRule("if (hearing is east) then moveDirection is E");
			MamdaniFuzzyRule rule8 = fuzzy.ParseRule("if (hearing is south-east) then moveDirection is SE");

			fuzzy.Rules.Add(rule1);
			fuzzy.Rules.Add(rule2);
			fuzzy.Rules.Add(rule3);
			fuzzy.Rules.Add(rule4);
			fuzzy.Rules.Add(rule5);
			fuzzy.Rules.Add(rule6);
			fuzzy.Rules.Add(rule7);
			fuzzy.Rules.Add(rule8);

		}
		catch(Exception ex)
		{
			Debug.Log(string.Format("Parsing exception: {0}", ex.Message));
		}
	}

	// Update is called once per frame
	void Update()
	{
		
		//0 - południe, 90 - zachód, 180 - północ, 270 - wschód
		//Debug.Log("Angle: " + (angle + 180));
		//Debug.Log("Distance: " + distance);
	}

	void FixedUpdate()
	{
		playerMovement.x = Input.GetAxisRaw("Horizontal");
		playerMovement.y = Input.GetAxisRaw("Vertical");

		if (playerMovement.x != 0 || playerMovement.y != 0)
			isPlayerMoving = true;
		else
			isPlayerMoving = false;

		movement.x = movement.y = 0;
		float distance = Vector3.Distance(rb.position, player.position);
		Vector3 targetDir = player.position - rb.position;
		angle = (double)Vector3.SignedAngle(targetDir, new Vector3(0, 1), new Vector3(0, 0, 1));

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
		}
		else
		{
			//Debug.Log("Enemy does not see player");
			isPlayerVisible = false;
			if (isPlayerMoving)
			{
				FuzzyVariable hearing = fuzzy.InputByName("hearing");
				FuzzyVariable moveDirection = fuzzy.OutputByName("moveDirection");

				Debug.Log("Hearing: " + hearing);

				Dictionary<FuzzyVariable, double> input = new Dictionary<FuzzyVariable, double>();
				input.Add(hearing, angle + 180);

				Debug.Log("Angle: " + angle);


				Dictionary<FuzzyVariable, double> result = fuzzy.Calculate(input);

				Debug.Log(result[moveDirection]);
				if (result[moveDirection] > 0 && result[moveDirection] < 1)
				{
					movement.x = 0;
					movement.y = -1;
				}
				else if (result[moveDirection] > 1 && result[moveDirection] < 2)
				{
					movement.x = -1;
					movement.y = -1;
				}
				else if (result[moveDirection] > 2 && result[moveDirection] < 3)
				{
					movement.x = -1;
					movement.y = 0;
				}
				else if (result[moveDirection] > 3 && result[moveDirection] < 4)
				{
					movement.x = -1;
					movement.y = 1;
				}
				else if (result[moveDirection] > 4 && result[moveDirection] < 5)
				{
					movement.x = 0;
					movement.y = 1;
				}
				else if (result[moveDirection] > 5 && result[moveDirection] < 6)
				{
					movement.x = 1;
					movement.y = 1;
				}
				else if (result[moveDirection] > 6 && result[moveDirection] < 7)
				{
					movement.x = 1;
					movement.y = 0;
				}
				else if (result[moveDirection] > 7 && result[moveDirection] < 8)
				{
					movement.x = 1;
					movement.y = -1;
				}
			}
		}


		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}
}
