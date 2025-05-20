using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _promptTextObject;
    [SerializeField] private GameObject _promptPanel;
    [SerializeField] private Image _buttonPromptImage;

    [SerializeField] private Vector2 _screenOffset = new Vector2(0, 50);
    [SerializeField] private Vector2 _dialogueButtonOffset = new Vector2(0, 200);
    private RectTransform _panelRectTransform;
    private RectTransform _buttonPromptRectTransform;
    private Vector2 _originalButtonPromptAnchoredPosition;

    private GameObject _currentTargetObject;
    private PlayerInteract _playerInteract;
    private bool _interactionEnabled = true;
    private bool _isShowingDialogue = false;

    public Canvas interactionCanvas;
    public Camera playerCamera;

    void Awake()
    {
        _playerInteract = GetComponent<PlayerInteract>();
        if (_playerInteract == null)
        {
            enabled = false;
            return;
        }
        if (_promptPanel != null)
        {
            _panelRectTransform = _promptPanel.GetComponent<RectTransform>();
        }
        else
        {
            enabled = false;
            return;
        }

        if (_buttonPromptImage != null)
        {
            _buttonPromptRectTransform = _buttonPromptImage.GetComponent<RectTransform>();
            if (_buttonPromptRectTransform != null)
            {
                _originalButtonPromptAnchoredPosition = _buttonPromptRectTransform.anchoredPosition;
            }
        }
        HideAllPrompts();
    }

    void OnEnable()
    {
        if (_playerInteract != null)
        {
            _playerInteract.OnCurrentTargetChanged += HandleTargetChanged;
            _playerInteract.OnInteractWithTarget += HandleSuccessfulInteraction;
            _playerInteract.OnInteractionEnded += HandleInteractionEnded;
        }
    }

    void OnDisable()
    {
        if (_playerInteract != null)
        {
            _playerInteract.OnCurrentTargetChanged -= HandleTargetChanged;
            _playerInteract.OnInteractWithTarget -= HandleSuccessfulInteraction;
            _playerInteract.OnInteractionEnded -= HandleInteractionEnded;
        }
    }

    private void HandleTargetChanged(GameObject newTarget)
    {
        _currentTargetObject = newTarget;
        if (!_isShowingDialogue)
        {
            UpdatePromptsVisibilityAndPosition();
        }
    }

    private void HandleSuccessfulInteraction(GameObject interactedObject)
    {
        _interactionEnabled = false;
        if (_buttonPromptImage != null)
        {
            _buttonPromptImage.enabled = false;
        }
    }

    private void HandleInteractionEnded()
    {
        _isShowingDialogue = false;
        _interactionEnabled = true;

        if (_buttonPromptRectTransform != null)
        {
            _buttonPromptRectTransform.anchoredPosition = _originalButtonPromptAnchoredPosition;
        }
        UpdatePromptsVisibilityAndPosition();
    }

    void Update()
    {
        if (_currentTargetObject == null)
        {
            if (_isShowingDialogue && _promptPanel != null && _promptPanel.activeSelf)
            {
                _isShowingDialogue = false;
                HideAllPrompts();
            }
            return;
        }

        if (_promptPanel != null)
        {
            bool shouldPanelBeActive = _isShowingDialogue || (_interactionEnabled && _promptPanel.activeSelf);
            if (shouldPanelBeActive)
            {
                if (!_promptPanel.activeSelf) _promptPanel.SetActive(true);
                PositionPromptPanel();
            }
        }
    }

    private void PositionPromptPanel()
    {
        if (playerCamera == null || _currentTargetObject == null || _panelRectTransform == null || _promptPanel == null) return;
        if (!_promptPanel.activeSelf) return;

        Transform positionReference = _currentTargetObject.transform;
        if (_isShowingDialogue) {
            Interactable interactable = _currentTargetObject.GetComponent<Interactable>();
            if (interactable != null && interactable.lookAtTarget != null) {
                positionReference = interactable.lookAtTarget;
            }
        }

        Vector3 worldPos = positionReference.position;
        var screenPos = playerCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z <= 0) {
            _promptPanel.SetActive(false);
            return;
        }

        if (interactionCanvas == null) return;
        var canvasRect = interactionCanvas.transform as RectTransform;
        if (canvasRect == null) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            interactionCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : interactionCanvas.worldCamera,
            out localPos);

        _panelRectTransform.anchoredPosition = localPos + _screenOffset;
    }

    private void UpdatePromptsVisibilityAndPosition()
    {
        if (_isShowingDialogue) return;

        if (_currentTargetObject != null && _interactionEnabled)
        {
            if (_promptPanel != null) _promptPanel.SetActive(true);

            if (_buttonPromptImage != null)
            {
                _buttonPromptImage.enabled = true;
                _buttonPromptImage.gameObject.SetActive(true);
                if (_buttonPromptRectTransform != null)
                {
                    _buttonPromptRectTransform.anchoredPosition = _originalButtonPromptAnchoredPosition;
                }
            }
            if (_promptTextObject != null)
            {
                _promptTextObject.gameObject.SetActive(false);
            }
            PositionPromptPanel();
        }
        else
        {
            HideAllPrompts();
        }
    }

    private void HideAllPrompts()
    {
        if (_promptPanel != null) _promptPanel.SetActive(false);
        if (_buttonPromptRectTransform != null)
        {
            _buttonPromptRectTransform.anchoredPosition = _originalButtonPromptAnchoredPosition;
        }
        if (_buttonPromptImage != null)
        {
            _buttonPromptImage.enabled = false;
        }
        if (_promptTextObject != null && (_promptPanel == null || !_promptTextObject.transform.IsChildOf(_promptPanel.transform)))
        {
            _promptTextObject.gameObject.SetActive(false);
        }
    }

    public void ShowDialogue(string dialogue)
    {
        _isShowingDialogue = true;
        _interactionEnabled = false;

        if (_promptPanel != null)
        {
            _promptPanel.SetActive(true);
        }
        else
        {
            _isShowingDialogue = false;
            return;
        }

        if (_buttonPromptImage != null)
        {
            _buttonPromptImage.enabled = false;
            _buttonPromptImage.gameObject.SetActive(true);
            if (_buttonPromptRectTransform != null)
            {
                _buttonPromptRectTransform.anchoredPosition = _dialogueButtonOffset;
            }
        }

        if (_promptTextObject != null)
        {
            _promptTextObject.text = dialogue;
            _promptTextObject.gameObject.SetActive(true);
        }
        PositionPromptPanel();
    }
}