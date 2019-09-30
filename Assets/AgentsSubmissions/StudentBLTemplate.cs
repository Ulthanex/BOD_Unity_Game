using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum of our team//
public enum playerTeam{
	blue,
	red
}
	
//Enum of zones//
public enum mapZone{
	redBase,
	redCourtyardA,
	redCourtyardB,
	redAlleyA,
	redAlleyB,
	blueAlleyA,
	blueAlleyB,
	blueCourtyardB,
	blueCourtyardA,
	blueBase,
	error
}

/*-------------------------------------------------------------------------------------------------------------------*/
//Class for Defensive positions, contains position, whether it's occupied & orientations to hold defensive positions //
public class defencePoint{
	public Vector3 pos;
	public bool occupied = false;
	public mapZone area = mapZone.error;
	public List<float> orientations;

	public defencePoint(Vector3 position, List<float> angles){
		pos = position;
		orientations = angles;
	}

	public defencePoint(Vector3 position, mapZone z,List<float> angles){
		pos = position;
		area = z;
		orientations = angles;
	}
}

/*------------------------------------------*/
// Located enemy presence at a certain time //
public class enemyPresence{
	public Vector3 pos; //The position of the enemy at time sighted
	public bool hasOurFlag; //Whether they have our flag
	public float timeSpotted; //The time in which it was spotted

	public enemyPresence(Vector3 p, bool f, float t){
		pos = p;
		hasOurFlag = f;
		timeSpotted = t;
	}
}


//Extends the BasicBehaviourLibrary which provides access to the different available sensors, properties should only be made
//Using the properties provided with no access to the agent class
public class StudentBLTemplate : BasicBehaviourLibrary {

	/*------------------*/
	//Personal variables//
	/*------------------*/
	private int agentID; //ID of the agent talking
	private playerTeam myTeam; //Team of the agent determined through spawn position 
	private defencePoint currentPointLocation = null; //The current point we are defending if we are
	private bool guarding = false; //Whether we are guarding a point (i.e., shoot at enemies seen)
	private bool aggressive = false; //Whether we are taking an aggresive approach
	private bool capturingFlag = false;
	private Vector3 correctivePointA;
	private Vector3 correctivePointB;
	private float correctiveFloat = 0.3f;

	/*---------------------------------*/
	// Static Communication Variables  //
	/*---------------------------------*/
	protected static List<Vector3> teamMemberLocations = new List<Vector3>(); //List of Vector3 for our agents
	protected static List<enemyPresence> enemySighting = new List<enemyPresence>(); //List of enemy Sightings
	protected static enemyPresence targettedSighting = null; //Painted target to focus
	protected static enemyPresence enemyFlagHolder = null; //Last Sighting of the enemy flag carrier
	protected static Vector3 enemyFlagPosition; //current position of the enemy flag, in case our teammates die
	private static IGrabable enemyFlag = null; //Reference to the flag enabling immediate pickup when placed ontop

	protected static float sightingTime = 10; //How long we keep hold of a sighting
	protected static float paintedTime = 3; //How long we keep hold of a painted sighting
	protected static float enemyFlagTime = 3; //How long we keep hold of a flag sighting
	protected static int memberCount = 0; //Incrementor for members
	protected bool initComplete = false; //Flag that is set after initial communication setup

	/*-------*/
	//Senses //----------------------------------------------------------------------------------------------------------------
	/*-------*/
	public bool InitialisationComplete(){return initComplete;} //Done Initialisation
	public bool GuardingPoint(){return guarding;} //Whether We Are guarding a point
	public bool FlagAtBase(){return !FriendlyFlagTaken;} //Whether our flag is at base
	public bool PaintedTarget(){ 
		if (targettedSighting != null) {
			//Check whether it should be binned due to time difference
			if (Time.time - targettedSighting.timeSpotted > paintedTime) {
				targettedSighting = null;
			}

			//if we are not aggressive yet we don't want to go to far
			if (targettedSighting != null) {
				if (Vector3.Distance (SpawnLocation, targettedSighting.pos) > 12f && aggressive == false) {
					targettedSighting = null;
				}
			}

		}
		return targettedSighting != null;} //Whether we have a painted target 
	public bool OurFlagSighting(){ 
		if (enemyFlagHolder != null) {
			//Check whether it should be binned due to time difference
			if (Time.time - enemyFlagHolder.timeSpotted > enemyFlagTime) {
				enemyFlagHolder = null;
			}
		}
		return enemyFlagHolder != null;} //Whether we have a painted target 
	public bool Aggression(){return aggressive;}
	public bool IsHoldingFlag(){
		if (HasFlag) { 
			enemyFlagPosition = CurrentLocation;
			return true;
		}
		return false;
	}
	public bool SpottedEnemyFlag(){
		if (enemyFlag == null) {
			if (EnemyFlagInSight != null) {
				enemyFlag = EnemyFlagInSight;
			}
		}
		return true;

	}
	public bool IsCapturingFlag(){return capturingFlag;}

	/*----------*/
	//Map Points//
	/*----------*/

	public static List<defencePoint> redTeamBaseDefPos = new List<defencePoint> (){ 
		new defencePoint (new Vector3 (-24f, 0f, -16f),mapZone.redBase, new List<float> (){50f}),
		new defencePoint (new Vector3 (-17f, 0f, -13f),mapZone.redBase, new List<float> (){270f}),
		new defencePoint (new Vector3 (-12.5f, 0f, -13.5f),mapZone.redBase, new List<float> (){220f}),
		new defencePoint (new Vector3 (-6f, 0f, -22.5f),mapZone.redBase, new List<float> (){320f}),
		new defencePoint (new Vector3 (-7f, 0f, -24f), mapZone.redBase,new List<float> (){325f})};

	public static List<defencePoint> blueTeamBaseDefPos = new List<defencePoint> (){ 
		new defencePoint (new Vector3 (24f, 0f, 16f),mapZone.blueBase, new List<float> (){230f}),
		new defencePoint (new Vector3 (17f, 0f, 13f),mapZone.blueBase, new List<float> (){90f}),
		new defencePoint (new Vector3 (12.5f, 0f, 13.5f),mapZone.blueBase, new List<float> (){40f}),
		new defencePoint (new Vector3 (6f, 0f, 22.5f),mapZone.blueBase, new List<float> (){140f}),
		new defencePoint (new Vector3 (7f, 0f, 24f), mapZone.blueBase,new List<float> (){145f})};


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

        //Enemy Flag Position setup
		enemyFlagPosition = EnemySpawnLocation;
			
		//Communication--------------------------------------------------
		agentID = announceSelf (); //Initialises AgentId within the Team

		//Timer for defensive Strategy-----------------------------------
		StartCoroutine(becomeAggressive()); //Starts a timer in which we consider becoming aggressive if we haven't seen anyone

		//Timer for corrective Path Finding
		StartCoroutine(correctivePathing());

		//Sets Flag to prevent re-initialisation
		initComplete = true; 
	}
		

	/*-------------------------------------------*/
	//Changes Strategy after a set amount of time//
	IEnumerator becomeAggressive(){
		yield return new WaitForSeconds(45); //Wait for 45 seconds
		aggressive = true;
	}

	/*--------------------------------*/
	//If it detects that we are stuck //
	IEnumerator correctivePathing(){
		while (true) {
			correctivePointA = CurrentLocation;
			yield return new WaitForSeconds(5); //Wait for 5 seconds
			correctivePointB = CurrentLocation;
			if (NavAgent.pathGenerated.Count > 0  && EnemiesSpotted() == false  && !IsDead) {
				if (Vector3.Distance (correctivePointA, correctivePointB) < correctiveFloat) {
					NavAgent.pathGenerated = new List<Vector3>();
				}

			}
		}
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

					//Adds New Sightings
					enemySighting.Add (new enemyPresence (enemyPosition,hFlag, tSpotted));

					//If its the first enemy we have seen -- Paint it as a target location to roam too
					if (targettedSighting == null) {
						targettedSighting = new enemyPresence (enemyPosition,hFlag, tSpotted);
					}

					//If the guy is carrying our flag update our sighting
					if(hFlag == true){
						enemyFlagHolder = new enemyPresence (enemyPosition,hFlag, tSpotted);

					} 

					//Removes excess sightings 
					if (enemySighting.Count > 10) {
						enemySighting.RemoveRange (0, enemySighting.Count - 10);
					}

				}
			}
			return true;
		}
	}
		
	/*--------------------------------------*/
	//Alerts team Members of being attacked //
	public bool IsUnderAttack(){
		//If we have been damaged and we don't have a sighting of an enemy
		if (IsDamaged) {
			if (targettedSighting == null) {
				targettedSighting = new enemyPresence (LocationLastIncomingFire,false,Time.time);
			}
		}
		return IsDamaged;
	}


	/*---------------------------------------------------------------------------------*/
	//Investigate Targetted location as long as we dont stray to far from defence point//
	public void InvestigateOurFlagSighting(){
		setPreciseTargetPosition(enemyFlagHolder.pos);
	}


	/*---------------------------------------------------------------------------------*/
	//Investigate Targetted location as long as we dont stray to far from defence point//
	public void InvestigateLocation(){
		setPreciseTargetPosition(targettedSighting.pos);
	}

	/*------------*/
	// Guard Base // --------------------------------------------------------------------------
	/*------------*/


	/*----------------------------------------*/
	//Find a position to scout and hold ground//
	/*----------------------------------------*/
	public void FindBaseDefencePosition(){
		
		switch (myTeam) {

		case playerTeam.red: 
			//Check If we aren't already currently defending the courtyard
			if(currentPointLocation == null || (currentPointLocation.area != mapZone.redBase)){
				if (NavAgent != null) {
					
					for (int i = 0; i < redTeamBaseDefPos.Count; i++) { //Look through available scout positions to go to
						if (!redTeamBaseDefPos [i].occupied) { //if the point is not occupied
							redTeamBaseDefPos [i].occupied = true; //set the point as occupied
							setPreciseTargetPosition (redTeamBaseDefPos [i].pos); //set point as new target to move to
							currentPointLocation = redTeamBaseDefPos [i]; 
							guarding = true;
							return;
						}
					}
				}
			}break;

        /*---------*/
        //Blue Team//
		/*---------*/

		case playerTeam.blue:
			//Check If we aren't already currently defending the courtyard
			if(currentPointLocation == null || (currentPointLocation.area != mapZone.blueBase)){
				if (NavAgent != null) {

					for (int i = 0; i < blueTeamBaseDefPos.Count; i++) { //Look through available scout positions to go to
						if (!blueTeamBaseDefPos [i].occupied) { //if the point is not occupied
							blueTeamBaseDefPos [i].occupied = true; //set the point as occupied
							setPreciseTargetPosition (blueTeamBaseDefPos [i].pos); //set point as new target to move to
							currentPointLocation = blueTeamBaseDefPos [i]; 
							guarding = true;
							return;
						}
					}
				}
			}break;
		}
	}

	/*-----------------------*/
	//Stands perfectly still //
	public void StandStill(){
		GetComponent<Rigidbody>().velocity = Vector3.zero; //Sets Velocity to zero, stopping their movement
	}

	/*--------------------*/
	//GuardCurrentPosition//
	public void GuardPosition(){
		StandStill ();
		rotateToAngle (currentPointLocation.orientations [0]);

		//If we get nudged away from our position by our agent or another try to move back
		//if (Vector3.Distance(CurrentLocation,currentPointLocation.pos) > 0.2) {
		//	NavAgent.TargetPosition = currentPointLocation.pos;
		//}
	}

	/*----------------------------------------------------*/
	//Resets position within position lists if it matches //
	public void resetPosition(defencePoint p){
		for (int i = 0; i < redTeamBaseDefPos.Count; i++) { //search for it 
			if (redTeamBaseDefPos[i].pos.Equals(p.pos)) { //reset it once found 
				redTeamBaseDefPos [i].occupied = false;
			}
		}

		for (int i = 0; i < blueTeamBaseDefPos.Count; i++) { //search for it 
			if (blueTeamBaseDefPos[i].pos.Equals(p.pos)) { //reset it once found 
				blueTeamBaseDefPos [i].occupied = false;
			}
		}
		currentPointLocation = null;
	}


	/*------------------*/
	//Default Behaviours// ----------------------------------------------------------------------------------------------
	/*------------------*/


	/*------------------------*/
	//Set New point to move to//
	/*------------------------*/
	public void setPreciseTargetPosition(Vector3 pos){
		//If we are on a defence point, reset it before accepting new one
		if (currentPointLocation != null) {resetPosition(currentPointLocation);}
		//Clear any uncompleted paths
		NavAgent.pathGenerated = new List<Vector3>();
		//set point as new target to move to
		NavAgent.TargetPosition = (pos); 
		guarding = false;
	}

	/*---------------------------------------------*/
	//Set New point to move to with a radius offset//
	/*---------------------------------------------*/
	public void setOffsetTargetPosition(Vector3 pos){
		//If we are on a defence point, reset it before accepting new one
		if (currentPointLocation != null) {resetPosition(currentPointLocation);}
		//Clear any uncompleted paths
		NavAgent.pathGenerated = new List<Vector3>();
		//set point as new target to move to
		NavAgent.TargetPosition =  positionAroundPoint(5,pos,0.7f); 
		guarding = false;

	}

	/*----------------------------------------------*/
	//Rotate around searching for potential enemies //
	public void LookAround(){
		Vector3 tempEuler = transform.eulerAngles; //Create temporary copy of our rotation
		float newRotation = tempEuler.y + 5;
		rotateToAngle (newRotation);
	}

	/*--------------------------------*/
	//Rotate Towards Spotted Sighting //
	public void LookTowardsSighting(){
		LookAt (targettedSighting.pos);
	}

	/*----------------------------*/
	//Rotate Towards Spotted Flag //
	public void LookTowardsFlag(){
		LookAt (enemyFlagHolder.pos);
	}
		
	/*----------------------------------------------------------------------------*/
	//Takes a number of agents and a radius to find even distribution around point//
	public Vector3 positionAroundPoint(int numAgents, Vector3 position, float radius){
		//Finds Angle segment for that number of agents
		float angleSlice = 2 * Mathf.PI / numAgents;
		//Finds the select angle for that agent
		float agentAngle = angleSlice * agentID;
		//Get X and Z position to head too
		int newX = (int)(position.x + radius * Mathf.Cos (agentAngle));
		int newZ = (int)(position.z + radius * Mathf.Sin (agentAngle));
		return new Vector3 (newX, 0, newZ);
	}
		
	/*------------------------------------*/
	//Clears any outdated enemy sightings //
	/*------------------------------------*/
	public void clearOutdatedSightings(){
		for (int i = enemySighting.Count-1; i >=0 ; i--) {
			if (Time.time - enemySighting [i].timeSpotted > sightingTime) {
				enemySighting.RemoveAt (i);
			}
		}
		if (targettedSighting != null) {
			if (Time.time - targettedSighting.timeSpotted >  paintedTime) {
				targettedSighting = null;
			}
		}
	}

	/*-------------------------*/
	//Rotates to a given angle //
	public void rotateToAngle(float ang){
		float x = Mathf.Sin (ang * Mathf.Deg2Rad);
		float y = Mathf.Cos (ang * Mathf.Deg2Rad);
		LookAt(new Vector3(CurrentLocation.x + x,0,CurrentLocation.z+y));
	}

	/*-----------------------------------------------------------------*/
	//If there are any enemies spotted, target first soldier and shoot // 
	public void FindEnemiesInSight()
	{
		Vector3 targetPosition = Vector3.zero; //Current position we are targetting
		int lowestHealth = 999; //Current lowest health enemy we have seen
		bool hasFlag = false; //Whether or not one of them has the flag

		//If No enemies, exit
		if (!EnemiesSpotted())
			return;

		//For All Enemies in view, target the weakest / flag holder
		for (int i = 0; i < EnemyAgentsInSight.Count; i++) {
			IAgent targetSoldier = EnemyAgentsInSight [i];
			// Any enemy soldiers in sight?
			if (targetSoldier != null) {

				//If Lowest health enemy so far
				if (targetSoldier.GetHealth() < lowestHealth && !hasFlag) {
					targetPosition = targetSoldier.GetLocation ();
					lowestHealth = targetSoldier.GetHealth ();
				}

				//If they have the flag -- Refresh flag spotted and target them
				if (targetSoldier.HasFlag ()) {
					bool hFlag = targetSoldier.HasFlag ();
					float tSpotted = Time.time;
					enemyFlagHolder = new enemyPresence (targetSoldier.GetLocation(), hFlag, tSpotted);

					targetPosition = targetSoldier.GetLocation ();
					hasFlag = true;
				}
			}
		}

		//Turns to look at our selected position
		LookAt (targetPosition);
	}


	/*------------------------------------------*/
	//Goes to grab enemy flag if it is in sight //
	public void Grab()
	{
		if (enemyFlag != null)
		{
			GrabFlag(enemyFlag);
		}
	}

	//---------------------------------------------------//
	//Sets Target position of nav mesh towards Enemy flag//
	public void SetPathToEnemyFlag()
	{
		if (NavAgent!= null)
		{
			setOffsetTargetPosition (enemyFlagPosition);
		}
	}

	//-------------------------------------------------------------------------------------//
	//Resets flag indicating intention to return to base, setting target to Spawn location //
	public void CaptureTheFlag()
	{
		capturingFlag = true;
		if (NavAgent!= null)
		{
			setPreciseTargetPosition (SpawnLocation);
		}
	}


	/*-------------------------------------------------------------------------------*/
	//Safely Returned to Base with flag, resets flag location and undoes return flag //
	public void CappedFlag(){
		capturingFlag = false; //We have captured the flag
		enemyFlagPosition = EnemySpawnLocation; //Reset known location of flag
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
	

