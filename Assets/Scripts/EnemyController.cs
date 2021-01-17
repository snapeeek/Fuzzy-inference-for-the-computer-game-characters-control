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
	MamdaniFuzzySystem hearingFuzzy, obstacleFuzzy;
	public double maxObstacleDistance = 35.0;
	double angle;

	//bool isPlayerVisible;
	Vector2 movement, playerMovement;
	bool isPlayerMoving;

	void Start()
	{
		obstacleFuzzy = new MamdaniFuzzySystem();

		//creating input fuzzy variables
		FuzzyVariable hearing = new FuzzyVariable("hearing", 0.0, 360.0);
		hearing.Terms.Add(new FuzzyTerm("south", new TriangularMembershipFunction(0.0, 0.0, 35.0)));
		hearing.Terms.Add(new FuzzyTerm("south_west", new TriangularMembershipFunction(10.0, 45.0, 80.0)));
		hearing.Terms.Add(new FuzzyTerm("west", new TriangularMembershipFunction(55.0, 90.0, 125.0)));
		hearing.Terms.Add(new FuzzyTerm("north_west", new TriangularMembershipFunction(100.0, 135.0, 170.0)));
		hearing.Terms.Add(new FuzzyTerm("north", new TriangularMembershipFunction(145.0, 180.0, 215.0)));
		hearing.Terms.Add(new FuzzyTerm("north_east", new TriangularMembershipFunction(190.0, 225.0, 260.0)));
		hearing.Terms.Add(new FuzzyTerm("east", new TriangularMembershipFunction(235.0, 270.0, 305.0)));
		hearing.Terms.Add(new FuzzyTerm("south_east", new TriangularMembershipFunction(280.0, 315.0, 350.0)));
		hearing.Terms.Add(new FuzzyTerm("south2", new TriangularMembershipFunction(325.0, 360.0, 360.0)));
		obstacleFuzzy.Input.Add(hearing);


		FuzzyVariable obstacleDistance = new FuzzyVariable("obstacleDistance", 0.0, maxObstacleDistance);
		obstacleDistance.Terms.Add(new FuzzyTerm("close", new TrapezoidMembershipFunction(0.0, 0.0, 1.0, 3.0)));
		obstacleDistance.Terms.Add(new FuzzyTerm("medium", new TriangularMembershipFunction(1.0, 4.0, 6.5)));
		obstacleDistance.Terms.Add(new FuzzyTerm("far", new TrapezoidMembershipFunction(4.0, 10.0, maxObstacleDistance, maxObstacleDistance)));
		obstacleFuzzy.Input.Add(obstacleDistance);

		FuzzyVariable wallOrientation = new FuzzyVariable("wallOrientation", 0.0, 2.0);
		wallOrientation.Terms.Add(new FuzzyTerm("horizontal", new TriangularMembershipFunction(0.0, 0.5, 1.0)));
		wallOrientation.Terms.Add(new FuzzyTerm("vertical", new TriangularMembershipFunction(1.0, 1.5, 2.0)));
		obstacleFuzzy.Input.Add(wallOrientation);

		FuzzyVariable closerVertex = new FuzzyVariable("closerVertex", 0.0, 2.0);
		closerVertex.Terms.Add(new FuzzyTerm("vertex1", new TriangularMembershipFunction(0.0, 1.0, 2.0)));
		closerVertex.Terms.Add(new FuzzyTerm("vertex2", new TriangularMembershipFunction(1.0, 2.0, 3.0)));
		obstacleFuzzy.Input.Add(closerVertex);

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

		obstacleFuzzy.Output.Add(outputDir);

		try
		{
			MamdaniFuzzyRule rule1 = obstacleFuzzy.ParseRule("if ((hearing is south) or (hearing is south2)) and (obstacleDistance is far) then moveDirection is S");
			MamdaniFuzzyRule rule2 = obstacleFuzzy.ParseRule("if (hearing is south_west) and ((obstacleDistance is far) or (obstacleDistance is medium)) then moveDirection is SW");
			MamdaniFuzzyRule rule3 = obstacleFuzzy.ParseRule("if (hearing is west) and (obstacleDistance is far) then moveDirection is W");
			MamdaniFuzzyRule rule4 = obstacleFuzzy.ParseRule("if (hearing is north_west) and ((obstacleDistance is far) or (obstacleDistance is medium)) then moveDirection is NW");
			MamdaniFuzzyRule rule5 = obstacleFuzzy.ParseRule("if (hearing is north) and (obstacleDistance is far) then moveDirection is N");
			MamdaniFuzzyRule rule6 = obstacleFuzzy.ParseRule("if (hearing is north_east) and ((obstacleDistance is far) or (obstacleDistance is medium)) then moveDirection is NE");
			MamdaniFuzzyRule rule7 = obstacleFuzzy.ParseRule("if (hearing is east) and (obstacleDistance is far) then moveDirection is E");
			MamdaniFuzzyRule rule8 = obstacleFuzzy.ParseRule("if (hearing is south_east) and ((obstacleDistance is far) or (obstacleDistance is medium)) then moveDirection is SE");

			MamdaniFuzzyRule rule9 = obstacleFuzzy.ParseRule(
				"if (hearing is south_west) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is W");
			MamdaniFuzzyRule rule10 = obstacleFuzzy.ParseRule(
				"if (hearing is south_west) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is S");
			MamdaniFuzzyRule rule11 = obstacleFuzzy.ParseRule(
				"if (hearing is north_west) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is W");
			MamdaniFuzzyRule rule12 = obstacleFuzzy.ParseRule(
				"if (hearing is north_west) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is N");
			MamdaniFuzzyRule rule13 = obstacleFuzzy.ParseRule(
				"if (hearing is north_east) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is E");
			MamdaniFuzzyRule rule14 = obstacleFuzzy.ParseRule(
				"if (hearing is north_east) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is N");
			MamdaniFuzzyRule rule15 = obstacleFuzzy.ParseRule(
				"if (hearing is south_east) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is E");
			MamdaniFuzzyRule rule16 = obstacleFuzzy.ParseRule(
				"if (hearing is south_east) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is S");

			MamdaniFuzzyRule rule17 = obstacleFuzzy.ParseRule(
				"if ((hearing is south) or (hearing is south2) or (hearing is north)) and (obstacleDistance is close) and closerVertex is vertex1 then moveDirection is W");
			MamdaniFuzzyRule rule18 = obstacleFuzzy.ParseRule(
				"if ((hearing is south) or (hearing is south2) or (hearing is north)) and (obstacleDistance is close) and closerVertex is vertex2 then moveDirection is E");
			MamdaniFuzzyRule rule19 = obstacleFuzzy.ParseRule(
				"if ((hearing is west) or (hearing is east)) and (obstacleDistance is close) and closerVertex is vertex1 then moveDirection is N");
			MamdaniFuzzyRule rule20 = obstacleFuzzy.ParseRule(
				"if ((hearing is west) or (hearing is east)) and (obstacleDistance is close) and closerVertex is vertex2 then moveDirection is S");

			MamdaniFuzzyRule rule21 = obstacleFuzzy.ParseRule(
				"if ((hearing is south) or (hearing is south2) or (hearing is west)) and (obstacleDistance is medium) and closerVertex is vertex1 then moveDirection is SW");
			MamdaniFuzzyRule rule22 = obstacleFuzzy.ParseRule(
				"if ((hearing is south) or (hearing is south2) or (hearing is east)) and (obstacleDistance is medium) and closerVertex is vertex2 then moveDirection is SE");

			MamdaniFuzzyRule rule23 = obstacleFuzzy.ParseRule(
				"if ((hearing is north) or (hearing is west)) and (obstacleDistance is medium) and closerVertex is vertex1 then moveDirection is NW");
			MamdaniFuzzyRule rule24 = obstacleFuzzy.ParseRule(
				"if ((hearing is north) or (hearing is east)) and (obstacleDistance is medium) and closerVertex is vertex2 then moveDirection is NE");

			MamdaniFuzzyRule rule25 = obstacleFuzzy.ParseRule(
				"if (hearing is east) and (obstacleDistance is medium) and closerVertex is vertex1 then moveDirection is NE");

			MamdaniFuzzyRule rule26 = obstacleFuzzy.ParseRule(
				"if (hearing is west) and (obstacleDistance is medium) and closerVertex is vertex2 then moveDirection is SW");


			obstacleFuzzy.Rules.Add(rule1);
			obstacleFuzzy.Rules.Add(rule2);
			obstacleFuzzy.Rules.Add(rule3);
			obstacleFuzzy.Rules.Add(rule4);
			obstacleFuzzy.Rules.Add(rule5);
			obstacleFuzzy.Rules.Add(rule6);
			obstacleFuzzy.Rules.Add(rule7);
			obstacleFuzzy.Rules.Add(rule8);
			obstacleFuzzy.Rules.Add(rule9);
			obstacleFuzzy.Rules.Add(rule10);
			obstacleFuzzy.Rules.Add(rule11);
			obstacleFuzzy.Rules.Add(rule12);
			obstacleFuzzy.Rules.Add(rule13);
			obstacleFuzzy.Rules.Add(rule14);
			obstacleFuzzy.Rules.Add(rule15);
			obstacleFuzzy.Rules.Add(rule16);
			obstacleFuzzy.Rules.Add(rule17);
			obstacleFuzzy.Rules.Add(rule18);
			obstacleFuzzy.Rules.Add(rule19);
			obstacleFuzzy.Rules.Add(rule20);
			obstacleFuzzy.Rules.Add(rule21);
			obstacleFuzzy.Rules.Add(rule22);
			obstacleFuzzy.Rules.Add(rule23);
			obstacleFuzzy.Rules.Add(rule24);
			obstacleFuzzy.Rules.Add(rule25);
			obstacleFuzzy.Rules.Add(rule26);

		}
		catch (Exception ex)
		{
			Debug.Log(string.Format("Parsing exception: {0}", ex.Message));
		}

	}

	// Update is called once per frame
	void Update()
	{		
		//0 - południe, 90 - zachód, 180 - północ, 270 - wschód
	}

	void FixedUpdate()
	{
		playerMovement.x = Input.GetAxisRaw("Horizontal");
		playerMovement.y = Input.GetAxisRaw("Vertical");

		if (playerMovement.x != 0 || playerMovement.y != 0)
			isPlayerMoving = true;
		else
			isPlayerMoving = false;

		//Debug.Log(isPlayerMoving);

		movement.x = movement.y = 0;
		float distance = Vector3.Distance(rb.position, player.position);
		Vector3 targetDir = player.position - rb.position;
		angle = (double)Vector3.SignedAngle(targetDir, new Vector3(0, 1), new Vector3(0, 0, 1));

		var rayDirection = player.transform.position - rb.transform.position;
		RaycastHit2D hit = Physics2D.Raycast(rb.transform.position, rayDirection, Mathf.Infinity);
		if (hit.transform == player.transform)
		{
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
			if (isPlayerMoving)
			{

				FuzzyVariable hearingOutput = obstacleFuzzy.InputByName("hearing");
				FuzzyVariable obstacleDistance = obstacleFuzzy.InputByName("obstacleDistance");
				FuzzyVariable wallOrientation = obstacleFuzzy.InputByName("wallOrientation");
				FuzzyVariable closerVertex = obstacleFuzzy.InputByName("closerVertex");

				

				FuzzyVariable output = obstacleFuzzy.OutputByName("moveDirection");

				Dictionary<FuzzyVariable, double> input2 = new Dictionary<FuzzyVariable, double>();
				input2.Add(hearingOutput, angle + 180);
				input2.Add(obstacleDistance, (double)Vector2.Distance(hit.collider.ClosestPoint(rb.position), rb.position));
				double orient = 0;
				if (hit.normal.x != 0)
					orient = 1.5;
				else if (hit.normal.y != 0)
					orient = 0.5;

				input2.Add(wallOrientation, orient);

				double closer = -1;
				if (hit.normal.x == 1)
				{
					if (Vector2.Distance(new Vector2(hit.collider.bounds.center.x + hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y + hit.collider.bounds.extents.y), rb.position) < Vector2.Distance(new Vector2(hit.collider.bounds.center.x + hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y - hit.collider.bounds.extents.y), rb.position))
						closer = 0.5;
					else
						closer = 1.5;
				}
				else if (hit.normal.x == -1)
				{
					if (Vector2.Distance(new Vector2(hit.collider.bounds.center.x - hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y + hit.collider.bounds.extents.y), rb.position) < Vector2.Distance(new Vector2(hit.collider.bounds.center.x - hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y - hit.collider.bounds.extents.y), rb.position))
						closer = 0.5;
					else
						closer = 1.5;
				}

				if (hit.normal.y == 1)
				{
					if (Vector2.Distance(new Vector2(hit.collider.bounds.center.x - hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y + hit.collider.bounds.extents.y), rb.position) < Vector2.Distance(new Vector2(hit.collider.bounds.center.x + hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y + hit.collider.bounds.extents.y), rb.position))
						closer = 1.0;
					else
						closer = 2.0;
				}
				else if (hit.normal.y == -1)
				{
					if (Vector2.Distance(new Vector2(hit.collider.bounds.center.x - hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y - hit.collider.bounds.extents.y), rb.position) < Vector2.Distance(new Vector2(hit.collider.bounds.center.x + hit.collider.bounds.extents.x,
						hit.collider.bounds.center.y - hit.collider.bounds.extents.y), rb.position))
						closer = 1.0;
					else
						closer = 2.0;
				}

				input2.Add(closerVertex, closer);
				
				//Debug.DrawLine(new Vector3(-10, -10), hit.normal, Color.yellow, 1f);
				Dictionary<FuzzyVariable, double> result2 = obstacleFuzzy.Calculate(input2);

				System.Random randomize = new System.Random();
				double randomElement = (double)randomize.Next(90, 110) / 100;

				//Debug.Log("Movement: " + result2[output]);
				decideMovement(result2[output] * randomElement);
			}
		}


		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}

	void decideMovement(double input)
	{
		if (input > 0 && input < 1)
		{
			movement.x = 0;
			movement.y = -1;
			//Debug.Log("S");
		}
		else if (input > 1 && input < 2)
		{
			movement.x = -1;
			movement.y = -1;
			//Debug.Log("SW");
		}
		else if (input > 2 && input < 3)
		{
			movement.x = -1;
			movement.y = 0;
			//Debug.Log("W");
		}
		else if (input > 3 && input < 4)
		{
			movement.x = -1;
			movement.y = 1;
			//Debug.Log("NW");
		}
		else if (input > 4 && input < 5)
		{
			movement.x = 0;
			movement.y = 1;
			//Debug.Log("N");
		}
		else if (input > 5 && input < 6)
		{
			movement.x = 1;
			movement.y = 1;
			//Debug.Log("NE");
		}
		else if (input > 6 && input < 7)
		{
			movement.x = 1;
			movement.y = 0;
			//Debug.Log("E");
		}
		else if (input > 7 && input < 8)
		{
			movement.x = 1;
			movement.y = -1;
			//Debug.Log("SE");
		}
	}


}