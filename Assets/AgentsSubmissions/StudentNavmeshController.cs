using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//Extends Default Navmesh Controller (A*)
public class StudentNavmeshController : NavmeshController
{
    public void TestMethod()
    {
		//this.gameObject.AddComponent<NavMeshObstacle> ();
		NavMeshAgent test = GetComponent<NavMeshAgent>();
		test.avoidancePriority = Random.Range (1, 50);
    }
}
