using UnityEngine;
using System.Collections;
using System.Linq;

public class EndSequenceTrigger : MonoBehaviour
{
    [SerializeField] private Transform[] endAnimalLocations;
    [SerializeField] private EndSequenceCinematic cinematicSequence;
    [SerializeField] private GameObject HUD;
    private PlayerInteract playerInteract;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerInteract>().CanTriggerEndSequence)
        {
            playerInteract = other.GetComponent<PlayerInteract>();

            StartCoroutine(EndSequenceCoroutine());
        }
    }

    private IEnumerator EndSequenceCoroutine()
    {
        //Teleport all five animals in the scene to each transform in the end animal locations
        playerInteract.InputEnabled(false);
        AnimalMovement[] allAnimals = FindObjectsOfType<AnimalMovement>();
        var animalsToMove = allAnimals.ToList();

        for (int i = 0; i < animalsToMove.Count; i++)
        {
            if (i < endAnimalLocations.Length && endAnimalLocations[i] != null)
            {
                AnimalMovement animal = animalsToMove[i];
                if (animal != null)
                {
                    animal.StopFollowing();
                    animal.enabled = false;

                    animal.transform.position = endAnimalLocations[i].position;
                    animal.transform.rotation = endAnimalLocations[i].rotation;
                }
            }
        }

        if (HUD != null)
        {
            HUD.SetActive(false);
        }

        if (cinematicSequence != null)
        {
            Debug.Log("Cinematic sequence found. Starting cinematic...");
            cinematicSequence.StartCinematicSequence();
        }
        else
        {
            Debug.LogWarning("Cinematic sequence is null. Did you forget to assign it in the Inspector?");
        }

        yield return null;
    }
}
