using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    public bool startOn = false;
    private bool IsOn = false;
    private GameObject toggleTiles;
    private void Start()
    {
        toggleTiles = transform.parent.Find("ToggleTiles").transform.gameObject;
        Set(startOn);
    }
    public void Toggle()
    {
        IsOn = !IsOn;
        toggleTiles.SetActive(IsOn);
    }

    private void Set(bool on)
    {
        toggleTiles.SetActive(on);
        IsOn = on;
    }

}
