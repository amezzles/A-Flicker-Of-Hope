using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

public class PlayerInteract : MonoBehaviour
{
    private GameObject _currentTargetObject;
    private PlayerPathMovement _playerPathMovement;
    private Camera _mainCamera;
    private Coroutine _cameraMoveCoroutine;
    private bool _interactionEnabled = true;

    [SerializeField] private float cameraMoveDuration = 1.0f;

    public event Action<GameObject> OnCurrentTargetChanged;
    public event Action<GameObject> OnInteractWithTarget;

    public GameObject CurrentTargetObject
    {
        get => _currentTargetObject;
        set
        {
            if (_currentTargetObject != value)
            {
                _currentTargetObject = value;
                OnCurrentTargetChanged?.Invoke(_currentTargetObject);
            }
        }
    }

    private void Awake()
    {
        _playerPathMovement = GetComponent<PlayerPathMovement>();
        _mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            CurrentTargetObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable") && other.gameObject == _currentTargetObject)
        {
            CurrentTargetObject = null;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && _currentTargetObject != null && _interactionEnabled)
        {
            InteractWithTarget(_currentTargetObject);
        }
    }

    private void InteractWithTarget(GameObject target)
    {
        OnInteractWithTarget?.Invoke(target);
        InputEnabled(false);

        Interactable targetInteractable = target.GetComponent<Interactable>();
        if (targetInteractable == null || _mainCamera == null) return;

        if (_playerPathMovement != null)
        {
            _playerPathMovement.SnapToNode(targetInteractable.interactionNodeIndex);
        }

        Vector3 directionToTarget = target.transform.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = lookRotation;
        }

        if (_cameraMoveCoroutine != null)
        {
            StopCoroutine(_cameraMoveCoroutine);
        }
        _cameraMoveCoroutine = StartCoroutine(MoveCameraToTarget(targetInteractable));
    }

    private void EndInteraction()
    {
        
        
    }

    private IEnumerator MoveCameraToTarget(Interactable interactable)
    {
        if (interactable.interactionCameraTransform == null) yield break;

        Transform targetCamTransform = interactable.interactionCameraTransform;
        Vector3 startPosition = _mainCamera.transform.position;
        Quaternion startRotation = _mainCamera.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < cameraMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / cameraMoveDuration);
            _mainCamera.transform.position = Vector3.Lerp(startPosition, targetCamTransform.position, t);
            _mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetCamTransform.rotation, t);
            yield return null;
        }

        _mainCamera.transform.position = targetCamTransform.position;
        _mainCamera.transform.rotation = targetCamTransform.rotation;

        _cameraMoveCoroutine = null;
    }

    private void InputEnabled(bool enabled)
    {
        _playerPathMovement._movementEnabled = enabled;
        _interactionEnabled = enabled;
    }
}