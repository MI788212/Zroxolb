using Assets.Scripts;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Switch : MonoBehaviour
{
    public bool startOn = false;
    private bool IsOn = false;
    private GameObject toggleTiles;
    private void Start()
    {
        toggleTiles = transform.parent.Find("ToggleTiles").transform.gameObject;
        IsOn = true;

        Set(startOn);

    }
    public void Toggle()
    {
        IsOn = !IsOn;

        toggleTiles.SetActive(IsOn);
        //foreach(Transform toggleTile in toggleTiles.transform)
        //{
        //    StartCoroutine(RotateToggleTile(toggleTile.gameObject, 3));
        //}
    }

    private IEnumerator RotateToggleTile(GameObject toggleTile, float rotationDuration)
    {
        float angle = 180;
        Vector3 pivotOfRotation = toggleTile.transform.Find("Hinge").transform.position;
        Vector3 axisOfRotation = toggleTile.transform.Find("Hinge").transform.eulerAngles;

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            float previousElapsedTime = elapsedTime;
            elapsedTime = Mathf.Min(elapsedTime + Time.deltaTime, rotationDuration);

            float deltaAngle = angle *
                ((elapsedTime - previousElapsedTime) / rotationDuration);

            toggleTile.transform.RotateAround(pivotOfRotation, axisOfRotation, deltaAngle);

            yield return null;
        }
    }
    private void Set(bool on)
    {
        if (!on)
        {
            Toggle();
        }
    }

}
