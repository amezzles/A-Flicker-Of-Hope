using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    public Transform interactionCameraTransform;
    public Transform lookAtTarget;
    public int interactionNodeIndex;
    public string dialogue;
    public bool isHealed = false;

    [SerializeField] private GameObject corruptionParticles;
    private AnimalMovement animalMovement;
    private bool followStarted = false;
    private float currentSpeed;
    private Animator animator;

    void Awake()
    {
        animalMovement = GetComponent<AnimalMovement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        currentSpeed = animalMovement != null ? animalMovement.CurrentSpeed : 0f;
        animator.SetFloat("Speed", currentSpeed);
    }

    public void OnInteractionComplete(PlayerPathMovement player)
    {
        if (isHealed && !followStarted && animalMovement != null && player != null)
        {
            Debug.Log($"Interaction complete for {gameObject.name}, animal will start following.");
            animator.SetTrigger("Healed");
            corruptionParticles.SetActive(false);
            animalMovement.StartFollowing(player);
            followStarted = true;
        }
    }
}