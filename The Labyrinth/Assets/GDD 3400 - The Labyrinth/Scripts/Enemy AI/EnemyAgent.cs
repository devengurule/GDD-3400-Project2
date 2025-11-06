using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Header("Turning Settings")]
        [SerializeField] private float turnRate = 1f;
        [SerializeField] private float maxTurnRate = 10f;
        [SerializeField] private float sharpMultiplier = 2f;
        [SerializeField] private float swayAmount = 1f;
        [SerializeField] private float swayTime = 1f;
        [SerializeField] private float turningTime;
        [SerializeField] private float reverseTime;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 0.1f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [SerializeField] private float stoppingTime = 0.1f;
        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] private float leavingPathDistance = 2f; // This should not be less than 1
        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] private float minimumPathDistance = 6f;

        [Header("Pheromon Settings")]
        [SerializeField] private GameObject pheromonePrefab;
        [SerializeField] private Color intrigueColor;
        [SerializeField] private Color excitedColor;
        [SerializeField] private float intrigueLifeTime;
        [SerializeField] private float excitedLifeTime;
        [SerializeField] private float spawnCooldown;
        private string intrigueTag = "IntrigueP";
        private string excitedTag = "ExcitedP";
        private Color currentColor;
        private float currentLifeTime;
        private string currentTag;

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
        private float currentSpeed;
        private bool turningRight;
        private bool turningLeft;
        private bool reversing;
        
        private float timeCounter = 0f;
        private float currentYAngle = 0f;
        private Timer turningTimer;
        private Timer reverseTimer;
        private Timer spawningTimer;

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
            //transform.rotation = Quaternion.Euler(0, Random.Range(0f,360f), 0);


            turningTimer = gameObject.AddComponent<Timer>();
            reverseTimer = gameObject.AddComponent<Timer>();
            spawningTimer = gameObject.AddComponent<Timer>();

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
            if (playerColliding)
            {
                // Colliding with player



                // Drop excited pheromone
                if (!spawningTimer.IsRunning())
                {
                    spawningTimer.Run(spawnCooldown);
                    SpawnPheromone(excitedColor, excitedLifeTime, excitedTag);
                }
            }
            else if (playerPColliding || intriguePColliding || excitedPColliding)
            {
                // Drop intriuge pheromone when hitting player pheromone
                // Drop excited pheromone when colliding with player
                // Follow all other pheromones
                if (playerPColliding)
                {

                    // Drop intrigue pheromone
                    if (!spawningTimer.IsRunning())
                    {
                        spawningTimer.Run(spawnCooldown);
                        SpawnPheromone(intrigueColor, intrigueLifeTime, intrigueTag);
                    }

                    if (FrontCollider.PlayerPColliding)
                    {
                        Forward();
                        if (LeftCollider.PlayerPColliding)
                        {
                            TurnRightSharp();
                        }
                        else if (RightCollider.PlayerPColliding)
                        {
                            TurnLeftSharp();
                        }
                    }
                    else if (LeftCollider.PlayerPColliding)
                    {
                        Forward();
                        TurnRightSharp();
                    }
                    else if (RightCollider.PlayerPColliding)
                    {
                        Forward();
                        TurnLeftSharp();
                    }
                }
                else if (excitedPColliding) {
                    if (FrontCollider.ExcitedPColliding)
                    {
                        Forward();
                        if (LeftCollider.ExcitedPColliding)
                        {
                            TurnRightSharp();
                        }
                        else if (RightCollider.ExcitedPColliding)
                        {
                            TurnLeftSharp();
                        }
                    }
                    else if (LeftCollider.ExcitedPColliding)
                    {
                        Forward();
                        TurnRightSharp();
                    }
                    else if (RightCollider.ExcitedPColliding)
                    {
                        Forward();
                        TurnLeftSharp();
                    }
                }
                else if (intriguePColliding) {
                    if (FrontCollider.IntriguePColliding)
                    {
                        Forward();
                        if (LeftCollider.IntriguePColliding)
                        {
                            TurnRightSharp();
                        }
                        else if (RightCollider.IntriguePColliding)
                        {
                            TurnLeftSharp();
                        }
                    }
                    else if (LeftCollider.IntriguePColliding)
                    {
                        Forward();
                        TurnRightSharp();
                    }
                    else if (RightCollider.IntriguePColliding)
                    {
                        Forward();
                        TurnLeftSharp();
                    }
                }
            }
            else if (wallColliding || turningTimer.IsRunning() || reverseTimer.IsRunning())
            {
                if (FrontCollider.WallColliding && LeftCollider.WallColliding && RightCollider.WallColliding || reverseTimer.IsRunning())
                {
                    // Head into a wall or corner
                    if (Random.Range(0f, 1f) > 0.5f)
                    {
                        Reverse();
                        turningRight = true;
                        turningLeft = false;
                        if (!turningTimer.IsRunning()) turningTimer.Run(turningTime);
                    }
                    else
                    {
                        Reverse();
                        turningRight = true;
                        turningLeft = false;
                        if (!turningTimer.IsRunning()) turningTimer.Run(turningTime);
                    }
                }
                else if(FrontCollider.WallColliding && LeftCollider.WallColliding)
                {
                    // Hitting front and left
                    Reverse();
                    turningRight = true;
                    turningLeft = false;
                    if(!turningTimer.IsRunning()) turningTimer.Run(turningTime);
                }
                else if(FrontCollider.WallColliding && RightCollider.WallColliding)
                {
                    // Hitting front and right
                    Reverse();
                    turningRight = true;
                    turningLeft = false;
                    if (!turningTimer.IsRunning()) turningTimer.Run(turningTime);
                }
                else if (FrontCollider.WallColliding)
                {
                    // Only hitting front
                    if (!reverseTimer.IsRunning()) reverseTimer.Run(reverseTime);
                    if (reverseTimer.IsRunning()) ReverseSlow();
                    if (Random.Range(0f, 1f) > 0.5f)
                    {
                        turningLeft = true;
                        turningRight = false;
                        TurnRight();
                    }
                    else
                    {
                        turningLeft = false;
                        turningRight = true;
                        TurnLeft();
                    }
                }
                else if (LeftCollider.WallColliding || turningLeft)
                {
                    // Only hitting left
                    if (!turningTimer.IsRunning() && !turningLeft) turningTimer.Run(turningTime);
                    turningRight = false;
                    turningLeft = true;
                    ForwardSlow();
                }
                else if (RightCollider.WallColliding || turningRight)
                {
                    // Only hitting right
                    if (!turningTimer.IsRunning() && !turningRight) turningTimer.Run(turningTime);
                    turningLeft = false;
                    turningRight = true;
                    ForwardSlow();
                }

            }
            else
            {
                Forward();
                currentYAngle = 0f;
            }
            Move();

            if (!turningTimer.IsRunning())
            {
                turningLeft = false;
                turningRight = false;
                currentYAngle = 0f;
            }
            //Debug.Log(currentYAngle);
            if (turningLeft && !turningRight)
            {
                TurnLeft();
            }
            else if (turningRight && !turningLeft)
            {
                TurnRight();
            }
            if (turningTimer.GetTimeLeft() > turningTimer.GetDurration() / 2)
            {
                // Halfway through timer
                currentYAngle += turnRate;
            }
            else
            {
                currentYAngle -= turnRate;
            }
            currentYAngle = Mathf.Clamp(currentYAngle, -maxTurnRate, maxTurnRate);
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
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + (swayAmount * Mathf.Cos(Time.time * swayTime)), 0);

            currentSpeed += acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            velocity = transform.forward * currentSpeed;
        }
        private void ForwardSlow()
        {
            currentSpeed += acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed/3, maxSpeed/3);
            velocity = transform.forward * currentSpeed;
        }

        private void Reverse()
        {
            currentSpeed -= acceleration * 2;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            velocity = transform.forward * currentSpeed;
        }

        private void ReverseSlow()
        {
            currentSpeed -= acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
            velocity = transform.forward * currentSpeed;
        }

        private void TurnLeft()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y - currentYAngle, 0);
        }

        private void TurnRight()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + currentYAngle, 0);
        }

        private void TurnLeftSharp()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y - currentYAngle * sharpMultiplier, 0);
        }
        private void TurnRightSharp()
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + currentYAngle * sharpMultiplier, 0);
        }

        private void SpawnPheromone(Color color, float lifeTime, string tag)
        {
            GameObject pheromone = Instantiate(pheromonePrefab, transform.position, Quaternion.identity);
            pheromone.GetComponent<Pheomone>().Initialize(color, lifeTime, tag);
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
