using UnityEngine;

public class TitleScreenPlayer : MonoBehaviour
{
    public float rotationSpeed = 90f;
    void Update()
    {
        transform.Rotate(-rotationSpeed * Time.deltaTime, 0f, 0f);
    }
}
