using GDD3400.Labyrinth;
using UnityEngine;

public class ColliderHandler : MonoBehaviour
{
    public bool PlayerPColliding = false;
    public bool IntriguePColliding = false;
    public bool ExcitedPColliding = false;
    public bool PlayerColliding = false;
    public bool WallColliding = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "PlayerP")
        {
            PlayerPColliding = true;
            IntriguePColliding = false;
            ExcitedPColliding = false;
            PlayerColliding = false;
            WallColliding = false;
        }
        else if (other.gameObject.tag == "IntrigueP")
        {
            PlayerPColliding = false;
            IntriguePColliding = true;
            ExcitedPColliding = false;
            PlayerColliding = false;
            WallColliding = false;
        }
        else if (other.gameObject.tag == "ExcitedP")
        {
            PlayerPColliding = false;
            IntriguePColliding = false;
            ExcitedPColliding = true;
            PlayerColliding = false;
            WallColliding = false;
        }
        else if (other.gameObject.tag == "Player")
        {
            PlayerPColliding = false;
            IntriguePColliding = false;
            ExcitedPColliding = false;
            PlayerColliding = true;
            WallColliding = false;
        }
        else if (other.gameObject.layer == 9)
        {
            PlayerPColliding = false;
            IntriguePColliding = false;
            ExcitedPColliding = false;
            PlayerColliding = false;
            WallColliding = true;
        }
        else
        {
            PlayerPColliding = false;
            IntriguePColliding = false;
            ExcitedPColliding = false;
            PlayerColliding = false;
            WallColliding = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PlayerP")
        {
            PlayerPColliding = false;
        }
        else if (other.gameObject.tag == "IntrigueP")
        {
            IntriguePColliding = false;
        }
        else if (other.gameObject.tag == "ExcitedP")
        {
            ExcitedPColliding = false;
        }
        else if (other.gameObject.tag == "Player")
        {
            PlayerColliding = false;
        }
        else if (other.gameObject.layer == 9)
        {
            WallColliding = false;
        }
    }
}