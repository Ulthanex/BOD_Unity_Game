using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum of our team//
//public enum playerTeam{
//	blue,
//	red
//}

//Enum of which side our scouts on
//public enum scoutRole{
//	left,
//	right,
//	none
//}

//Enum of zones//
//public enum mapZone{
//	redBase,
//	redCourtyardA,
//	redCourtyardB,
//	redAlleyA,
//	redAlleyB,
//	blueAlleyA,
//	blueAlleyB,
//	blueCourtyardB,
//	blueCourtyardA,
//	blueBase,
//	error
//}

/*-------------------------------------------------------------------------------------------------------------------*/
//Class for Defensive positions, contains position, whether it's occupied & orientations to hold defensive positions //
//public class defencePoint{
//	public Vector3 pos;
//	public bool occupied = false;
//	public mapZone area = mapZone.error;
//	public List<float> orientations;
//
//	public defencePoint(Vector3 position, List<float> angles){
//		pos = position;
//		orientations = angles;
//	}
//
//	public defencePoint(Vector3 position, mapZone z,List<float> angles){
//		pos = position;
//		area = z;
//		orientations = angles;
//	}
//}

/*------------------------------------------*/
// Located enemy presence at a certain time //
//public class enemyPresence{
//	public Vector3 pos; //The position of the enemy at time sighted
//	public mapZone zone; //The zone they were spotted in
//	public int currentHealth; //The Health they were spotted at
//	public bool hasOurFlag; //Whether they have our flag
//	public float timeSpotted; //The time in which it was spotted
//
//	public enemyPresence(Vector3 p, mapZone z, int c, bool f, float t){
//		pos = p;
//		zone = z;
//		currentHealth = c;
//		hasOurFlag = f;
//		timeSpotted = t;
//	}
//}


//Extends the BasicBehaviourLibrary which provides access to the different available sensors, properties should only be made
//Using the properties provided with no access to the agent class
public class OldBLTemplate : BasicBehaviourLibrary {

	/*------------------*/
	//Personal variables//
	/*------------------*/
	private int agentID; //ID of the agent talking
	private playerTeam myTeam; //Team of the agent determined through spawn position 
	private defencePoint currentPointLocation = null; //The current point we are defending if we are
	private bool scouting = false; //Whether we are currently scouting a spot (i.e., flee when enemies get to close)
	private bool guarding = false; //Whether we are guarding a point (i.e., shoot at enemies seen)
	private bool aggressive = false; //Whether we are taking an aggresive approach
	private float sightingTime = 10; //How long we keep hold of a sighting

	/*---------------------------------*/
	// Static Communication Variables  //
	/*---------------------------------*/
	protected static List<Vector3> teamMemberLocations = new List<Vector3>(5); //List of Vector3 for our agents
	protected static List<enemyPresence> enemySighting = new List<enemyPresence>(); //List of enemy Sightings
	protected static int memberCount = 0; //Incrementor for members
	protected bool initComplete = false; //Flag that is set after initial communication setup

	/*-------*/
	//Senses //----------------------------------------------------------------------------------------------------------------
	/*-------*/
	public bool InitialisationComplete(){return initComplete;} //Done Initialisation
	public bool GuardingPoint(){return guarding;} //Whether We Are guarding a point
	public bool ScoutingPoint(){return scouting;} //Whether We Are Scouting a Point
	/*
	public bool enemyRedBase(){return enemyInZone(mapZone.redBase);}
	public bool enemyBlueBase(){return enemyInZone(mapZone.blueBase);}
	public bool enemyRedCourtyardA(){return enemyInZone(mapZone.redCourtyardA);}
	public bool enemyBlueCourtyardA(){return enemyInZone(mapZone.blueCourtyardA);}
	public bool enemyRedCourtyardB(){return enemyInZone(mapZone.redCourtyardB);}
	public bool enemyBlueCourtyardB(){return enemyInZone(mapZone.blueCourtyardB);}
	public bool enemyRedAlleyA(){return enemyInZone(mapZone.redAlleyA);}
	public bool enemyBlueAlleyA(){return enemyInZone(mapZone.blueAlleyA);}
	public bool enemyRedAlleyB(){return enemyInZone(mapZone.redAlleyB);}
	public bool enemyBlueAlleyB(){return enemyInZone(mapZone.blueAlleyB);}
	public bool enemyLeftSide(){return (enemyInZone (mapZone.blueAlleyB) || enemyInZone (mapZone.redAlleyA));} //Left side of map
	public bool enemyRightSide(){return (enemyInZone (mapZone.blueAlleyA) || enemyInZone (mapZone.redAlleyB));} //Right side of map*/

	/*----------*/
	//Map Points//
	/*----------*/
	public static List<defencePoint> redTeamScoutPos = new List<defencePoint> (){ 
		new defencePoint (new Vector3 (1f, 0f, -17.5f),mapZone.redCourtyardB, new List<float> (){60f}), 
		new defencePoint (new Vector3 (-23f, 0f, 1f),mapZone.blueAlleyB, new List<float> (){55f})};

	public static List<defencePoint> redTeamInitialDefPos = new List<defencePoint> (){  
		new defencePoint (new Vector3 (-6f, 0f, -22.5f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-1f, 0f, -15f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-23f, 0f, -10.5f), new List<float> (){15f})};

	
	public static List<defencePoint> redTeamCourtyardDefPos = new List<defencePoint> (){  
		new defencePoint (new Vector3 (-6f, 0f, -22.5f),mapZone.redCourtyardA,new List<float> (){15f}),
		new defencePoint (new Vector3 (-1f, 0f, -15f),mapZone.redCourtyardA, new List<float> (){15f}),
		new defencePoint (new Vector3 (-1f, 0f, -24f),mapZone.redCourtyardA, new List<float> (){15f}),
		new defencePoint (new Vector3 (-7f, 0f, -24f),mapZone.redCourtyardA, new List<float> (){15f}),
		new defencePoint (new Vector3 (-2f, 0f, -13.5f),mapZone.redCourtyardA, new List<float> (){15f})};

	public static List<defencePoint> redTeamAlleyDefPos = new List<defencePoint> (){  
		new defencePoint (new Vector3 (-22f, 0f, -9f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-24f, 0f, -9.5f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-20.5f, 0f, -11f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-23f, 0f, -10.5f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-19f, 0f, -13.5f), new List<float> (){15f})};

	public static List<defencePoint> redTeamBaseDefPos = new List<defencePoint> (){ 
		new defencePoint (new Vector3 (-24f, 0f, -16f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-20.5f, 0f, -16f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-12.5f, 0f, -13.5f), new List<float> (){220f}),
		new defencePoint (new Vector3 (-16f, 0f, -19f), new List<float> (){15f}),
		new defencePoint (new Vector3 (-16f, 0f, -22.5f), new List<float> (){15f})};


	public List<Vector3> blueTeamScoutPos = new List<Vector3>(){new Vector3(-17f,0f,1.5f)};

	//Zone Boundaries//
	//public static Vector2 RedTeamBaseACornerA = new Vector2(-25f,-12f);
	//public static Vector2 RedTeamBaseACornerB = new Vector2(-11f,-25f);
	public static Rect RedTeamBaseRect = Rect.MinMaxRect(-25f,-25f,-11f,-12f);
	//public static Vector2 RedTeamCourtyardACornerA = new Vector2(-11f,-12f);
	//public static Vector2 RedTeamCourtyardACornerB = new Vector2(0f,-25f);
	public static Rect RedTeamCourtyardARect = Rect.MinMaxRect(-11f,-25f,0f,-12f);
	//public static Vector2 RedTeamCourtyardBCornerA = new Vector2(0f,-12f);
	//public static Vector2 RedTeamCourtyardBCornerB = new Vector2(25f,-25f);
	public static Rect RedTeamCourtyardBRect = Rect.MinMaxRect(0f,-25f,25f,-12f);
	//public static Vector2 RedTeamAlleyACornerA = new Vector2(-25f,0f);
	//public static Vector2 RedTeamAlleyACornerB = new Vector2(0f,-12f);
	public static Rect RedTeamAlleyARect = Rect.MinMaxRect(-25f,-12f,0f,0f);
	//public static Vector2 RedTeamAlleyBCornerA = new Vector2(0f,0f);
	//public static Vector2 RedTeamAlleyBCornerB = new Vector2(-25f,-12f);
	public static Rect RedTeamAlleyBRect = Rect.MinMaxRect(0f,-12f,25f,0f);


	//public static Vector2 BlueTeamBaseACornerA= new Vector2(11f,25f);
	//public static Vector2 BlueTeamBaseACornerB = new Vector2(25f,12f);
	public static Rect BlueTeamBaseRect = Rect.MinMaxRect(11f,12f,25f,25f);
	//public static Vector2 BlueTeamCourtyardACornerA = new Vector2(0f,25f);
	//public static Vector2 BlueTeamCourtyardACornerB = new Vector2(11f,12f);
	public static Rect BlueTeamCourtyardARect = Rect.MinMaxRect(0f,12f,11f,25f);
	//public static Vector2 BlueTeamCourtyardBCornerA = new Vector2(-25f,25f);
	//public static Vector2 BlueTeamCourtyardBCornerB = new Vector2(0f,12f);
	public static Rect BlueTeamCourtyardBRect = Rect.MinMaxRect(-25f,12f,0f,25f);
	//public static Vector2 BlueTeamAlleyACornerA = new Vector2(0f,12f);
	//public static Vector2 BlueTeamAlleyACornerB = new Vector2(25f,0f);
	public static Rect BlueTeamAlleyARect = Rect.MinMaxRect(0f,0f,25f,12f);
	//public static Vector2 BlueTeamAlleyBCornerA = new Vector2(-25f,12f);
	//public static Vector2 BlueTeamAlleyBCornerB = new Vector2(0f,0f);
	public static Rect BlueTeamAlleyBRect = Rect.MinMaxRect(-25f,0f,0f,12f);

	/*---------------*/
	//Initialisation //----------------------------------------------------------------------------------------------------
	/*---------------*/
	public void RoundStartInitialisation(){

		//Team Setup --------------------------------------------------
		if (SpawnLocation.x < 0) { //Red if spawn position is negative
			myTeam = playerTeam.red;
		} else { //Blue if spawn position is positive
			myTeam = playerTeam.blue;
		}
			
		//Communication--------------------------------------------------
		agentID = announceSelf (); //Initialises AgentId within the Team

		//Timer for defensive Strategy-----------------------------------
		StartCoroutine(becomeAggressive()); //Starts a timer in which we consider becoming aggressive if we haven't seen anyone

		//Sets Flag to prevent re-initialisation
		initComplete = true; 

	
		//NavAgent.TargetPosition = positionAroundPoint(5, SpawnLocation, 2);
		//Debug.Log ("Target = " + NavAgent.targetPosition);
	}
		

	/*-------------------------------------------*/
	//Changes Strategy after a set amount of time//
	IEnumerator becomeAggressive(){
		yield return new WaitForSeconds(45); //Wait for 45 seconds
		aggressive = true;
	}


	/*--------------*/
	//Communication //----------------------------------------------------------------------------------------------------
	/*--------------*/


	/*------------------------------*/
	// Static Communication methods //
	/*------------------------------*/
	static int announceSelf(){memberCount++; return memberCount-1;}


	/*-----------------------*/
	//State Current Location //
	/*-----------------------*/
	public void stateLocation(){

	}


	/*-------------------------------------------*/
	//Alerts team members to presence of enemies //
	public bool SpottedEnemyPresence(){
		
		//If no enemies are visible, return false
		if (!EnemiesSpotted ()) {
			return false;
		//Enemies are visible, communicate them to team then return true
		} else {

			//Clear any Outdated Sightings first
			clearOutdatedSightings();

			//For all enemies present, create a location of where they were seen
			for (int i = 0; i < EnemyAgentsInSight.Count; i++) {
				IAgent targetSoldier = EnemyAgentsInSight [i];
				if (targetSoldier != null) {
					Vector3 enemyPosition = targetSoldier.GetLocation ();
					bool hFlag = targetSoldier.HasFlag ();
					float tSpotted = Time.time;
					enemySighting.Add (new enemyPresence (enemyPosition, hFlag, tSpotted));

					//Removes excess sightings 
					if (enemySighting.Count > 10) {
						enemySighting.RemoveRange (0, enemySighting.Count - 10);
					}
				}
			}
			//Debug.Log ("Enemy presence count = " + enemySighting.Count);
			//Debug.Log ("ARE THEY AT RIGHT Alley: " + enemyRedAlleyB ());
			//Debug.Log ("ARE THEY AT RIGHT Blue Alley: " + enemyBlueAlleyA ());
			//Debug.Log ("ARE THEY AT Left Blue Alley: " + enemyBlueAlleyB ());
			//Debug.Log ("ARE THEY AT Left Alley: " + enemyRedAlleyA ());
			return true;
		}


	}

	/*-----------------------------*/
	//Targets an enemy to focus on //
	public void PaintEnemyTarget()
	{
		//If no enemies are visible, return
		if (!EnemiesSpotted ()) {return;}

		//Target a single enemy and alert others to focus on it
		IAgent targetSoldier = EnemyAgentsInSight[0];
		if (targetSoldier != null){
			//Tell Teammates of target to focus

		}
	}

	/*------------*/
	// Guard Base // --------------------------------------------------------------------------
	/*------------*/


	/*----------------------------------------*/
	//Find a position to scout and hold ground//
	/*----------------------------------------*/
	public void FindScoutPosition(){
		switch (myTeam) {

		case playerTeam.red: 
			if (NavAgent!= null){
				for (int i = 0; i < redTeamScoutPos.Count; i++) { //Look through available scout positions to go to
					if (!redTeamScoutPos [i].occupied) { //if the point is not occupied
						setTargetPosition(redTeamScoutPos[i].pos); //Sets position to travel to and resets any current locations
						redTeamScoutPos [i].occupied = true; //set the point as occupied
						currentPointLocation = redTeamScoutPos[i]; //Sets the point as our current locations
						scouting = true; //We are scouting a point
						if (redTeamScoutPos [i].area == mapZone.redCourtyardB) {
					
						} else {

						}

						return;
				   }
				}
			}break;

		case playerTeam.blue:
			if (NavAgent!= null){
				//Look through available scout positions to go to
				for (int i = 0; i < blueTeamScoutPos.Count; i++) {
					NavAgent.TargetPosition = (blueTeamScoutPos [0]);
				}
			}break;
		}
	}

	/*----------------------------------------*/
	//Find a position to scout and hold ground//
	/*----------------------------------------*/
	public void FindInitialDefencePosition(){
		switch (myTeam) {

		case playerTeam.red: 
			if (NavAgent!= null){
				for (int i = 0; i < redTeamInitialDefPos.Count; i++) { //Look through available scout positions to go to
					if (!redTeamInitialDefPos[i].occupied) { //if the point is not occupied
						NavAgent.TargetPosition = (redTeamInitialDefPos[i].pos); //set point as new target to move to
						redTeamInitialDefPos[i].occupied = true; //set the point as occupied
						if (currentPointLocation != null) { //If we are on a defence point, reset it before accepting new one
							resetPosition(currentPointLocation);
						}
						currentPointLocation = redTeamInitialDefPos[i]; 
						guarding = true;
						//Debug.Log("IM Heading to " + NavAgent.TargetPosition);
						return;
					}
				}
			}break;

		case playerTeam.blue:
			if (NavAgent!= null){
				//Look through available scout positions to go to
				for (int i = 0; i < blueTeamScoutPos.Count; i++) {
					NavAgent.TargetPosition = (blueTeamScoutPos [0]);
				}
			}break;
		}
	}


	/*----------------------------------------*/
	//Find a position to scout and hold ground//
	/*----------------------------------------*/
	public void FindCourtyardDefencePosition(){
		switch (myTeam) {

		case playerTeam.red: 
			//Check If we aren't already currently defending the courtyard
			if(currentPointLocation == null || (currentPointLocation.area != mapZone.redCourtyardA)){

				if (NavAgent != null) {
					for (int i = 0; i < redTeamCourtyardDefPos.Count; i++) { //Look through available scout positions to go to
						if (!redTeamCourtyardDefPos [i].occupied) { //if the point is not occupied
							setTargetPosition(redTeamCourtyardDefPos [i].pos); //set point as new target to move to
							redTeamCourtyardDefPos [i].occupied = true; //set the point as occupied
							currentPointLocation = redTeamCourtyardDefPos [i]; 
							scouting = false;
							guarding = true;
							return;
						}
					}
				}
			}break;

		case playerTeam.blue:
			if (NavAgent!= null){
				//Look through available scout positions to go to
				for (int i = 0; i < blueTeamScoutPos.Count; i++) {
					NavAgent.TargetPosition = (blueTeamScoutPos [0]);
				}
			}break;
		}
	}



	/*--------------------*/
	//GuardCurrentPosition//
	public void GuardPosition(){
		GetComponent<Rigidbody>().velocity = Vector3.zero; //Sets Velocity to zero, stopping their movement
		transform.rotation = Quaternion.Euler(0,currentPointLocation.orientations[0],0); //Updates the rotation of our agents

		//If we get nudged away from our position by our agent or another try to move back
		if (Vector3.Distance(CurrentLocation,currentPointLocation.pos) > 0.2) {
			NavAgent.TargetPosition = currentPointLocation.pos;
		}
	}

	/*----------------------------------------------------*/
	//Resets position within position lists if it matches //
	public void resetPosition(defencePoint p){
		if (redTeamScoutPos.Contains (p)) { //if its contained
			for (int i = 0; i < redTeamScoutPos.Count; i++) { //search for it 
				if (redTeamScoutPos [i].Equals (p)) { //reset it once found 
					redTeamScoutPos [i].occupied = false;
				}
			}
		}
	}


	/*------------------*/
	//Default Behaviours// ----------------------------------------------------------------------------------------------
	/*------------------*/


	/*------------------------*/
	//Set New point to move to//
	/*------------------------*/
	public void setTargetPosition(Vector3 pos){
		//If we are on a defence point, reset it before accepting new one
		if (currentPointLocation != null) {resetPosition(currentPointLocation);}
		//set point as new target to move to
		NavAgent.TargetPosition = (pos); 
	}


	/*----------------------------------------------*/
	//Rotate around searching for potential enemies //
	public void LookAround(){
		Vector3 tempEuler = transform.eulerAngles; //Create temporary copy of our rotation
		float newRotation = tempEuler.y + 1;
		float x = Mathf.Sin (newRotation * Mathf.Deg2Rad);
		float y = Mathf.Sin (newRotation * Mathf.Deg2Rad);
		LookAt(new Vector3(CurrentLocation.x + x,0,CurrentLocation.z+y));
		//Vector3 tempEuler = transform.eulerAngles; //Create temporary copy of our rotation
		//float newRotation = tempEuler.y + 5; //Increments the Y-rotation by 5
		//transform.rotation = Quaternion.Euler(0,newRotation,0); //Updates the rotation of our agents
	}
		
	/*----------------------------------------------------------------------------*/
	//Takes a number of agents and a radius to find even distribution around point//
	public Vector3 positionAroundPoint(int numAgents, Vector3 position, int radius){
		//Finds Angle segment for that number of agents
		float angleSlice = 2 * Mathf.PI / numAgents;
		//Finds the select angle for that agent
		float agentAngle = angleSlice * agentID;
		//Get X and Z position to head too
		int newX = (int)(position.x + radius * Mathf.Cos (agentAngle));
		int newZ = (int)(position.z + radius * Mathf.Sin (agentAngle));
		return new Vector3 (newX, 0, newZ);
	}

	/*----------------------------*/
	//Check which zone they are in//
	/*----------------------------*/
	public mapZone findZonePos(Vector3 position){
		Vector2 testedPosition = new Vector2 (position.x, position.z); //Converts our position to 2D

		//RedBase
		if (RedTeamBaseRect.Contains (testedPosition, true)) {
			return mapZone.redBase;
		}

		//Red CourtyardA
		if (RedTeamCourtyardARect.Contains (testedPosition, true)) {
			return mapZone.redCourtyardA;
		}

		//Red CourtyardB
		if (RedTeamCourtyardBRect.Contains (testedPosition, true)) {
			return mapZone.redCourtyardB;
		}
			
		//Red AlleyA
		if (RedTeamAlleyARect.Contains (testedPosition, true)) {
			return mapZone.redAlleyA;
		}
			
		//Red AlleyB
		if (RedTeamAlleyBRect.Contains (testedPosition, true)) {
			return mapZone.redAlleyB;
		}

		//BlueBase
		if (BlueTeamBaseRect.Contains (testedPosition, true)) {
			return mapZone.blueBase;
		}

		//Blue CourtyardA
		if (BlueTeamCourtyardARect.Contains (testedPosition, true)) {
			return mapZone.blueCourtyardA;
		}

		//Blue CourtyardB
		if (BlueTeamCourtyardBRect.Contains (testedPosition, true)) {
			return mapZone.blueCourtyardB;
		}

		//Blue AlleyA
		if (BlueTeamAlleyARect.Contains (testedPosition, true)) {
			return mapZone.blueAlleyA;
		}

		//Blue AlleyB
		if (BlueTeamAlleyBRect.Contains (testedPosition, true)) {
			return mapZone.blueAlleyB;
		}
		return mapZone.error;
	}

	/*-----------------------------------------------*/
	//Check if there is enemy presence within a zone //
	/*-----------------------------------------------*/
	//public bool enemyInZone(mapZone zone){
	//	for (int i = 0; i < enemySighting.Count; i++) {
	//		if (enemySighting [i].zone == zone) {
	//			return true;
	//		}
	//	}
	//	return false;
	//}

	/*------------------------------------*/
	//Clears any outdated enemy sightings //
	/*------------------------------------*/
	public void clearOutdatedSightings(){
		for (int i = enemySighting.Count-1; i >=0 ; i--) {
			if (Time.time - enemySighting [i].timeSpotted > sightingTime) {
				enemySighting.RemoveAt (i);
			}
		}
	}

	/*------------------------------*/
	//Default Move to Flag behaviour//
	/*------------------------------*/
	public void MovetoFlag()
    {
        LookAt(EnemyFlagInSight.GetLocation()); //Turns the agent towards the vector position of the Enemy flag in sight 
        MoveTowards(EnemyFlagInSight.GetLocation()); //Moves the agent towards the position of the Enemy flag
    }
		




}

//Properties we have access to:
/*   
     Within Behaviour Library Linker ----------------

     AllAgentsInSight --Returns list of all agents (IAgents) in sight 
     Friendly agents/ Enemy agents in sight -- Returns List of friendly/enemy agents (IAgents) in sight
     AllFlagsInSight -- Returns list off all the flags in sight
     Friendly/Enemy Flag in sight -- Returns true if friendly/enemy flag is in sight
     LocationLastFiredUpon -- Vector position that we were last fired upon from
     Location of enemy spawn -- Vector position of the enemy spawn
     Location of friendly spawn -- Vector position of our spawn
     Current Location -- Our location
     Is Damaged -- If we have been damaged recently
     Has Flag -- If we are currntly holding the flag
     Is Dead -- If we are dead
     Friendly/Enemy Flag taken -- whether the flag has been taken from its base
     LastAgentWhoDied -- Returns Last agent who died (Questionable)

     Within Behaviour Library -----------------------

     Returning to spawn -- Flag that indicates intention to return to base
     UnderAttack -- Returns true whether recently damaged
     AlltargetsDead -- Returns true whether no enemies are spotted
     EnemiesSpotted -- Returns true if atleast one enemy spotted

 */


