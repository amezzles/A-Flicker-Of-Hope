using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class OpeningSceneManager : MonoBehaviour
{
    public PlayableDirector openingDirector;
    public GameObject player;
    public GameObject hud;
    public Camera playerCamera;
    public Camera cinematicCamera;

    private PlayerInput playerInput;
  

    void Start()
    {
        hud.SetActive(false);

        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        //if (openingDirector != null && !openingDirector.playOnAwake)
        //{
        //    openingDirector.Play();
        //}

        //swtich cameras
        if (playerCamera != null) playerCamera.enabled = false;
        if (cinematicCamera != null) cinematicCamera.enabled = true;

        //enable after scene ends
        openingDirector.stopped += OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        hud.SetActive(true);
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        if (cinematicCamera != null) cinematicCamera.enabled = false;
        if (playerCamera != null) playerCamera.enabled = true;

        //remove callback to prevent memory leaks
        openingDirector.stopped -= OnTimelineFinished;
    }
}
