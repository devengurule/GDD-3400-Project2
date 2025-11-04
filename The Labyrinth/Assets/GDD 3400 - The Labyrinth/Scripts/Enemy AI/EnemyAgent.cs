using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace GDD3400.Labyrinth
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAgent : MonoBehaviour
    {
        #region Variables
        [SerializeField] private LevelManager _levelManager;

        private bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }
        [SerializeField] private float turnRate = 10f;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] private float leavingPathDistance = 2f; // This should not be less than 1
        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] private float minimumPathDistance = 6f;
        [SerializeField] private GameObject pheromonePrefab;

        private Vector3 velocity;
        private Vector3 floatingTarget;
        private Vector3 destinationTarget;
        List<PathNode> path;
        private Rigidbody rb;
        private LayerMask wallLayer;
        private bool DEBUG_SHOW_PATH = true;
        Transform[] childObjects;
        

        #endregion

        #region Unity Methods
        public void Awake()
        {
            // Grab and store the rigidbody component
            rb = GetComponent<Rigidbody>();

            // Grab and store the wall layer
            wallLayer = LayerMask.GetMask("Walls");
        }

        public void Start()
        {
            childObjects = GetComponentsInChildren<Transform>();

            // If we didn't manually set the level manager, find it
            if (_levelManager == null) _levelManager = FindAnyObjectByType<LevelManager>();

            // If we still don't have a level manager, throw an error
            if (_levelManager == null) Debug.LogError("Unable To Find Level Manager");
        }

        public void Update()
        {
            // Loops through all child objects
            foreach (Transform childObject in childObjects)
            {

            }

            if (!isActive) return;

            DecisionMaking();
        }

        #endregion

        //#region Path Following

        //// Perform path following
        //private void PathFollowing()
        //{
        //    // TODO: Implement path following

        //    int closestNodeIndex = GetClosestNode();
        //    int nextNodeIndex = closestNodeIndex + 1;

        //    PathNode targetNode = null;

        //    if (nextNodeIndex < path.Count)
        //    {
        //        targetNode = path[nextNodeIndex];
        //    }
        //    else
        //    {
        //        targetNode = path[closestNodeIndex];
        //    }

        //    floatingTarget = targetNode.transform.position;
        //}

        //// Public method to set the destination target
        //public void SetDestinationTarget(Vector3 destination)
        //{
        //    // TODO: Implement destination target setting

        //    destinationTarget = destination;


        //    // If straight line distance to target is greater than a minimum then do pathfinding
        //    if (Vector3.Distance(transform.position, destination) > minimumPathDistance)
        //    {
        //        PathNode startNode = _levelManager.GetNode(transform.position);
        //        PathNode endNode = _levelManager.GetNode(destination);

        //        if (startNode == null || endNode == null)
        //        {
        //            // We have a problem
        //            print("Error with startNode or endNode");
        //            return;
        //        }

        //        // Path to follow
        //        path = Pathfinder.FindPath(startNode, endNode);

        //        StartCoroutine(DrawPathDebugLines(path));
        //    }
        //    // Move directly to the destination
        //    else
        //    {
        //        floatingTarget = destination;
        //    }


        //}

        //// Get the closest node to the player's current position
        //private int GetClosestNode()
        //{
        //    int closestNodeIndex = 0;
        //    float closestDistance = float.MaxValue;

        //    for (int i = 0; i < path.Count; i++)
        //    {
        //        float distance = Vector3.Distance(transform.position, path[i].transform.position);
        //        if (distance < closestDistance)
        //        {
        //            closestDistance = distance;
        //            closestNodeIndex = i;
        //        }
        //    }
        //    return closestNodeIndex;
        //}

        ///// <summary>
        ///// Move along node path
        ///// </summary>
        //private void FixedUpdate()
        //{
        //    if (!isActive) return;


        //    Debug.DrawLine(this.transform.position, floatingTarget, Color.green);

        //    // If we have a floating target and we are not close enough to it, move towards it
        //    if (floatingTarget != Vector3.zero && Vector3.Distance(transform.position, floatingTarget) > stoppingDistance)
        //    {
        //        // Calculate the direction to the target position
        //        Vector3 direction = (floatingTarget - transform.position).normalized;

        //        direction.y = 0f;
        //        // Calculate the movement vector
        //        velocity = direction * maxSpeed;
        //    }

        //    // If we are close enough to the floating target, slow down
        //    else
        //    {
        //        velocity *= .9f;
        //    }

        //    // Calculate the desired rotation towards the movement vector
        //    if (velocity != Vector3.zero)
        //    {
        //        Quaternion targetRotation = Quaternion.LookRotation(velocity);

        //        // Smoothly rotate towards the target rotation based on the turn rate
        //        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate);
        //    }

        //    rb.linearVelocity = velocity;
        //}

        //#endregion

        #region Decision Making
        private void DecisionMaking()
        {
            
        }

        #endregion

        #region Perception

        public void PheromoneCollisionHandler(GameObject child, Collision collidedObject)
        {
            
        }
        public void PlayerCollisionHandler(GameObject child, Collision collidedObject)
        {
            
        }
        public void WallCollisionHandler(GameObject child, Collision collidedObject)
        {
            
        }

        #endregion

        #region Actions



        #endregion

        #region DebugUI
        private IEnumerator DrawPathDebugLines(List<PathNode> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, Color.red, 3.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }

        #endregion
    }
}
