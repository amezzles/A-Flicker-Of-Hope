using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPathMovement : MonoBehaviour
{
    [SerializeField] private PathManager pathManager;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float currentDistanceAlongPath = 0f;
    [SerializeField] private float snapToNodeDuration = 0.5f;

    private float currentMoveInput = 0f;
    private Vector3 lastLookDirection;
    private Coroutine _snapToNodeCoroutine;

    public bool _movementEnabled = true;
    private bool _orientToPath = true;

    public bool IsSnapping => _snapToNodeCoroutine != null;


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
                lastLookDirection = initialDirection;
            }
            else
            {
                lastLookDirection = transform.forward;
            }
        }
        else
        {
            lastLookDirection = transform.forward;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!_movementEnabled) { return; }
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

        if (pathManager.GetNodeCount() >= 1 && _orientToPath)
        {
            Vector3 pathSegmentDirection = pathManager.GetDirectionAtDistance(currentDistanceAlongPath);
            if (!float.IsNaN(pathSegmentDirection.x) && pathSegmentDirection != Vector3.zero)
            {
                if (currentMoveInput < -0.01f)
                {
                    lastLookDirection = -pathSegmentDirection;
                }
                else if (currentMoveInput > 0.01f)
                {
                    lastLookDirection = pathSegmentDirection;
                }

                if (lastLookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lastLookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    private IEnumerator SnapToNodeCoroutine(int nodeIndex)
    {
        _orientToPath = false;

        int nodeCount = pathManager.GetNodeCount();
        float distanceAtNode = pathManager.GetDistanceAtNode(nodeIndex);

        if (float.IsNaN(distanceAtNode))
        {
            Debug.LogError($"Could not get valid distance for nodeIndex: {nodeIndex}", this);
            _orientToPath = true;
            yield break;
        }

        Vector3 targetPosition = pathManager.GetPointAtDistance(distanceAtNode);
        if (float.IsNaN(targetPosition.x))
        {
            Debug.LogError($"Could not get valid point at distance for nodeIndex: {nodeIndex}", this);
            _orientToPath = true;
            yield break;
        }

        Quaternion targetRotation = transform.rotation;
        Vector3 snapDirection = Vector3.zero;

        if (nodeCount > 0)
        {
            snapDirection = pathManager.GetDirectionAtDistance(distanceAtNode);
            if (!float.IsNaN(snapDirection.x) && snapDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(snapDirection);
            }
        }

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < snapToNodeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / snapToNodeDuration);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        currentDistanceAlongPath = distanceAtNode;
        if (snapDirection != Vector3.zero)
        {
            lastLookDirection = snapDirection;
        }
        _snapToNodeCoroutine = null;
    }

    public void SnapToNode(int nodeIndex)
    {
        if (_snapToNodeCoroutine != null)
        {
            StopCoroutine(_snapToNodeCoroutine);
        }
        _snapToNodeCoroutine = StartCoroutine(SnapToNodeCoroutine(nodeIndex));
    }

    public void EnableOrientation()
    {
        _orientToPath = true;
    }
}