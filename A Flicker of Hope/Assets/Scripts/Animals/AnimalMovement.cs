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
    [SerializeField] private float initialStillDuration = 1.0f;
    [SerializeField] private float initialApproachMoveDuration = 1.5f;

    private float currentTargetDistanceOnPath;
    private bool isFollowing = false;
    private Coroutine initialApproachCoroutine;
    private Vector3 animalLastLookDirection;

    private float currentSpeed;
    public float CurrentSpeed => currentSpeed;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

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
        animalLastLookDirection = transform.forward;
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

        Quaternion finalTargetRotation = transform.rotation;
        Vector3 determinedLookDirForApproach = transform.forward;

        Vector3 pathDirAtAnimalLandingSpot = pathManager.GetDirectionAtDistance(targetFollowDistanceOnPath);
        if (!float.IsNaN(pathDirAtAnimalLandingSpot.x) && pathDirAtAnimalLandingSpot != Vector3.zero)
        {
            if (playerToFollow.CurrentMoveInput < -0.01f)
            {
                determinedLookDirForApproach = -pathDirAtAnimalLandingSpot;
            }
            else
            {
                determinedLookDirForApproach = pathDirAtAnimalLandingSpot;
            }
            finalTargetRotation = Quaternion.LookRotation(determinedLookDirForApproach);
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

        if (determinedLookDirForApproach != Vector3.zero)
        {
            this.animalLastLookDirection = determinedLookDirForApproach;
        }


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

        Vector3 currentPathDirForAnimal = pathManager.GetDirectionAtDistance(currentTargetDistanceOnPath);

        if (!float.IsNaN(currentPathDirForAnimal.x) && currentPathDirForAnimal != Vector3.zero)
        {
            if (playerToFollow.CurrentMoveInput < -0.01f)
            {
                animalLastLookDirection = -currentPathDirForAnimal;
            }
            else if (playerToFollow.CurrentMoveInput > 0.01f)
            {
                animalLastLookDirection = currentPathDirForAnimal;
            }
        }

        if (animalLastLookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(animalLastLookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }

    public void StopFollowing()
    {
        isFollowing = false;
        animator.SetBool("Ending", true);
        Debug.Log("Stop following");
        if (initialApproachCoroutine != null)
        {
            StopCoroutine(initialApproachCoroutine);
            initialApproachCoroutine = null;
        }
    }
}