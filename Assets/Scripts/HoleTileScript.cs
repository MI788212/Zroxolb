using UnityEngine;

public class HoleTileScript : MonoBehaviour
{
    private PlayerScript playerScript;
    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerScript.PlayerOrientation == Orientation.Y)
            {
                playerScript.HandleFallIntoHole();
            }
        }
    }
}
