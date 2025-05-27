using UnityEngine;
using System.Collections;

public class AnimalMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPathMovement playerToFollow;
    private PathManager pathManager;

    [Header("Following Settings")]
    [SerializeField] private float followDistanceOffset = 2.0f;
    [SerializeField] private float positionLerpSpeed = 5.0f;
    [SerializeField] private float rotationLerpSpeed = 5.0f;

    [Header("Initial Approach Settings")]
    [SerializeField] private float initialStillDuration = 0.5f;
    [SerializeField] private float initialApproachMoveDuration = 1.5f;

    private float currentTargetDistanceOnPath;
    private bool isFollowing = false;
    private Coroutine initialApproachCoroutine;

    private float currentSpeed;
    public float CurrentSpeed => currentSpeed;

    public void StartFollowing(PlayerPathMovement player)
    {
        if (player == null)
        {
            Debug.LogError("AnimalMovement: Player to follow is null.", this);
            enabled = false;
            return;
        }

        playerToFollow = player;
        pathManager = playerToFollow.PathManagerInstance;

        if (pathManager == null || pathManager.GetNodeCount() == 0)
        {
            Debug.LogError("AnimalMovement: PathManager not available or path is empty.", this);
            enabled = false;
            return;
        }

        isFollowing = false;
        if (initialApproachCoroutine != null)
        {
            StopCoroutine(initialApproachCoroutine);
        }
        initialApproachCoroutine = StartCoroutine(PerformInitialApproach());
    }

    private IEnumerator PerformInitialApproach()
    {
        yield return new WaitForSeconds(initialStillDuration);

        float playerCurrentDistanceValue = playerToFollow.CurrentDistanceAlongPath;
        float targetFollowDistanceOnPath = Mathf.Max(0, playerCurrentDistanceValue - followDistanceOffset);

        Vector3 finalTargetPosition = pathManager.GetPointAtDistance(targetFollowDistanceOnPath);
        if (float.IsNaN(finalTargetPosition.x))
        {
            Debug.LogError("AnimalMovement: Initial position for animal on path is NaN. Disabling.", this);
            enabled = false;
            initialApproachCoroutine = null;
            yield break;
        }

        Quaternion finalTargetRotation;
        Vector3 playerLookDirectionValue = playerToFollow.LastLookDirection;
        if (playerLookDirectionValue != Vector3.zero)
        {
            finalTargetRotation = Quaternion.LookRotation(playerLookDirectionValue);
        }
        else
        {
            Vector3 pathDir = pathManager.GetDirectionAtDistance(targetFollowDistanceOnPath);
            if (!float.IsNaN(pathDir.x) && pathDir != Vector3.zero)
            {
                finalTargetRotation = Quaternion.LookRotation(pathDir);
            }
            else
            {
                finalTargetRotation = transform.rotation;
            }
        }

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < initialApproachMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / initialApproachMoveDuration);

            transform.position = Vector3.Lerp(startPosition, finalTargetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, finalTargetRotation, t);

            yield return null;
        }

        transform.position = finalTargetPosition;
        transform.rotation = finalTargetRotation;

        this.currentTargetDistanceOnPath = targetFollowDistanceOnPath;

        isFollowing = true;
        initialApproachCoroutine = null;
        Debug.Log($"{gameObject.name} started following {playerToFollow.name}.");
    }

    void Update()
    {
        if (!isFollowing || playerToFollow == null || pathManager == null) { return; }

        currentSpeed = playerToFollow.CurrentSpeed;
        float playerCurrentDistance = playerToFollow.CurrentDistanceAlongPath;
        currentTargetDistanceOnPath = playerCurrentDistance - followDistanceOffset;

        float totalPathLength = pathManager.GetTotalPathLength();
        if (float.IsNaN(totalPathLength) || totalPathLength < 0) { return; }
        currentTargetDistanceOnPath = Mathf.Clamp(currentTargetDistanceOnPath, 0f, totalPathLength);

        Vector3 targetPositionOnPath = pathManager.GetPointAtDistance(currentTargetDistanceOnPath);
        if (float.IsNaN(targetPositionOnPath.x)) { return; }

        transform.position = Vector3.Lerp(transform.position, targetPositionOnPath, positionLerpSpeed * Time.deltaTime);

        Vector3 playerLookDirection = playerToFollow.LastLookDirection;

        if (playerLookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(playerLookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }

    public void StopFollowing()
    {
        isFollowing = false;
        if (initialApproachCoroutine != null)
        {
            StopCoroutine(initialApproachCoroutine);
            initialApproachCoroutine = null;
        }
    }
}
