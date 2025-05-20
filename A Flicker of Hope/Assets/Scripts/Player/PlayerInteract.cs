using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

public class PlayerInteract : MonoBehaviour
{
    private GameObject _currentTargetObject;
    private PlayerPathMovement _playerPathMovement;
    private PlayerInteractionUI _playerInteractionUI;
    private Camera _mainCamera;
    private Coroutine _cameraMoveCoroutine;
    private Coroutine _interactionSequenceCoroutine;
    private bool _interactionEnabled = true;
    private bool _isInteracting = false;
    private bool _nextInteractionIsHealing = false;

    [SerializeField] private float _cameraMoveDuration = 1.0f;
    [SerializeField] private float _playerLookAtTargetDuration = 0.25f;
    [SerializeField] private GameObject _healingParticles;
    [SerializeField] private float _postHealInputDisableDuration = 1.0f;

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
        _playerInteractionUI = GetComponent<PlayerInteractionUI>();
        _mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && !interactable.isHealed)
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
        if (context.performed && _interactionEnabled)
        {
            if (_currentTargetObject != null)
            {
                Interactable targetInteractable = _currentTargetObject.GetComponent<Interactable>();
                if (targetInteractable == null) return;

                if (_nextInteractionIsHealing && !targetInteractable.isHealed)
                {
                    if (_interactionSequenceCoroutine != null)
                    {
                        StopCoroutine(_interactionSequenceCoroutine);
                        _interactionSequenceCoroutine = null;
                    }
                    _interactionSequenceCoroutine = StartCoroutine(HealAndEndSequence(targetInteractable));
                    _nextInteractionIsHealing = false;
                }
                else if (!_isInteracting && !targetInteractable.isHealed)
                {
                    if (_interactionSequenceCoroutine != null)
                    {
                        StopCoroutine(_interactionSequenceCoroutine);
                    }
                    _interactionSequenceCoroutine = StartCoroutine(InteractionSequenceCoroutine(targetInteractable));
                }
            }
            else if (_isInteracting)
            {
                TriggerEndInteraction();
            }
        }
    }

    private IEnumerator InteractionSequenceCoroutine(Interactable targetInteractable)
    {
        GameObject target = targetInteractable.gameObject;
        _isInteracting = true;
        _nextInteractionIsHealing = false;
        OnInteractWithTarget?.Invoke(target);
        InputEnabled(false);

        if (_mainCamera != null)
        {
            _originalCameraParent = _mainCamera.transform.parent;
            _originalCameraLocalPosition = _mainCamera.transform.localPosition;
            _originalCameraLocalRotation = _mainCamera.transform.localRotation;
            _mainCamera.transform.SetParent(null, true);
        }

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

        // Moving player and camera

        Vector3 directionToTarget = target.transform.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion initialPlayerRotation = transform.rotation;
            Quaternion targetPlayerLookRotation = Quaternion.LookRotation(directionToTarget);

            if (_playerLookAtTargetDuration > 0f)
            {
                float lookAtElapsedTime = 0f;
                while (lookAtElapsedTime < _playerLookAtTargetDuration)
                {
                    lookAtElapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(lookAtElapsedTime / _playerLookAtTargetDuration);
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

        // Dialogue Scrolling

        if (_playerInteractionUI != null)
        {
            _playerInteractionUI.ShowDialogue(targetInteractable.dialogue);
            while (_playerInteractionUI.IsTextCurrentlyScrolling())
            {
                yield return null;
            }
            _nextInteractionIsHealing = true;
        }
        _interactionSequenceCoroutine = null;
    }

    private IEnumerator HealAndEndSequence(Interactable targetToHeal)
    {
        _isInteracting = true;
        if (_healingParticles != null)
        {
            Instantiate(_healingParticles, targetToHeal.transform.position, Quaternion.identity);
        }
        targetToHeal.isHealed = true;

        yield return new WaitForSeconds(_postHealInputDisableDuration);

        TriggerEndInteraction();
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
        if (_mainCamera != null && _originalCameraParent != null)
        {
            _mainCamera.transform.SetParent(_originalCameraParent, false);
            _mainCamera.transform.localPosition = _originalCameraLocalPosition;
            _mainCamera.transform.localRotation = _originalCameraLocalRotation;
        }
        else if (_mainCamera != null && _originalCameraParent == null)
        {
             _mainCamera.transform.SetParent(null, false);
             _mainCamera.transform.localPosition = _originalCameraLocalPosition;
             _mainCamera.transform.localRotation = _originalCameraLocalRotation;
        }
    }

    private IEnumerator MoveCameraToTarget(Interactable interactable)
    {
        if (interactable.interactionCameraTransform == null || _mainCamera == null)
        {
            _cameraMoveCoroutine = null;
            yield break;
        }

        Transform targetCamTransform = interactable.interactionCameraTransform;
        Vector3 startPosition = _mainCamera.transform.position;
        Quaternion startRotation = _mainCamera.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < _cameraMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _cameraMoveDuration);
            _mainCamera.transform.position = Vector3.Lerp(startPosition, targetCamTransform.position, t);
            _mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetCamTransform.rotation, t);
            yield return null;
        }

        _mainCamera.transform.position = targetCamTransform.position;
        _mainCamera.transform.rotation = targetCamTransform.rotation;
        _cameraMoveCoroutine = null;
    }

    private IEnumerator HealInteractionTarget(Interactable interactable)
    {
        if (interactable == null) yield break;
        
    }

    private void InputEnabled(bool enabled)
    {
        if (_playerPathMovement != null) _playerPathMovement._movementEnabled = enabled;
        _interactionEnabled = enabled;
    }
}