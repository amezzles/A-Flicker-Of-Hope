using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] public Transform interactionCameraTransform;
    [SerializeField] public Transform lookAtTarget;
    [SerializeField] public int interactionNodeIndex;
}