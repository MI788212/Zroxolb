using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    public float RollDuration = 0.5f;
    private bool rollingAllowed { get; set; }
    private bool IsRolling { get; set; }

    private float FloorUnit = 1;

    public float initialLockDuration = 0.5f;
    public float fallDuration = 2f;

    private Coroutine rollCoroutine;
    internal Orientation PlayerOrientation { get; set; }

    private Rigidbody rb;

    public InputActionReference rollForward;
    public InputActionReference rollBack;
    public InputActionReference rollLeft;
    public InputActionReference rollRight;

    public float force = 5f; // the amount of force with which you push the block when it falls off the platform
    private bool wholeBlockFall = false; //when falling (fail), is the whole block off the platform or just half of it; important to make physics of falling look correct

    public event Action PlayerFellOffPlatform;
    public event Action PlayerFellIntoHole;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartPlayer(Vector2 initialPlayerPosition)
    {
        Initialize(initialPlayerPosition);

        UnconstrainRB();
        StartCoroutine(DelayRollingAllowed());
    }

    public void Initialize(Vector2 initialPlayerPosition)
    {
        ConstrainRB();

        transform.position = new Vector3(initialPlayerPosition.x, 2, initialPlayerPosition.y);
        transform.eulerAngles = new Vector3(0, 0, 0);
        PlayerOrientation = Orientation.Y;

        IsRolling = false;
        rollingAllowed = false;
        wholeBlockFall = false;
    }

    private IEnumerator DelayRollingAllowed()
    {
        yield return new WaitForSeconds(initialLockDuration);

        ConstrainRB();
        rollingAllowed = true;
    }

    private void ConstrainRB()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void UnconstrainRB()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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

    public void HandlePlayerFall(Vector3 emptyTilePosition)
    {
        if (rollingAllowed)
        {
            rollingAllowed = false;
        }
        else
        {
            wholeBlockFall = true;
            return;
        }

        rollingAllowed = false;

        StartCoroutine(FallOffPlatform(emptyTilePosition));
    }

    public IEnumerator FallOffPlatform(Vector3 emptyTilePosition)
    {

        if (rollCoroutine != null)
        {
            yield return rollCoroutine;
        }

        UnconstrainRB();
        if (wholeBlockFall)
        {
            rb.AddForce(Vector3.down * force, ForceMode.Impulse);
        }
        else
        {
            rb.AddForceAtPosition(Vector3.down * force, emptyTilePosition + Vector3.up * 0.5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(fallDuration);

        PlayerFellOffPlatform?.Invoke();
    }

    public void HandleFallIntoHole()
    {
        if (!rollingAllowed)
            return;

        rollingAllowed = false;

        StartCoroutine(FallIntoHole());
    }

    private IEnumerator FallIntoHole()
    {
        if (rollCoroutine != null)
        {
            yield return rollCoroutine;
        }

        UnconstrainRB();
        rb.AddForce(Vector3.down * force, ForceMode.Impulse);
        yield return new WaitForSeconds(fallDuration);

        PlayerFellIntoHole?.Invoke();
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
        if (!rollingAllowed)
            return;

        //Debug.Log("Roll!");
        rollCoroutine = StartCoroutine(RollToDirection(rollDirection));
    }

    private IEnumerator RollToDirection(Vector3 rollDirection)
    {
        if (!IsRolling)
        {
            //Debug.Log("Rolling starts!");
            IsRolling = true;

            float angle = 90f;
            Vector3 axisOfRotation = GetAxisOfRotation(rollDirection);
            Vector3 pivotOfRotation = GetPivotOfRotation(rollDirection);
            PlayerOrientation = NextOrientation(rollDirection);

            float elapsedTime = 0f;

            while (elapsedTime < RollDuration)
            {
                float previousElapsedTime = elapsedTime;
                elapsedTime = Mathf.Min(elapsedTime + Time.deltaTime, RollDuration);

                float deltaAngle = angle *
                    ((elapsedTime - previousElapsedTime) / RollDuration);

                transform.RotateAround(pivotOfRotation, axisOfRotation, deltaAngle);

                yield return null;
            }

            Correction();

            //Debug.Log("Rolling ends.");
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
