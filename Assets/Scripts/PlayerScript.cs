using Assets.Scripts;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    public float RollDuration = 0.5f;
    private bool rollingAllowed { get; set; }
    private bool IsRolling { get; set; }
    private bool voidRolling;
    private int voidRollsLeft;
    private GameObject currentStar;

    private float FloorUnit = 1;

    public float initialLockDuration = 0.5f;
    public float fallDuration = 2f;
    public float startFallHeight = 10;
    public float gravity = -40f;

    internal Orientation PlayerOrientation { get; set; }

    private Rigidbody rb;

    public InputActionReference rollForward;
    public InputActionReference rollBack;
    public InputActionReference rollLeft;
    public InputActionReference rollRight;

    public float force = 5f; // the amount of force with which you push the block when it falls off the platform

    public event Action PlayerFellOffPlatform;
    public event Action PlayerFellIntoHole;

    private GameObject cube1;
    private GameObject cube2;

    private Material playerMaterial;

    public TMP_Text starPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, gravity, 0);
        initialLockDuration = Mathf.Sqrt(2f * startFallHeight / Mathf.Abs(gravity));
    }

    private void Start()
    {
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

        transform.position = new Vector3(initialPlayerPosition.x, startFallHeight , initialPlayerPosition.y);
        transform.eulerAngles = new Vector3(0, 0, 0);
        PlayerOrientation = Orientation.Y;

        IsRolling = false;
        rollingAllowed = false;
        voidRolling = false;
        voidRollsLeft = 0;
        currentStar = null;

        StarTransform(false);
        starPower.gameObject.SetActive(false);
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

    public IEnumerator FallOffPlatform(bool cube1supported, bool cube2supported)
    {
        //Debug.Log("FallOffPlatform");
        rollingAllowed = false;

        if (cube1supported||cube2supported)
        {
            if (cube2supported)
            {
                GameObject tmp = cube2;
                cube2 = cube1;
                cube1 = tmp;
            }
            float angle = 90;
            float elapsedTime = 0f;
            Vector3 pivotOfRotation = (cube1.transform.position + cube2.transform.position) / 2 + Vector3.down * FloorUnit / 2;
            Vector3 axisOfRotation = Quaternion.Euler(0, 90, 0)*(cube2.transform.position - cube1.transform.position);

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

        UnconstrainRB();

        rb.AddForce(Vector3.down * force, ForceMode.Impulse);

        yield return new WaitForSeconds(fallDuration);

        PlayerFellOffPlatform?.Invoke();
    }
    private IEnumerator FallIntoHole()
    {
        rollingAllowed = false;
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
        StartCoroutine(RollToDirection(rollDirection));
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

            CheckTiles();
            
            IsRolling = false;
        }
    }

    private void CheckTiles()
    {
        TileCheckResult tcr1 = CheckTilesUnderCube(cube1.transform);
        TileCheckResult tcr2 = CheckTilesUnderCube(cube2.transform);

        if (voidRolling)
        {
            voidRollsLeft--;
            starPower.text = "Star Power: " + voidRollsLeft;
            Debug.Log(voidRollsLeft);
            if (voidRollsLeft == 0)
            {
                voidRolling = false;
                StarTransform(false);
                currentStar.SetActive(true);
                currentStar = null;
                starPower.gameObject.SetActive(false);
            }
        }

        if(tcr1.star != null)
        {
            AcquireStar(tcr1.star);
        }
        else if(tcr2.star != null)
        {
            AcquireStar(tcr2.star);
        }

        if(PlayerOrientation == Orientation.Y && !voidRolling)
        {
            if(tcr1.holeTile != null)
            {
                StartCoroutine(FallIntoHole());
                return;
            }

            if(tcr1.weakTile != null)
            {
                WeakTileBreakAndFall(tcr1.weakTile);
                return;
            }

            if(tcr1.switchX != null)
            {
                //Debug.Log("Toggle x switch pls");
                ToggleSwitchTile(tcr1.switchX);
                return;
            }
        }
        
        bool cube1supported = TileSupportsCube(tcr1);
        bool cube2supported = TileSupportsCube(tcr2);

        if(tcr1.switchO != null)
        {
            ToggleSwitchTile(tcr1.switchO);
        }

        if (tcr2.switchO != null)
        {
            if(PlayerOrientation != Orientation.Y)
                ToggleSwitchTile(tcr2.switchO);
        }

        //Debug.Log(cube1supported +" "+ cube2supported);
        if(!voidRolling && ((!cube1supported) || (!cube2supported)))
        {
            StartCoroutine(FallOffPlatform(cube1supported, cube2supported));
        }
    }

    private bool TileSupportsCube(TileCheckResult tcr)
    {
        return tcr.normalTile != null || tcr.toggleTile != null || tcr.weakTile != null|| tcr.holeTile != null;
    }

    private void AcquireStar(GameObject star)
    {
        if (currentStar != null)
        {
            currentStar.SetActive(true);
        }
        currentStar = star;
        voidRolling = true;
        voidRollsLeft = star.GetComponent<Star>().voidRolls;
        currentStar.SetActive(false);
        StarTransform(true);
        starPower.gameObject.SetActive(true);
        starPower.text = "Star Power: " + voidRollsLeft;
    }

    private void ToggleSwitchTile(GameObject switchObject)
    {
        switchObject.GetComponent<SwitchScript>().Toggle();
    }

    private void WeakTileBreakAndFall(GameObject weakTile)
    {
        weakTile.AddComponent<Rigidbody>();
        StartCoroutine(FallOffPlatform(false, false));
    }

    private TileCheckResult CheckTilesUnderCube(Transform cube)
    {
        TileCheckResult tileCheckResult = new TileCheckResult();

        RaycastHit[] hits = Physics.RaycastAll(cube.position, Vector3.down, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        foreach (RaycastHit hit in hits)
        {
            string hitTag = hit.transform.tag;
            switch (hitTag)
            {
                case "NormalTile":
                    tileCheckResult.normalTile = hit.transform.gameObject;
                    break;
                case "WeakTile":
                    tileCheckResult.weakTile = hit.transform.gameObject;
                    break;
                case "SwitchX":
                    tileCheckResult.switchX = hit.transform.gameObject;
                    break;
                case "SwitchO":
                    tileCheckResult.switchO = hit.transform.gameObject;
                    break;
                case "HoleTile":
                    Debug.Log("HoleTile detected");
                    tileCheckResult.holeTile = hit.transform.gameObject;
                    break;
                case "ToggleTile":
                    tileCheckResult.toggleTile = hit.transform.gameObject;
                    break;
                case "Star":
                    tileCheckResult.star = hit.transform.gameObject;
                    break;
                default:
                    //Debug.Log("Raycast hit smth else: " + hit.transform.name);
                    break;
            }
        }
        return tileCheckResult;
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

            Debug.Log("Transform ON!");
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

            Debug.Log("Transform OFF!");
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
