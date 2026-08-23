using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    public Vector2 InitialPlayerPosition = new Vector2(0, 0);
    public float RollDuration = 0.5f;
    public float initialLockDuration = 0.5f;
    private float FloorUnit = 1;
    private bool IsRolling { get; set; }
    private Coroutine rollCoroutine;
    private Orientation PlayerOrientation { get; set; }

    private Rigidbody rb;

    private bool gameOver { get; set; }
    private bool rollingAllowed { get; set; }

    public InputActionReference rollForward;
    public InputActionReference rollBack;
    public InputActionReference rollLeft;
    public InputActionReference rollRight;

    public float gravity = -10f;
    public float force = 5f;

    private bool wholeBlockFalling = false;

    private void Awake()
    {
        Physics.gravity = new Vector3(0, gravity, 0);
        rb = GetComponent<Rigidbody>();
        Initialize();
    }

    private void Initialize()
    {
        ConstrainRB();
        transform.position = new Vector3(InitialPlayerPosition.x, 2, InitialPlayerPosition.y);
        transform.eulerAngles = new Vector3(0, 0, 0);
        PlayerOrientation = Orientation.Y;

        IsRolling = false;
        gameOver = false;
        rollingAllowed = false;
        wholeBlockFalling = false;
    }

    private void Start()
    {
        BeginGame();
    }

    private void BeginGame()
    {
        UnconstrainRB();
        StartCoroutine(DelayRollingAllowed());
    }

    private IEnumerator DelayRollingAllowed()
    {
        float elapsedTime = 0f;

        while (elapsedTime < initialLockDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ConstrainRB();
        rollingAllowed = true;
    }

    private void ConstrainRB()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    private void UnconstrainRB()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;
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

    public void GameOver(Vector3 emptyTilePosition)
    {
        if (gameOver)
        {
            wholeBlockFalling = true;
            return;
        }

        gameOver = true;
        rollingAllowed = false;

        StartCoroutine(GameOverCoroutine(emptyTilePosition));
    }

    public IEnumerator GameOverCoroutine(Vector3 emptyTilePosition)
    {

        if (rollCoroutine != null)
        {
            yield return rollCoroutine;
        }

        UnconstrainRB();
        if (!wholeBlockFalling)
        {
            rb.AddForceAtPosition(Vector3.down * force, emptyTilePosition + Vector3.up * 0.5f, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(Vector3.down * force, ForceMode.Impulse);
        }

        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        float restartDelay = 2f;

        float elapsedTime = 0f;

        while (elapsedTime < restartDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Initialize();
        BeginGame();
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
        if (gameOver||!rollingAllowed)
            return;

        Debug.Log("Roll!");
        rollCoroutine = StartCoroutine(RollToDirection(rollDirection));
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
                float previousElapsedTime = elapsedTime;
                elapsedTime = Mathf.Min(elapsedTime + Time.deltaTime, RollDuration);

                float deltaAngle = angle *
                    ((elapsedTime - previousElapsedTime) / RollDuration);

                transform.RotateAround(pivotOfRotation, axisOfRotation, deltaAngle);

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
