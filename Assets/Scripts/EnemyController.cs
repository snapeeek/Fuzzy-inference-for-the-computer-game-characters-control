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
		hearingFuzzy = new MamdaniFuzzySystem();


		//creating input fuzzy variables
		FuzzyVariable hearing = new FuzzyVariable("hearing", 0.0, 360.0);
		hearing.Terms.Add(new FuzzyTerm("south", new TriangularMembershipFunction(0.0, 0.0, 25.0)));
		hearing.Terms.Add(new FuzzyTerm("south2", new TriangularMembershipFunction(335.0, 360.0, 360.0)));
		hearing.Terms.Add(new FuzzyTerm("south-west", new TriangularMembershipFunction(20.0, 45.0, 70.0)));
		hearing.Terms.Add(new FuzzyTerm("west", new TriangularMembershipFunction(65.0, 90.0, 115.0)));
		hearing.Terms.Add(new FuzzyTerm("north-west", new TriangularMembershipFunction(110.0, 135.0, 160.0)));
		hearing.Terms.Add(new FuzzyTerm("north", new TriangularMembershipFunction(155.0, 180.0, 205.0)));
		hearing.Terms.Add(new FuzzyTerm("north-east", new TriangularMembershipFunction(200.0, 225.0, 250.0)));
		hearing.Terms.Add(new FuzzyTerm("east", new TriangularMembershipFunction(245.0, 270.0, 295.0)));
		hearing.Terms.Add(new FuzzyTerm("south-east", new TriangularMembershipFunction(290.0, 315.0, 340.0)));
		hearingFuzzy.Input.Add(hearing);


		//FuzzyVariable obstacle = new FuzzyVariable("obstacle", 0.0, )


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
		hearingFuzzy.Output.Add(outputDir);


		//creating fuzzy rules
		try
		{
			MamdaniFuzzyRule rule1 = hearingFuzzy.ParseRule("if (hearing is south) then moveDirection is S");
			MamdaniFuzzyRule rule12 = hearingFuzzy.ParseRule("if (hearing is south2) then moveDirection is S");
			MamdaniFuzzyRule rule2 = hearingFuzzy.ParseRule("if (hearing is south-west) then moveDirection is SW");
			MamdaniFuzzyRule rule3 = hearingFuzzy.ParseRule("if (hearing is west) then moveDirection is W");
			MamdaniFuzzyRule rule4 = hearingFuzzy.ParseRule("if (hearing is north-west) then moveDirection is NW");
			MamdaniFuzzyRule rule5 = hearingFuzzy.ParseRule("if (hearing is north) then moveDirection is N");
			MamdaniFuzzyRule rule6 = hearingFuzzy.ParseRule("if (hearing is north-east) then moveDirection is NE");
			MamdaniFuzzyRule rule7 = hearingFuzzy.ParseRule("if (hearing is east) then moveDirection is E");
			MamdaniFuzzyRule rule8 = hearingFuzzy.ParseRule("if (hearing is south-east) then moveDirection is SE");

			hearingFuzzy.Rules.Add(rule1);
			hearingFuzzy.Rules.Add(rule12);
			hearingFuzzy.Rules.Add(rule2);
			hearingFuzzy.Rules.Add(rule3);
			hearingFuzzy.Rules.Add(rule4);
			hearingFuzzy.Rules.Add(rule5);
			hearingFuzzy.Rules.Add(rule6);
			hearingFuzzy.Rules.Add(rule7);
			hearingFuzzy.Rules.Add(rule8);

		}
		catch(Exception ex)
		{
			Debug.Log(string.Format("Parsing exception: {0}", ex.Message));
		}


		obstacleFuzzy = new MamdaniFuzzySystem();
		FuzzyVariable hearingOutput = new FuzzyVariable("hearingOutput", 0.0, 8.0);
		hearingOutput.Terms.Add(new FuzzyTerm("S", new TriangularMembershipFunction(0.0, 0.5, 1.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("SW", new TriangularMembershipFunction(1.0, 1.5, 2.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("W", new TriangularMembershipFunction(2.0, 2.5, 3.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("NW", new TriangularMembershipFunction(3.0, 3.5, 4.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("N", new TriangularMembershipFunction(4.0, 4.5, 5.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("NE", new TriangularMembershipFunction(5.0, 5.5, 6.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("E", new TriangularMembershipFunction(6.0, 6.5, 7.0)));
		hearingOutput.Terms.Add(new FuzzyTerm("SE", new TriangularMembershipFunction(7.0, 7.5, 8.0)));
		obstacleFuzzy.Input.Add(hearingOutput);


		FuzzyVariable obstacleDistance = new FuzzyVariable("obstacleDistance", 0.0, maxObstacleDistance);
		obstacleDistance.Terms.Add(new FuzzyTerm("close", new TrapezoidMembershipFunction(0.0, 0.0, 1.0, 2.0)));
		obstacleDistance.Terms.Add(new FuzzyTerm("medium", new TriangularMembershipFunction(1.5, 4.0, 6.5)));
		obstacleDistance.Terms.Add(new FuzzyTerm("far", new TrapezoidMembershipFunction(6.0, 10.0, maxObstacleDistance, maxObstacleDistance)));
		obstacleFuzzy.Input.Add(obstacleDistance);

		FuzzyVariable wallOrientation = new FuzzyVariable("wallOrientation", 0.0, 2.0);
		wallOrientation.Terms.Add(new FuzzyTerm("horizontal", new TriangularMembershipFunction(0.0, 0.5, 1.0)));
		wallOrientation.Terms.Add(new FuzzyTerm("vertical", new TriangularMembershipFunction(1.0, 1.5, 2.0)));
		obstacleFuzzy.Input.Add(wallOrientation);

		obstacleFuzzy.Output.Add(outputDir);

		try
		{
			MamdaniFuzzyRule rule1 = obstacleFuzzy.ParseRule("if (hearingOutput is S) and (obstacleDistance is far) then moveDirection is S");
			MamdaniFuzzyRule rule2 = obstacleFuzzy.ParseRule("if (hearingOutput is SW) and (obstacleDistance is far or obstacleDistance is medium) then moveDirection is SW");
			MamdaniFuzzyRule rule3 = obstacleFuzzy.ParseRule("if (hearingOutput is W) and (obstacleDistance is far) then moveDirection is W");
			MamdaniFuzzyRule rule4 = obstacleFuzzy.ParseRule("if (hearingOutput is NW) and (obstacleDistance is far or obstacleDistance is medium) then moveDirection is NW");
			MamdaniFuzzyRule rule5 = obstacleFuzzy.ParseRule("if (hearingOutput is N) and (obstacleDistance is far) then moveDirection is N");
			MamdaniFuzzyRule rule6 = obstacleFuzzy.ParseRule("if (hearingOutput is NE) and (obstacleDistance is far or obstacleDistance is medium) then moveDirection is NE");
			MamdaniFuzzyRule rule7 = obstacleFuzzy.ParseRule("if (hearingOutput is E) and (obstacleDistance is far) then moveDirection is E");
			MamdaniFuzzyRule rule8 = obstacleFuzzy.ParseRule("if (hearingOutput is SE) and (obstacleDistance is far or obstacleDistance is medium) then moveDirection is SE");

			MamdaniFuzzyRule rule9 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is SW) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is W");
			MamdaniFuzzyRule rule10 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is SW) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is S");
			MamdaniFuzzyRule rule11 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is NW) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is W");
			MamdaniFuzzyRule rule12 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is NW) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is N");
			MamdaniFuzzyRule rule13 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is NE) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is E");
			MamdaniFuzzyRule rule14 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is NE) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is N");
			MamdaniFuzzyRule rule15 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is SE) and (obstacleDistance is close) and wallOrientation is horizontal then moveDirection is E");
			MamdaniFuzzyRule rule16 = obstacleFuzzy.ParseRule(
				"if (hearingOutput is SE) and (obstacleDistance is close) and wallOrientation is vertical then moveDirection is S");

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
				FuzzyVariable hearing = hearingFuzzy.InputByName("hearing");
				FuzzyVariable moveDirection = hearingFuzzy.OutputByName("moveDirection");

				//Debug.Log("Hearing: " + hearing);

				Dictionary<FuzzyVariable, double> input = new Dictionary<FuzzyVariable, double>();
				input.Add(hearing, angle + 180);
				Dictionary<FuzzyVariable, double> result = hearingFuzzy.Calculate(input);


				FuzzyVariable hearingOutput = obstacleFuzzy.InputByName("hearingOutput");
				FuzzyVariable obstacleDistance = obstacleFuzzy.InputByName("obstacleDistance");
				FuzzyVariable wallOrientation = obstacleFuzzy.InputByName("wallOrientation");

				

				FuzzyVariable output = obstacleFuzzy.OutputByName("moveDirection");

				Dictionary<FuzzyVariable, double> input2 = new Dictionary<FuzzyVariable, double>();
				input2.Add(hearingOutput, result[moveDirection]);
				input2.Add(obstacleDistance, (double)Vector2.Distance(hit.collider.ClosestPoint(rb.position), rb.position));
				double orient = 0;
				if (hit.normal.x != 0)
					orient = 1.5;
				else if (hit.normal.y != 0)
					orient = 0.5;

				input2.Add(wallOrientation, orient);

				
				Debug.DrawLine(new Vector3(-10, -10), hit.normal, Color.yellow, 1f);
				Dictionary<FuzzyVariable, double> result2 = obstacleFuzzy.Calculate(input2);

				Debug.Log("Movement: " + result2[output]);
				decideMovement(result2[output]);
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

	Vector3 findClosestVertex(Collider2D collider)
	{
		//Debug.Log(collider.bounds.size);
		//Debug.Log(collider.bounds.center);
		//Debug.Log(collider.bounds.extents);
		BoxCollider2D boxCollider = rb.GetComponentInParent<BoxCollider2D>();
		Debug.Log("BoxCollider size: " + boxCollider.size);

		List<Vector2> vertexes = new List<Vector2>
		{
			new Vector2(collider.bounds.center.x - collider.bounds.extents.x - boxCollider.size.x,
			collider.bounds.center.y - collider.bounds.extents.y - boxCollider.size.y),
			new Vector2(collider.bounds.center.x - collider.bounds.extents.x - boxCollider.size.x,
			collider.bounds.center.y + collider.bounds.extents.y + boxCollider.size.y),
			new Vector2(collider.bounds.center.x + collider.bounds.extents.x + boxCollider.size.x,
			collider.bounds.center.y - collider.bounds.extents.y - boxCollider.size.y),
			new Vector2(collider.bounds.center.x + collider.bounds.extents.x,
			collider.bounds.center.y + collider.bounds.extents.y + boxCollider.size.y),
		};

		vertexes.Sort(delegate(Vector2 x, Vector2 y)
		{
			if (Vector2.Distance(rb.position, x) == Vector2.Distance(rb.position, y)) return 0;
			else if (Vector2.Distance(rb.position, x) > Vector2.Distance(rb.position, y)) return 1;
			else if (Vector2.Distance(rb.position, x) < Vector2.Distance(rb.position, y)) return -1;

			return 0;
		});

		
		if (Vector2.Distance(rb.position, vertexes[0]) < 2)
			return new Vector3(vertexes[1].x, vertexes[1].y);
		else
			return new Vector3(vertexes[0].x, vertexes[0].y);
	}

	//double checkOrientation(Collider2D collider)
	//{

	//}
}
