using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MovementInputResponse
{
    public float horizontal;
    public float vertical;
}

[RequireComponent(typeof(PlayerMovementModule))]
public class ConeRunner : MonoBehaviour
{
    [SerializeField] private int _playerNumber;
    public int PlayerNumber { get => _playerNumber; set => _playerNumber = value; }
    public bool IsActive = false;
    protected float ang;
    protected float startAngle;
    protected float currentRotationAngle;
    protected float rotationSpeed = 1f;
    protected bool initialAngleSet = false;
    public float speed = 1;
    private float startSpeed;
    [SerializeField] private float highSpeedMod = 2f;
    [SerializeField] private LayerMask collisionLayer;
    [HideInInspector] public bool IsHoldingStuff;
    private float collisionOffsetOfHoldingHands = 300f;

    [SerializeField] protected Animator characterAnimator;
    protected float minHeight = 280f;
    protected float maxHeight = 3700f;

    public float coneAngleFactor = 1.66f;
    private bool rotationLocked = false;

    private MovementModule movementModule;

    private Vector3 downV = Vector3.zero;
    private Collider mainCollider;
    private List<Collider> surroundingColliders = new List<Collider>();

    private void Start()
    {
        AssignCollider();
        movementModule = GetComponent<PlayerMovementModule>();
        startSpeed = speed;
    }

    private void AssignCollider()
    {
        if (GetComponent<MeshCollider>())
        {
            mainCollider = GetComponent<MeshCollider>();
        }
        else if (GetComponent<CapsuleCollider>())
        {
            mainCollider = GetComponent<CapsuleCollider>();
        }
        else if (GetComponent<BoxCollider>())
        {
            mainCollider = GetComponent<BoxCollider>();
        }
        else if (GetComponent<SphereCollider>())
        {
            mainCollider = GetComponent<SphereCollider>();
        }
    }

    public void AssignCollider(Collider cldr)
    {
        mainCollider = cldr;
    }

    public MovementInputResponse MovementInput()
    {
        MovementInputResponse response = new MovementInputResponse();
        response.horizontal = ControlSetup.Instance.GetAxis(PlayerNumber, "Horizontal");
        response.vertical = -ControlSetup.Instance.GetAxis(PlayerNumber, "Vertical");

        return response;
    }

    private void Update()
    {
        if (!IsActive)
        {
            return;
        }

        Movement();
        SetPos();
    }

    private void Reset()
    {
        ang = 0;
        startAngle = 0;
        currentRotationAngle = 0;
        initialAngleSet = false;
        rotationLocked = false;
    }

    public float GetMinHeight()
    {
        return minHeight;
    }

    public float GetMaxHeight()
    {
        return maxHeight;
    }

    public float GetMainColliderHeight()
    {
        return mainCollider.bounds.extents.y;
    }

    public float GetMainColliderBounds()
    {
        return mainCollider.bounds.extents.x;
    }

    public Animator GetAnimator()
    {
        return characterAnimator;
    }

    public void LockRotation()
    {
        rotationLocked = true;
    }

    public float GetCurrentRotation()
    {
        return currentRotationAngle;
    }

    public void SetCurrentRotation(float rot)
    {
        currentRotationAngle = rot;
    }

    float lastDiff = 0;

    private void SetPos()
    {
        if (!initialAngleSet)
        {
            ang = ConePositions.Instance.CalculateAngle(transform.position);
            startAngle = ang;
            initialAngleSet = true;
        }

        if (lastDiff != 0)
        {
            transform.position -= new Vector3(0, -lastDiff, 0);
        }

        CalculatePositionResponse cpr = ConePositions.Instance.CalculatePosition(transform.position.y, ang);
        Vector3 upVector = Quaternion.AngleAxis(ConePositions.Instance.angle, cpr.crossVector) * Vector3.up;
        Vector3 downVector = Quaternion.AngleAxis(180 - ConePositions.Instance.angle, cpr.crossVector) * -Vector3.up;
        downV = downVector;

        if (!rotationLocked)
        {
            Vector3 rotationVector = Quaternion.AngleAxis(currentRotationAngle, upVector) * downVector;
            transform.rotation = Quaternion.LookRotation(rotationVector, upVector);
        }

        transform.position = cpr.posVector;
    }

    public Vector3 GetDownVector()
    {
        CalculatePositionResponse cpr = ConePositions.Instance.CalculatePosition(transform.position.y, ang);
        Vector3 upVector = Quaternion.AngleAxis(ConePositions.Instance.angle, cpr.crossVector) * Vector3.up;
        Vector3 downVector = Quaternion.AngleAxis(180 - ConePositions.Instance.angle, cpr.crossVector) * -Vector3.up;

        return downVector;
    }

    public Vector3 GetUpVector()
    {
        CalculatePositionResponse cpr = ConePositions.Instance.CalculatePosition(transform.position.y, ang);
        Vector3 upVector = Quaternion.AngleAxis(ConePositions.Instance.angle, cpr.crossVector) * Vector3.up;
        Vector3 downVector = Quaternion.AngleAxis(180 - ConePositions.Instance.angle, cpr.crossVector) * -Vector3.up;

        return upVector;
    }

    public Vector3 GetCrossVector()
    {
        CalculatePositionResponse cpr = ConePositions.Instance.CalculatePosition(transform.position.y, ang);

        return cpr.crossVector;
    }

    private void AlignToCone(float y, Vector3 upVector)
    {
        // TODO: Magic number, why 4000?
        float upFactor = (((4000 - y) / (4000 - 0)) - 0.5f) * (-1) * 80;
        float lastTransformY = transform.position.y;
        transform.position += upVector.normalized * upFactor;
        lastDiff = lastTransformY - transform.position.y;
    }

    public IEnumerator LerpAngle(float newAngle, float time)
    {
        if (currentRotationAngle == newAngle)
        {
            currentRotationAngle = newAngle;
            yield break;
        }

        float startTime = Time.time;
        float startAngle = currentRotationAngle;

        if (startAngle > 360)
        {
            startAngle -= 360;
        }

        if (startAngle < -360)
        {
            startAngle += 360;
        }

        if (newAngle > 360)
        {
            newAngle -= 360;
        }

        if (newAngle < -360)
        {
            newAngle += 360;
        }

        while (startTime + time > Time.time)
        {
            currentRotationAngle = MathHelper.LerpAngle(currentRotationAngle * Mathf.Deg2Rad, newAngle * Mathf.Deg2Rad, (Time.time - startTime) / time) * Mathf.Rad2Deg;
            yield return new WaitForEndOfFrame();
        }

        currentRotationAngle = newAngle;
    }

    IEnumerator ChangeRotationSmoothly(Quaternion start, Quaternion end)
    {
        float startTime = Time.time;

        while (startTime + 0.1f > Time.time)
        {
            transform.rotation = Quaternion.Lerp(start, end, (Time.time - startTime) / 0.1f);
            yield return new WaitForEndOfFrame();
        }

        transform.rotation = end;
    }

    protected void Movement()
    {
        movementModule.HandleMovement();
    }

    public void HandleCollisions(ref Vector2 movementVector, CalculatePositionResponse cpr)
    {
        Vector2 originalMovementVector = movementVector;
        surroundingColliders.Clear();
        surroundingColliders.AddRange(
            Physics.OverlapSphere(cpr.posVector, Mathf.Max(mainCollider.bounds.extents.x, mainCollider.bounds.extents.z), collisionLayer)
            .Where(a => a.gameObject != gameObject).ToArray()
        );

        if (IsHoldingStuff)
        {
            Collider[] colliders2 = Physics.OverlapSphere(
                    cpr.posVector + (transform.forward * collisionOffsetOfHoldingHands), mainCollider.bounds.extents.x, collisionLayer
                )
                .Where(a => a.gameObject != gameObject).ToArray();
            surroundingColliders.AddRange(colliders2);
        }

        bool hasXBeenReversed = false;
        bool hasYBeenReversed = false;
        Vector2 vectorsToSubtract = Vector2.zero;

        for (int i = 0; i < surroundingColliders.Count; i++)
        {
            Collider col = surroundingColliders[i];
            Vector3 pointOnBounds = col.ClosestPoint(cpr.posVector);
            Vector3 normalizedDir = (pointOnBounds - cpr.posVector).normalized;
            normalizedDir = (Quaternion.AngleAxis((ang - coneAngleFactor) * Mathf.Rad2Deg, Vector3.up) * normalizedDir);

            if (normalizedDir.x < 0.1f && normalizedDir.x > -0.1f)
            {
                normalizedDir.x = 0;
            }

            if (normalizedDir.y < 0.1f && normalizedDir.y > -0.1f)
            {
                normalizedDir.y = 0;
            }

            Vector2 vToSubtract = new Vector2(normalizedDir.x, normalizedDir.y).normalized;
            vectorsToSubtract += vToSubtract;

            if (((originalMovementVector.x >= 0 && movementVector.x <= 0) || (originalMovementVector.x <= 0 && movementVector.x >= 0)))
            {
                hasXBeenReversed = true;
            }

            if (((originalMovementVector.y >= 0 && movementVector.y <= 0) || (originalMovementVector.y <= 0 && movementVector.y >= 0)))
            {
                hasYBeenReversed = true;
            }

            if (hasXBeenReversed && hasYBeenReversed)
            {
                movementVector.x = 0;
                movementVector.y = 0;
                break;
            }
        }

        if (Mathf.Abs(originalMovementVector.y) < Mathf.Abs(vectorsToSubtract.y) && Mathf.Abs(originalMovementVector.x) < Mathf.Abs(vectorsToSubtract.x))
        {
            movementVector = Vector2.zero;
        }
        else
        {
            movementVector -= vectorsToSubtract;
        }
    }

    public float GetAngle()
    {
        return ang;
    }

    public void SetAngle(float a)
    {
        ang = a;
    }

    public Vector2 GetConePosition()
    {
        return new Vector2(ang, transform.position.y);
    }

    public Vector2 GetConeDirectionVector(float range, float angleOffset)
    {
        Vector2 ret = MathHelper.RotateVector2(
            new Vector2(0.0f, 1.0f),
            Mathf.Deg2Rad * (currentRotationAngle + angleOffset)
        );
        const float speedCoeff = 0.6f;
        ret.Set(
            -ret.x * 0.02f * speedCoeff * 0.015f * range,
            ret.y * 100.0f * speedCoeff * 0.015f * range
        );

        return ret;
    }

    public void SpeedUp()
    {
        speed = startSpeed * highSpeedMod;
    }

    public void ResetSpeed()
    {
        if (speed != startSpeed)
        {
            speed = startSpeed;
        }
    }

    public float GetDefaultSpeed()
    {
        return startSpeed;
    }

    public void AddCollisionLayers(int[] layers)
    {
        foreach (int l in layers)
        {
            collisionLayer |= (1 << l);
        }
    }
}
