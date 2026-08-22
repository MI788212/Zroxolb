using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    public Vector2 InitialPlayerPosition = new Vector2(0, 0);
    public float RollDuration = 0.5f;
    private float FloorUnit = 1;
    private bool IsRolling { get; set; }
    private Orientation PlayerOrientation { get; set; }

    public InputActionReference rollForward;
    public InputActionReference rollBack;
    public InputActionReference rollLeft;
    public InputActionReference rollRight;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        IsRolling = false;
        PlayerOrientation = Orientation.Y;
    }

    private void OnEnable()
    {
        rollForward.action.started += RollForward;
        rollBack.action.started += RollBack;
        rollLeft.action.started += RollLeft;
        rollRight.action.started += RollRight;
    }
    private void OnDisable()
    {
        rollForward.action.started -= RollForward;
        rollBack.action.started -= RollBack;
        rollLeft.action.started -= RollLeft;
        rollRight.action.started -= RollRight;
    }

    private void RollForward(InputAction.CallbackContext context)
    {
        Roll(Vector3.forward);
    }

    private void RollBack(InputAction.CallbackContext context)
    {
        Roll(Vector3.back);
    }
    private void RollLeft(InputAction.CallbackContext context)
    {
        Roll(Vector3.left);
    }

    private void RollRight(InputAction.CallbackContext context)
    {
        Roll(Vector3.right);
    }

    private void Roll(Vector3 rollDirection)
    {
        Debug.Log("Roll!");
        StartCoroutine(RollToDirection(rollDirection));
    }

    private IEnumerator RollToDirection(Vector3 rollDirection)
    {
        if (!IsRolling)
        {
            Debug.Log("Rolling starts!");
            IsRolling = true;

            float angle = 90f;
            Vector3 axisOfRotation = GetAxisOfRotation(rollDirection);
            Vector3 pivotOfRotation = GetPivotOfRotation(rollDirection);
            PlayerOrientation = NextOrientation(rollDirection);

            float elapsedTime = 0f;

            while (elapsedTime < RollDuration)
            {
                elapsedTime += Time.deltaTime;

                transform.RotateAround(pivotOfRotation, axisOfRotation, (angle * (Time.deltaTime / RollDuration)));
                yield return null;
            }

            Correction();

            Debug.Log("Rolling ends.");
            IsRolling = false;
        }
    }
    private Vector3 GetAxisOfRotation(Vector3 rollDirection)
    {
        if (rollDirection == Vector3.back)
            return Vector3.left;
        if (rollDirection == Vector3.forward)
            return Vector3.right;
        if (rollDirection == Vector3.left)
            return Vector3.forward;
        if (rollDirection == Vector3.right)
            return Vector3.back;
        else return Vector3.zero;
    }

    private Vector3 GetPivotOfRotation(Vector3 rollDirection)
    {
        Vector3 pivotOfRotation = transform.position;
        if (PlayerOrientation == Orientation.Y)
        {
            pivotOfRotation += Vector3.down * FloorUnit + rollDirection * FloorUnit / 2;
        }
        else if(PlayerOrientation == Orientation.X)
        {
            pivotOfRotation += Vector3.down * FloorUnit / 2;
            if(rollDirection==Vector3.forward || rollDirection == Vector3.back)
            {
                pivotOfRotation += rollDirection * FloorUnit / 2;
            }
            else
            {
                pivotOfRotation += rollDirection * FloorUnit;
            }
        }
        else
        {
            pivotOfRotation += Vector3.down * FloorUnit / 2;
            if (rollDirection == Vector3.left || rollDirection == Vector3.right)
            {
                pivotOfRotation += rollDirection * FloorUnit / 2;
            }
            else
            {
                pivotOfRotation += rollDirection * FloorUnit;
            }
        }
        return pivotOfRotation;
    }

    private Orientation NextOrientation(Vector3 rollDirection)
    {
        if(PlayerOrientation == Orientation.Y)
        {
            if(rollDirection == Vector3.left || rollDirection == Vector3.right)
            {
                return Orientation.X;
            }
            else
            {
                return Orientation.Z;
            }
        }
        else if (PlayerOrientation == Orientation.X)
        {
            if(rollDirection == Vector3.left || rollDirection == Vector3.right)
            {
                return Orientation.Y;
            }
            else
            {
                return Orientation.X;
            }
        }
        else
        {
            if (rollDirection == Vector3.left || rollDirection == Vector3.right)
            {
                return Orientation.Z;
            }
            else
            {
                return Orientation.Y;
            }
        }
    }
    private void Correction()
    {
        float x = RoundToUnit(transform.position.x, FloorUnit / 2);
        float y = RoundToUnit(transform.position.y, FloorUnit / 2);
        float z = RoundToUnit(transform.position.z, FloorUnit / 2);
        transform.position = new Vector3(x, y, z);
        x = RoundToUnit(transform.eulerAngles.x, 90);
        y = RoundToUnit(transform.eulerAngles.y, 90);
        z = RoundToUnit(transform.eulerAngles.z, 90);
        transform.eulerAngles = new Vector3(x, y, z);
    }
    private float RoundToUnit(float value, float unit)
    {
        return (float)Math.Round(value / unit) * unit;
    }
}


public enum Orientation
{
    X,  //standing
    Y,  //laying along x axis
    Z   //laying along z axis
}
