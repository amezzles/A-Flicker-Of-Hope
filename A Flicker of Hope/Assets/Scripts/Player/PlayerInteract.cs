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
    private Coroutine _interactionSequenceCoroutine;
    private bool _interactionEnabled = true;
    private bool _isInteracting = false;

    [SerializeField] private float cameraMoveDuration = 1.0f;
    [SerializeField] private float playerLookAtTargetDuration = 0.25f;

    public event Action<GameObject> OnCurrentTargetChanged;
    public event Action<GameObject> OnInteractWithTarget;
    public event Action OnInteractionEnded;

    private Transform _originalCameraParent;
    private Vector3 _originalCameraLocalPosition;
    private Quaternion _originalCameraLocalRotation;


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

    public bool IsInteracting => _isInteracting;

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
        if (context.performed && _currentTargetObject != null && _interactionEnabled && !_isInteracting)
        {
            if (_interactionSequenceCoroutine != null)
            {
                StopCoroutine(_interactionSequenceCoroutine);
                if (_cameraMoveCoroutine != null) StopCoroutine(_cameraMoveCoroutine);
                RestoreCameraParenting();
                InputEnabled(true);
                if (_playerPathMovement != null) _playerPathMovement.EnableOrientation();
            }
            _interactionSequenceCoroutine = StartCoroutine(InteractionSequenceCoroutine(_currentTargetObject));
        }
    }

    private IEnumerator InteractionSequenceCoroutine(GameObject target)
    {
        _isInteracting = true;
        OnInteractWithTarget?.Invoke(target);
        InputEnabled(false);

        // Detach camera
        if (_mainCamera != null)
        {
            _originalCameraParent = _mainCamera.transform.parent;
            _originalCameraLocalPosition = _mainCamera.transform.localPosition;
            _originalCameraLocalRotation = _mainCamera.transform.localRotation;
            _mainCamera.transform.SetParent(null, true); // true to keep world position
        }

        Interactable targetInteractable = target.GetComponent<Interactable>();
        if (targetInteractable == null || _mainCamera == null)
        {
            EndInteraction();
            _interactionSequenceCoroutine = null;
            yield break;
        }

        if (_playerPathMovement != null)
        {
            _playerPathMovement.SnapToNode(targetInteractable.interactionNodeIndex);
            while (_playerPathMovement.IsSnapping)
            {
                yield return null;
            }
        }

        Vector3 directionToTarget = target.transform.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion initialPlayerRotation = transform.rotation;
            Quaternion targetPlayerLookRotation = Quaternion.LookRotation(directionToTarget);

            if (playerLookAtTargetDuration > 0f)
            {
                float lookAtElapsedTime = 0f;
                while (lookAtElapsedTime < playerLookAtTargetDuration)
                {
                    lookAtElapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(lookAtElapsedTime / playerLookAtTargetDuration);
                    transform.rotation = Quaternion.Slerp(initialPlayerRotation, targetPlayerLookRotation, t);
                    yield return null;
                }
            }
            transform.rotation = targetPlayerLookRotation;
        }

        if (_cameraMoveCoroutine != null)
        {
            StopCoroutine(_cameraMoveCoroutine);
        }
        _cameraMoveCoroutine = StartCoroutine(MoveCameraToTarget(targetInteractable));
        if (_cameraMoveCoroutine != null)
        {
            yield return _cameraMoveCoroutine;
        }

        _interactionSequenceCoroutine = null;
    }

    public void TriggerEndInteraction()
    {
        if (_isInteracting)
        {
            if (_interactionSequenceCoroutine != null)
            {
                StopCoroutine(_interactionSequenceCoroutine);
                _interactionSequenceCoroutine = null;
            }
            if (_cameraMoveCoroutine != null)
            {
                StopCoroutine(_cameraMoveCoroutine);
                _cameraMoveCoroutine = null;
            }
            EndInteraction();
        }
    }

    private void EndInteraction()
    {
        RestoreCameraParenting();
        InputEnabled(true);
        if (_playerPathMovement != null)
        {
            _playerPathMovement.EnableOrientation();
        }
        _isInteracting = false;
        OnInteractionEnded?.Invoke();
    }

    private void RestoreCameraParenting()
    {
        if (_mainCamera != null)
        {
            _mainCamera.transform.SetParent(_originalCameraParent, false);
            _mainCamera.transform.localPosition = _originalCameraLocalPosition;
            _mainCamera.transform.localRotation = _originalCameraLocalRotation;
        }
    }

    private IEnumerator MoveCameraToTarget(Interactable interactable)
    {
        if (interactable.interactionCameraTransform == null)
        {
            _cameraMoveCoroutine = null;
            yield break;
        }

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
        if (_playerPathMovement != null) _playerPathMovement._movementEnabled = enabled;
        _interactionEnabled = enabled;
    }
}