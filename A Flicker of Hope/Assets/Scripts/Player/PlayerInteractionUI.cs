using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerInteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _promptTextObject;
    [SerializeField] private GameObject _promptPanel;
    [SerializeField] private Image _buttonPromptImage;
    [SerializeField] private TextMeshProUGUI _dialogueDisplayArea;

    [SerializeField] private Vector2 _screenOffset = new Vector2(0, 50);
    [SerializeField] private Vector2 _dialogueButtonOffset = new Vector2(0, 200);
    [SerializeField] private float _textScrollSpeed = 0.15f;

    private RectTransform _panelRectTransform;
    private RectTransform _buttonPromptRectTransform;
    private Vector2 _originalButtonPromptAnchoredPosition;

    private GameObject _currentTargetObject;
    private PlayerInteract _playerInteract;
    private bool _interactionEnabled = true;
    private bool _isShowingDialogue = false;
    private bool _isAwaitingHealAction = false;

    public Canvas interactionCanvas;
    public Camera playerCamera;

    private Coroutine _currentScrollingCoroutine;

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
        if (_currentScrollingCoroutine != null)
        {
            StopCoroutine(_currentScrollingCoroutine);
            _currentScrollingCoroutine = null;
        }
    }

    private void HandleTargetChanged(GameObject newTarget)
    {
        if (_currentTargetObject != newTarget)
        {
            _isAwaitingHealAction = false;
            if (_dialogueDisplayArea != null && _dialogueDisplayArea.gameObject.activeSelf)
            {
                _dialogueDisplayArea.text = "";
                _dialogueDisplayArea.gameObject.SetActive(false);
            }
        }
        _currentTargetObject = newTarget;

        if (!_isShowingDialogue)
        {
            UpdatePromptsVisibilityAndPosition();
        }
    }

    private void HandleSuccessfulInteraction(GameObject interactedObject)
    {
        _interactionEnabled = false;
        if (_promptTextObject != null)
        {
            _promptTextObject.gameObject.SetActive(false);
        }
        if (_buttonPromptImage != null)
        {
            _buttonPromptImage.enabled = false;
        }
    }

    private void HandleInteractionEnded()
    {
        _isShowingDialogue = false;
        _interactionEnabled = true;
        _isAwaitingHealAction = false;

        if (_currentScrollingCoroutine != null)
        {
            StopCoroutine(_currentScrollingCoroutine);
            _currentScrollingCoroutine = null;
        }

        if (_dialogueDisplayArea != null)
        {
            _dialogueDisplayArea.text = "";
            _dialogueDisplayArea.gameObject.SetActive(false);
        }

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
            if (_isShowingDialogue || _isAwaitingHealAction || (_promptPanel != null && _promptPanel.activeSelf))
            {
                _isShowingDialogue = false;
                _isAwaitingHealAction = false;
                if (_currentScrollingCoroutine != null)
                {
                    StopCoroutine(_currentScrollingCoroutine);
                    _currentScrollingCoroutine = null;
                }
                HideAllPrompts();
            }
            return;
        }

        if (_promptPanel != null)
        {
            bool shouldPanelBeActive = _isShowingDialogue || _isAwaitingHealAction || (_interactionEnabled && _currentTargetObject != null);
            if (shouldPanelBeActive)
            {
                if (!_promptPanel.activeSelf) _promptPanel.SetActive(true);
                PositionPromptPanel();
            }
            else if (_promptPanel.activeSelf)
            {
                HideAllPrompts();
            }
        }
    }

    private void PositionPromptPanel()
    {
        if (playerCamera == null || _currentTargetObject == null || _panelRectTransform == null || _promptPanel == null) return;

        Transform positionReference = _currentTargetObject.transform;
        Interactable interactable = _currentTargetObject.GetComponent<Interactable>();
        if (interactable != null && interactable.lookAtTarget != null)
        {
            positionReference = interactable.lookAtTarget;
        }

        Vector3 worldPos = positionReference.position;
        var screenPos = playerCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z <= 0)
        {
            if (_promptPanel.activeSelf) _promptPanel.SetActive(false);
            return;
        }
        if (!_promptPanel.activeSelf && (_isShowingDialogue || _isAwaitingHealAction || (_interactionEnabled && _currentTargetObject != null)))
        {
            _promptPanel.SetActive(true);
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
        if (_isShowingDialogue)
        {
            if (_promptTextObject != null)
            {
                _promptTextObject.gameObject.SetActive(false);
            }
            return;
        }

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
                _promptTextObject.gameObject.SetActive(true);
                if (_isAwaitingHealAction)
                {
                    _promptTextObject.text = "Heal";
                }
                else
                {
                    _promptTextObject.text = "Talk";
                }
            }

            if (_dialogueDisplayArea != null && !_isAwaitingHealAction)
            {
                _dialogueDisplayArea.gameObject.SetActive(false);
                if (_dialogueDisplayArea.text != "") _dialogueDisplayArea.text = "";
            }
            else if (_dialogueDisplayArea != null && _isAwaitingHealAction)
            {
                _dialogueDisplayArea.gameObject.SetActive(true);
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

        if (_promptTextObject != null)
        {
            _promptTextObject.text = "";
            _promptTextObject.gameObject.SetActive(false);
        }
        if (_dialogueDisplayArea != null)
        {
            _dialogueDisplayArea.text = "";
            _dialogueDisplayArea.gameObject.SetActive(false);
        }

        if (_currentScrollingCoroutine != null)
        {
            StopCoroutine(_currentScrollingCoroutine);
            _currentScrollingCoroutine = null;
        }
        _isAwaitingHealAction = false;
        _isShowingDialogue = false;
    }

    public bool isTextCurrentlyScrolling => _currentScrollingCoroutine != null;

    public bool isShowingDialogueState => _isShowingDialogue || _isAwaitingHealAction;


    public void ShowDialogue(string dialogue)
    {
        if (_currentTargetObject == null || string.IsNullOrEmpty(dialogue) || _dialogueDisplayArea == null) return;

        _isShowingDialogue = true;
        _isAwaitingHealAction = false;
        _interactionEnabled = false;

        if (_promptPanel != null)
        {
            _promptPanel.SetActive(true);
        }

        if (_promptTextObject != null)
        {
            _promptTextObject.gameObject.SetActive(false);
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

        _dialogueDisplayArea.gameObject.SetActive(true);
        string formattedDialogue = $"{_currentTargetObject.name}: {dialogue}";

        if (_currentScrollingCoroutine != null)
        {
            StopCoroutine(_currentScrollingCoroutine);
        }
        _currentScrollingCoroutine = StartCoroutine(ScrollTextCoroutine(formattedDialogue));

        if (_promptPanel != null) PositionPromptPanel();
    }

    private IEnumerator ScrollTextCoroutine(string textToScroll)
    {
        if (_dialogueDisplayArea == null) { yield break; }

        _dialogueDisplayArea.text = "";
        _dialogueDisplayArea.ForceMeshUpdate();

        foreach (char letter in textToScroll)
        {
            _dialogueDisplayArea.text += letter;
            yield return new WaitForSeconds(_textScrollSpeed);
        }

        _currentScrollingCoroutine = null;
        _isShowingDialogue = false;
        _isAwaitingHealAction = true;
        _interactionEnabled = true;

        if (_buttonPromptImage != null)
        {
            _buttonPromptImage.enabled = true;
            if (_buttonPromptRectTransform != null)
            {
                _buttonPromptRectTransform.anchoredPosition = _originalButtonPromptAnchoredPosition;
            }
        }

        UpdatePromptsVisibilityAndPosition();
    }
}