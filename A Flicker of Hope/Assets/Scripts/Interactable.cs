using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    public Transform interactionCameraTransform;
    public Transform lookAtTarget;
    public int interactionNodeIndex;
    public string dialogue;
    public bool isHealed = false;
}