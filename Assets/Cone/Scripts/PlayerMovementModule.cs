using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementModule : MovementModule
{
    private ConeRunner player;

    private readonly float maxDirectionChangeSpeed = 0.2f;
    [Range(0, 0.2f)]
    [SerializeField] private float directionChangeSpeed = 0.06f;
    [SerializeField] private float verticalMovementModifier = 1f;

    private readonly float maxRotationSpeed = 32f;
    [Range(0, 32f)]
    [SerializeField] private float rotationInertia = 16;
    [SerializeField] private float minVerticalPosition;
    [SerializeField] private float maxVerticalPosition;

    private float lastHorizontal;
    private float lastVertical;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        player = GetComponent<ConeRunner>();
    }
    public override void HandleMovement()
    {
        Vector2 movementVector = Vector2.zero;
        MovementInputResponse response = player.MovementInput();
        float horizontalMovement = response.horizontal;
        float verticalMovement = -response.vertical;

        if (horizontalMovement == 0f && verticalMovement == 0f)
        {
            return;
        }

        float scaledDirectionChangeSpeed = directionChangeSpeed * (1 / 60) / Time.deltaTime;

        horizontalMovement = Mathf.Lerp(lastHorizontal, horizontalMovement, Mathf.Min((maxDirectionChangeSpeed - directionChangeSpeed) / Mathf.Abs(lastHorizontal - horizontalMovement), 1));
        verticalMovement = Mathf.Lerp(lastVertical, verticalMovement, Mathf.Min((maxDirectionChangeSpeed - directionChangeSpeed) / Mathf.Abs(lastVertical - verticalMovement), 1));

        lastHorizontal = horizontalMovement;
        lastVertical = verticalMovement;

        movementVector = new Vector2(horizontalMovement, verticalMovement).normalized;

        float newAngle = player.GetAngle() + player.speed * 0.2f * Time.deltaTime * -movementVector.x;
        Vector3 newPosition = new Vector3(
            transform.position.x,
            transform.position.y + player.speed * verticalMovementModifier * Time.deltaTime * movementVector.y,
            //Mathf.Clamp(transform.position.y + player.speed * verticalMovementModifier * Time.deltaTime * movementVector.y, minHeight, maxHeight), 
            transform.position.z
        );
        CalculatePositionResponse cpr = ConePositions.Instance.CalculatePosition(newPosition.y, newAngle);
        player.HandleCollisions(ref movementVector, cpr);
        newAngle = player.GetAngle() + player.speed * 0.2f * Time.deltaTime * -movementVector.x;
        //newPosition = new Vector3(transform.position.x, transform.position.y + player.speed * verticalMovementModifier * Time.deltaTime * movementVector.y, transform.position.z);
        newPosition = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + player.speed * verticalMovementModifier * Time.deltaTime * movementVector.y, minVerticalPosition, maxVerticalPosition), transform.position.z);
        transform.position = newPosition;
        player.SetAngle(newAngle);
        float angle = Vector3.SignedAngle(new Vector3(verticalMovement, 0, horizontalMovement), new Vector3(1, 0, 0), transform.up);

        if (horizontalMovement != 0 || verticalMovement != 0)
        {
            float scaledRotationInertia = (rotationInertia / Time.deltaTime) / 60;
            player.SetCurrentRotation(
                Mathf.LerpAngle(
                    player.GetCurrentRotation(),
                    angle,
                    Mathf.Min((maxRotationSpeed - scaledRotationInertia) / Mathf.Abs(Mathf.DeltaAngle(player.GetCurrentRotation(), angle)), 1)
                )
            );
            player.SetCurrentRotation(player.GetCurrentRotation() % 360);
        }
    }
}
