using System.Collections.Generic;
using UnityEngine;

//Basic Behaviour Library -- should not be changed
public class BasicBehaviourLibrary : BehaviourLibraryLinker
{
    /*
    NAVIGATOR ------------------------------------------------------------------------------------------------------------------------
    */
    private bool returningToSpawn = false; //Determines whether agent is returning to spawn


	//Returns true is Agent has generated nav path
    public bool DoesPathExist(){return NavAgent.pathGenerated.Count > 0;}

	//-------------------------------------------------------------//
	//Sets Target position of nav mesh towards Enemy Spawn location//
    public void SetPathToEnemyBase()
    {
        returningToSpawn = false;

        if (NavAgent!= null)
        {
            NavAgent.TargetPosition = (EnemySpawnLocation);
        }
    }

	//------------------------------------------------------//
	//Sets Random positon to travel to within the world grid//
    public void SetPathToRandom()
    {
        Vector2 pos;
        pos.x = (float)Random.Range(-GridManager.instance.gridSize.x * 0.5f, GridManager.instance.gridSize.x * 0.5f);
        pos.y = (float)Random.Range(-GridManager.instance.gridSize.y * 0.5f, GridManager.instance.gridSize.y * 0.5f);

        if (NavAgent != null)
            NavAgent.TargetPosition = (pos);
    }

	//-----------------------------------------------------------------------//
	//Turns agents to look at next Navigation point within the path generated//
    public void LookAtNextNavPoint()
    {
        LookAt(NavAgent.pathGenerated[0]);
    }

	//---------------------------------------------------//
	//Moves to next navigation point within the path set //
    public void MoveToNextNode()
    {
        if (NavAgent.pathGenerated.Count > 0)
        {
            MoveTowards(NavAgent.pathGenerated[0]);
        }
    }

	//-------------------------------------------------------------------------------------//
	//Resets flag indicating intention to return to base, setting target to Spawn location //
    public void ReturnToBase()
    {
        if (!returningToSpawn)
        {
            returningToSpawn = true;
            NavAgent.TargetPosition = SpawnLocation;
        }
    }

	//----------------------------------------------//
	//Looks towards locations of last incoming fire //
    public void LookAtDamage()
    {
        LookAt(LocationLastIncomingFire);
    }



    /*
    COMBAT ----------------------------------------------------------------------------------------------------------------------------
    */


    public bool UnderAttack(){return IsDamaged;} //Determines whether the agent has been attacked within a recent set of time
    public bool AllTargetsAreDead(){return !EnemiesSpotted();} //Determines whether no enemies are currently spotted in the vicinity
	public bool EnemiesSpotted(){return EnemyAgentsInSight.Count > 0;} //Returns true if atleast 1 agent is spotted

	/*-----------------------------------------------------------------*/
	//If there are any enemies spotted, target first soldier and shoot // 
    public void ShootEnemiesInSight()
    {
        if (!EnemiesSpotted())
            return;

        IAgent targetSoldier = EnemyAgentsInSight[0];
        // Any enemy soldiers in sight?
        if (targetSoldier != null)
        {
            // Look at them, if we are, shoot.
            if (LookAt(targetSoldier.GetLocation()))
            {
                // pew pew
                Shoot();
            }
        }
    }

	/*-----------------------------------*/
	//Shoots current weapon if available //
    public void Shoot()
    {
        if (IsDead)
            return;
        CurrentWeapon.Shoot();
    }



    /*
    Flag ------------------------------------------------------------------------------------------------------------------------------------------------
    */
    public bool HoldsFlag(){return HasFlag;} //Returns true if holding flag
    public bool EnemyFlagNotInBase(){return EnemyFlagTaken;} //Returns true if the enemy flag is not present within the base
    public bool EnemyTeamFlagInSight(){return EnemyFlagInSight != null;} //Returns true if enemy flag is visible

	/*------------------------------------------*/
	//Goes to grab enemy flag if it is in sight //
    public void GrabEnemyTeamFlag()
    {
        if (EnemyTeamFlagInSight())
        {
            GrabFlag(EnemyFlagInSight);
        }
    }


}