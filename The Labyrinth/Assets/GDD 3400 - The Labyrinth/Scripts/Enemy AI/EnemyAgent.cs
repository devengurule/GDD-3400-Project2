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
        [SerializeField] private float acceleration = 0.1f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [SerializeField] private float stoppingTime = 0.1f;
        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] private float leavingPathDistance = 2f; // This should not be less than 1
        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] private float minimumPathDistance = 6f;
        
        
        [SerializeField] private GameObject pheromonePrefab;


        private Vector3 velocity = Vector3.zero;
        private Vector3 floatingTarget;
        private Vector3 destinationTarget;
        List<PathNode> path;
        private Rigidbody rb;
        private LayerMask wallLayer;
        private bool DEBUG_SHOW_PATH = true;
        private Transform[] childObjects;

        private bool playerPColliding = false;
        private bool intriguePColliding = false;
        private bool excitedPColliding = false;
        private bool playerColliding = false;
        private bool wallColliding = false;

        private ColliderHandler FrontCollider;
        private ColliderHandler LeftCollider;
        private ColliderHandler RightCollider;
        private Vector3 target;
        private float currentSpeed;
        private bool turningRight;
        private bool turningLeft;
        [SerializeField] private float turningTime;
        private float timeCounter = 0;

        #endregion

        public void SetTarget(Vector3 newTarget)
        {
            target = newTarget;
        }

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

            // Get colliders and store them for use later
            foreach (Transform child in childObjects)
            {
                if (child.gameObject.tag == "Front")
                {
                    FrontCollider = child.gameObject.GetComponent<ColliderHandler>();
                }
                else if (child.gameObject.tag == "Left")
                {
                    LeftCollider = child.gameObject.GetComponent<ColliderHandler>();
                }
                else if (child.gameObject.tag == "Right")
                {
                    RightCollider = child.gameObject.GetComponent<ColliderHandler>();
                }
            }



            // If we didn't manually set the level manager, find it
            if (_levelManager == null) _levelManager = FindAnyObjectByType<LevelManager>();

            // If we still don't have a level manager, throw an error
            if (_levelManager == null) Debug.LogError("Unable To Find Level Manager");
        }

        public void FixedUpdate()
        {
            if (!isActive) return;
            Perception();
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
            if (playerPColliding || intriguePColliding || excitedPColliding)
            {
                // Colliding with a pheromone
            }
            else if (playerColliding)
            {
                // Colliding with player
            }
            else if (wallColliding || turningLeft || turningRight)
            {
                // Colliding with wall

                if (FrontCollider.WallColliding && !LeftCollider.WallColliding && !RightCollider.WallColliding)
                {
                    // Colliding in front
                    Reverse();
                    if (Random.Range(1, 2) == 1)
                    {
                        TurnLeft();
                    }
                    else TurnRight();
                }
                else if (LeftCollider.WallColliding && !FrontCollider.WallColliding && !RightCollider.WallColliding || turningRight)
                {
                    // Colliding to left
                    ForwardSlow();
                    TurnRight();
                    turningRight = true;
                }
                else if (RightCollider.WallColliding && !FrontCollider.WallColliding && !LeftCollider.WallColliding || turningLeft)
                {
                    // Colliding to right
                    ForwardSlow();
                    TurnLeft();
                    turningLeft = true;
                }
                else if (FrontCollider.WallColliding && LeftCollider.WallColliding && !RightCollider.WallColliding)
                {
                    // Colliding to front and left
                    Reverse();
                    TurnRight();
                }
                else if (FrontCollider.WallColliding && RightCollider.WallColliding && !LeftCollider.WallColliding)
                {
                    // Colliding to front and right
                    Reverse();
                    TurnLeft();
                }
                else if(FrontCollider.WallColliding && LeftCollider.WallColliding && RightCollider.WallColliding || turningLeft || turningRight)
                {
                    if (Random.Range(1, 2) == 1)
                    {
                        TurnLeft();
                    }
                    else TurnRight();
                }
            }
            else
            {
                if (!turningLeft && !turningRight) Forward();
            }
            Move();

            if (turningLeft || turningRight)
            {
                if (timeCounter < turningTime)
                {
                    timeCounter += Time.deltaTime;
                }
                else
                {
                    turningLeft = false;
                    turningRight = false;
                    timeCounter = 0;
                }
            }
        }

        #endregion

        #region Perception

        private void Perception()
        {
            if (FrontCollider.PlayerColliding || RightCollider.PlayerColliding || LeftCollider.PlayerColliding)
            {
                // Colliding With Player Takes First Priority
                playerPColliding = false;
                intriguePColliding = false;
                excitedPColliding = false;
                playerColliding = true;
                wallColliding = false;
                return;
            }
            else if (FrontCollider.PlayerPColliding || RightCollider.PlayerPColliding || LeftCollider.PlayerPColliding || FrontCollider.IntriguePColliding || RightCollider.IntriguePColliding || LeftCollider.IntriguePColliding || FrontCollider.ExcitedPColliding || RightCollider.ExcitedPColliding || LeftCollider.ExcitedPColliding)
            {
                // Colliding with Pheromone Takes Second Priority
                if(FrontCollider.PlayerPColliding || RightCollider.PlayerPColliding || LeftCollider.PlayerPColliding)
                {
                    playerPColliding = true;
                    intriguePColliding = false;
                    excitedPColliding = false;
                    playerColliding = false;
                    wallColliding = false;
                }
                else if (FrontCollider.IntriguePColliding || RightCollider.IntriguePColliding || LeftCollider.IntriguePColliding)
                {
                    playerPColliding = false;
                    intriguePColliding = true;
                    excitedPColliding = false;
                    playerColliding = false;
                    wallColliding = false;
                }
                else if (FrontCollider.ExcitedPColliding || RightCollider.ExcitedPColliding || LeftCollider.ExcitedPColliding)
                {
                    playerPColliding = false;
                    intriguePColliding = false;
                    excitedPColliding = true;
                    playerColliding = false;
                    wallColliding = false;
                }
                return;
            }
            else if(FrontCollider.WallColliding || RightCollider.WallColliding || LeftCollider.WallColliding)
            {
                // Colliding with Wall Takes Last Priority
                playerPColliding = false;
                intriguePColliding = false;
                excitedPColliding = false;
                playerColliding = false;
                wallColliding = true;
            }
            else
            {
                playerPColliding = false;
                intriguePColliding = false;
                excitedPColliding = false;
                playerColliding = false;
                wallColliding = false;
            }
            
        }

        #endregion

        #region Actions
        private void Move()
        {
            rb.linearVelocity = velocity;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }

        private void Forward()
        {
            if (acceleration < 0) acceleration *= -1;
            currentSpeed += acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            velocity = transform.forward * currentSpeed;
        }
        private void ForwardSlow()
        {
            currentSpeed += acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed/2, maxSpeed/2);
            velocity = transform.forward * currentSpeed;
        }

        private void Reverse()
        {
            currentSpeed -= acceleration * 1.5f;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            velocity = transform.forward * currentSpeed;
        }

        private void TurnLeft()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y - turnRate, 0);
        }

        private void TurnRight()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + turnRate, 0);
        }

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
