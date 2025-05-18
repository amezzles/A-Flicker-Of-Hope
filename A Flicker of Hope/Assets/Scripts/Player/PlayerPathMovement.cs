using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPathMovement : MonoBehaviour
{
    [SerializeField] private PathManager pathManager;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float currentDistanceAlongPath = 0f;

    private float currentMoveInput = 0f;

    void Start()
    {
        if (pathManager == null || pathManager.GetNodeCount() == 0)
        {
            enabled = false;
            return;
        }

        Vector3 initialPosition = pathManager.GetPointAtDistance(0);
        if (float.IsNaN(initialPosition.x))
        {
            enabled = false;
            return;
        }
        transform.position = initialPosition;

        if (pathManager.GetNodeCount() >= 1)
        {
            Vector3 initialDirection = pathManager.GetDirectionAtDistance(0);
            if (!float.IsNaN(initialDirection.x) && initialDirection != Vector3.zero)
            {
                transform.forward = initialDirection;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        currentMoveInput = inputVector.y;
    }

    void Update()
    {
        currentDistanceAlongPath += currentMoveInput * moveSpeed * Time.deltaTime;

        float totalPathLength = pathManager.GetTotalPathLength();
        if (float.IsNaN(totalPathLength) || totalPathLength < 0)
        {
            return;
        }
        currentDistanceAlongPath = Mathf.Clamp(currentDistanceAlongPath, 0f, totalPathLength);

        Vector3 targetPosition = pathManager.GetPointAtDistance(currentDistanceAlongPath);
        if (float.IsNaN(targetPosition.x))
        {
            return;
        }
        transform.position = targetPosition;

        if (pathManager.GetNodeCount() >= 1)
        {
            Vector3 pathDirection = pathManager.GetDirectionAtDistance(currentDistanceAlongPath);
            if (!float.IsNaN(pathDirection.x) && pathDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(pathDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void SnapToNode(int nodeIndex)
    {
        if (pathManager == null) { return; }

        int nodeCount = pathManager.GetNodeCount();
        if (nodeIndex < 0 || nodeIndex >= nodeCount) { return; }

        float distanceAtNode = pathManager.GetDistanceAtNode(nodeIndex);
        if (float.IsNaN(distanceAtNode)) { return; }
        currentDistanceAlongPath = distanceAtNode;

        Vector3 snapPosition = pathManager.GetPointAtDistance(currentDistanceAlongPath);
        if (float.IsNaN(snapPosition.x)) { return; }
        transform.position = snapPosition;

        if (nodeCount >= 1)
        {
            Vector3 snapDirection = pathManager.GetDirectionAtDistance(currentDistanceAlongPath);
            if (!float.IsNaN(snapDirection.x) && snapDirection != Vector3.zero)
            {
                transform.forward = snapDirection;
            }
        }
    }
}
