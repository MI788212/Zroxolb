using UnityEngine;

public class WeakTile : MonoBehaviour
{
    public void TileBreak()
    {
        gameObject.AddComponent<Rigidbody>();
    }
}
