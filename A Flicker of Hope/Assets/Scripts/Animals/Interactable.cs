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

    void Awake()
    {
        animalMovement = GetComponent<AnimalMovement>();
    }

    public void OnInteractionComplete(PlayerPathMovement player)
    {
        if (isHealed && !followStarted && animalMovement != null && player != null)
        {
            Debug.Log($"Interaction complete for {gameObject.name}, animal will start following.");
            corruptionParticles.SetActive(false);
            animalMovement.StartFollowing(player);
            followStarted = true;
        }
    }
}