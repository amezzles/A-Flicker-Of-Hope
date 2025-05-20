using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject _promptTextObject;
    [SerializeField] private GameObject _promptPanel;
    [SerializeField] private Image _buttonPromptImage;
    [SerializeField] private Sprite _buttonSprite;

    [SerializeField] private Vector2 _screenOffset = new Vector2(0, 50);
    private RectTransform _panelRectTransform;

    private GameObject _currentTargetObject;
    private PlayerInteract _playerInteract;
    private bool _interactionEnabled = true;

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
        if (_promptPanel != null) _panelRectTransform = _promptPanel.GetComponent<RectTransform>();

        HideAllPrompts();
    }

    void OnEnable()
    {
        if (_playerInteract != null)
        {
            _playerInteract.OnCurrentTargetChanged += HandleTargetChanged;
            _playerInteract.OnInteractWithTarget += HandleSuccessfulInteraction;
        }
    }

    void OnDisable()
    {
        if (_playerInteract != null)
        {
            _playerInteract.OnCurrentTargetChanged -= HandleTargetChanged;
            if (_playerInteract != null)
            {
                _playerInteract.OnInteractWithTarget -= HandleSuccessfulInteraction;
            }
        }
    }

    private void HandleTargetChanged(GameObject newTarget)
    {
        _currentTargetObject = newTarget;
        UpdatePromptsVisibilityAndPosition();
    }

    private void HandleSuccessfulInteraction(GameObject interactedObject)
    {
        _interactionEnabled = false;
        HideAllPrompts();
    }

    void Update()
    {
        if (_currentTargetObject != null && _promptPanel != null && _promptPanel.activeSelf && _interactionEnabled)
        {
            PositionPromptPanel();
        }
    }

    private void PositionPromptPanel()
    {
        if (playerCamera == null || _currentTargetObject == null || _panelRectTransform == null) { return; }

        Vector3 worldPos = _currentTargetObject.transform.position;
        var screenPos = playerCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z <= 0) { return; }

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
        if (_currentTargetObject != null)
        {
            if (_buttonPromptImage != null) _buttonPromptImage.gameObject.SetActive(true);
            if (_promptPanel != null) _promptPanel.SetActive(true);
            if (_promptTextObject != null) _promptTextObject.SetActive(true);
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
        if (_promptTextObject != null) _promptTextObject.SetActive(false);
        if (_buttonPromptImage != null) _buttonPromptImage.gameObject.SetActive(false);
    }
}
