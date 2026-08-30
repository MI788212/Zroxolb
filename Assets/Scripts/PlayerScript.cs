using Assets.Scripts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerScript : MonoBehaviour
{
    public float RollDuration = 0.1f;
    public float Gravity = -40f;
    public float StageUnit = 1;
    public float StartFallHeight = 10;
    private float initialLockDuration;
    public float FallDuration = 1f;

    private bool rollingAllowed;
    private bool voidRolling;
    private int voidRollsLeft;
    private GameObject currentStar;

    internal Orientation playerOrientation;

    private GameObject cube1;
    private GameObject cube2;

    private Rigidbody rb;
    private Material playerMaterial;

    public InputActionReference RollForward;
    public InputActionReference RollBack;
    public InputActionReference RollLeft;
    public InputActionReference RollRight;

    public event Action PlayerFellOffPlatform;
    public event Action PlayerFellIntoHole;
    public event Action Rolled;
    public event Action<int> UpdatedStarPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        Physics.gravity = new Vector3(0, Gravity, 0);
        initialLockDuration = Mathf.Sqrt(2f * StartFallHeight / Mathf.Abs(Gravity));

        cube1 = transform.Find("Cube1").gameObject;
        cube2 = transform.Find("Cube2").gameObject;

        playerMaterial = transform.Find("player").transform.GetComponent<Renderer>().material;
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

        transform.position = new Vector3(initialPlayerPosition.x, StartFallHeight , initialPlayerPosition.y);
        transform.eulerAngles = new Vector3(0, 0, 0);
        playerOrientation = Orientation.Y;

        rollingAllowed = false;
        voidRolling = false;
        voidRollsLeft = 0;
        currentStar = null;
        StarTransform(false);
        UpdatedStarPower?.Invoke(0);
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
        RollForward.action.started += OnRollForward;
        RollBack.action.started += OnRollBack;
        RollLeft.action.started += OnRollLeft;
        RollRight.action.started += OnRollRight;
    }

    private void OnDisable()
    {
        RollForward.action.started -= OnRollForward;
        RollBack.action.started -= OnRollBack;
        RollLeft.action.started -= OnRollLeft;
        RollRight.action.started -= OnRollRight;
    }

    private void OnRollForward(InputAction.CallbackContext context)
    {
        Roll(Vector3.forward);
    }

    private void OnRollBack(InputAction.CallbackContext context)
    {
        Roll(Vector3.back);
    }
    private void OnRollLeft(InputAction.CallbackContext context)
    {
        Roll(Vector3.left);
    }

    private void OnRollRight(InputAction.CallbackContext context)
    {
        Roll(Vector3.right);
    }

    private void Roll(Vector3 rollDirection)
    {
        if (!rollingAllowed)
            return;
        StartCoroutine(RollToDirection(rollDirection));
    }

    private IEnumerator RollToDirection(Vector3 rollDirection)
    {
        //Debug.Log("Rolling starts!");
        rollingAllowed = false;

        float angle = 90f;
        Vector3 axisOfRotation = GetAxisOfRotation(rollDirection);
        Vector3 pivotOfRotation = GetPivotOfRotation(rollDirection);
        playerOrientation = NextOrientation(rollDirection);

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

        Rolled?.Invoke();

        rollingAllowed = CheckState();
    }

    private bool CheckState()
    {
        StageUnitState sus1 = new StageUnitState(cube1.transform);
        StageUnitState sus2 = new StageUnitState(cube2.transform);

        if (voidRolling)
        {
            voidRollsLeft--;
            UpdatedStarPower?.Invoke(voidRollsLeft);
            if (voidRollsLeft == 0)
            {
                ReleaseStar();
            }
        }

        if(sus1.star != null)
        {
            AcquireStar(sus1.star);
        }
        else if(sus2.star != null)
        {
            AcquireStar(sus2.star);
        }

        if(playerOrientation == Orientation.Y && !voidRolling)
        {
            if(sus1.holeTile != null)
            {
                StartCoroutine(FallIntoHole());
                return false;
            }

            if(sus1.weakTile != null)
            {
                sus1.weakTile.GetComponent<WeakTile>().TileBreak();
                StartCoroutine(FallOffPlatform(false, false));
                return false;
            }

            if(sus1.switchX != null)
            {
                sus1.switchX.GetComponent<SwitchScript>().Toggle();
            }
        }
        
        if (sus1.switchO != null)
        {
            sus1.switchO.GetComponent<SwitchScript>().Toggle();
        }

        if (sus2.switchO != null)
        {
            if(playerOrientation != Orientation.Y)
                sus2.switchO.GetComponent<SwitchScript>().Toggle();
        }

        bool cube1supported = TileSupportsCube(sus1);
        bool cube2supported = TileSupportsCube(sus2);

        if (!voidRolling && ((!cube1supported) || (!cube2supported)))
        {
            StartCoroutine(FallOffPlatform(cube1supported, cube2supported));
            return false;
        }

        return true;
    }

    private bool TileSupportsCube(StageUnitState tcr)
    {
        return tcr.normalTile != null || tcr.toggleTile != null || tcr.weakTile != null|| tcr.holeTile != null;
    }

    private void AcquireStar(GameObject star)
    {
        if (currentStar != null)
        {
            ReleaseStar();
        }
        currentStar = star;
        voidRolling = true;
        voidRollsLeft = star.GetComponent<Star>().voidRolls;
        currentStar.SetActive(false);
        StarTransform(true);
        UpdatedStarPower?.Invoke(voidRollsLeft);
    }

    private void ReleaseStar()
    {
        currentStar.SetActive(true);
        currentStar = null;
        voidRolling = false;
        StarTransform(false);
        UpdatedStarPower?.Invoke(voidRollsLeft);
    }

    private void StarTransform(bool on)
    {
        if (on)
        {
            Material mat = playerMaterial;

            mat.SetFloat("_Surface", 1f);

            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);

            mat.renderQueue = (int)RenderQueue.Transparent;

            Color color = mat.GetColor("_BaseColor");
            color.a = 0.8f;
            mat.SetColor("_BaseColor", color);

            mat.SetFloat("_Metallic", 0.15f);
        }
        else {
            Material mat = playerMaterial;
            mat.SetFloat("_Surface", 0f);

            mat.SetFloat("_SrcBlend", (float)BlendMode.One);
            mat.SetFloat("_DstBlend", (float)BlendMode.Zero);
            mat.SetFloat("_ZWrite", 1f);

            mat.renderQueue = (int)RenderQueue.Geometry;

            Color color = mat.GetColor("_BaseColor");
            color.a = 1f;
            mat.SetColor("_BaseColor", color);

            mat.SetFloat("_Metallic", 0f);
        }
    }
    public IEnumerator FallOffPlatform(bool cube1supported, bool cube2supported)
    {
        if (cube1supported || cube2supported)
        {
            yield return AdjustPlayerBeforeFall(cube2supported);
        }

        UnconstrainRB();

        yield return new WaitForSeconds(FallDuration);

        PlayerFellOffPlatform?.Invoke();
    }

    private IEnumerator AdjustPlayerBeforeFall(bool cube2supported)
    {
        if (cube2supported)
        {
            GameObject tmp = cube2;
            cube2 = cube1;
            cube1 = tmp;
        }
        float angle = 90;
        float elapsedTime = 0f;
        Vector3 pivotOfRotation = (cube1.transform.position + cube2.transform.position) / 2 + Vector3.down * StageUnit / 2;
        Vector3 axisOfRotation = Quaternion.Euler(0, 90, 0) * (cube2.transform.position - cube1.transform.position);

        while (elapsedTime < RollDuration)
        {
            float previousElapsedTime = elapsedTime;
            elapsedTime = Mathf.Min(elapsedTime + Time.deltaTime, RollDuration);

            float deltaAngle = angle *
                ((elapsedTime - previousElapsedTime) / RollDuration);

            transform.RotateAround(pivotOfRotation, axisOfRotation, deltaAngle);

            yield return null;
        }
    }
    private IEnumerator FallIntoHole()
    {
        UnconstrainRB();

        yield return new WaitForSeconds(FallDuration);

        PlayerFellIntoHole?.Invoke();
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
        if (playerOrientation == Orientation.Y)
        {
            pivotOfRotation += Vector3.down * StageUnit + rollDirection * StageUnit / 2;
        }
        else if(playerOrientation == Orientation.X)
        {
            pivotOfRotation += Vector3.down * StageUnit / 2;
            if(rollDirection==Vector3.forward || rollDirection == Vector3.back)
            {
                pivotOfRotation += rollDirection * StageUnit / 2;
            }
            else
            {
                pivotOfRotation += rollDirection * StageUnit;
            }
        }
        else
        {
            pivotOfRotation += Vector3.down * StageUnit / 2;
            if (rollDirection == Vector3.left || rollDirection == Vector3.right)
            {
                pivotOfRotation += rollDirection * StageUnit / 2;
            }
            else
            {
                pivotOfRotation += rollDirection * StageUnit;
            }
        }
        return pivotOfRotation;
    }

    private Orientation NextOrientation(Vector3 rollDirection)
    {
        if(playerOrientation == Orientation.Y)
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
        else if (playerOrientation == Orientation.X)
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
        float x = RoundToUnit(transform.position.x, StageUnit / 2);
        float y = RoundToUnit(transform.position.y, StageUnit / 2);
        float z = RoundToUnit(transform.position.z, StageUnit / 2);
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
