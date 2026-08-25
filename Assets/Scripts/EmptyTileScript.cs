using UnityEngine;

public class EmptyTileScript : MonoBehaviour
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
            playerScript.HandlePlayerFall(transform.position);
        }
    }
}
