using GDD3400.Labyrinth;
using UnityEngine;

public class ColliderHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Pheromone")
        {
            GetComponentInParent<EnemyAgent>().PheromoneCollisionHandler(this.gameObject, other);
        }
        else if (other.gameObject.tag == "Player")
        {
            GetComponentInParent<EnemyAgent>().PlayerCollisionHandler(this.gameObject, other);
        }
        else if(other.gameObject.tag == "Wall")
        {
            GetComponentInParent<EnemyAgent>().WallCollisionHandler(this.gameObject, other);
        }
    }
}