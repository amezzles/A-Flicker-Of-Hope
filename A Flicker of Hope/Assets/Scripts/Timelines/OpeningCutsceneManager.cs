//using UnityEngine;
//using System.Collections;

//public class OpeningCutsceneManager : MonoBehaviour
//{
//    [SerializeField] private GameObject player;
//    [SerializeField] private Camera cutsceneCamera;
//    [SerializeField] private float cutsceneDuration = 5f;

//    private PlayerInteract _playerInteract;

//    void Start()
//    {
//        _playerInteract = player.GetComponent<PlayerInteract>();

//        if (_playerInteract != null)
//        {
//            _playerInteract.InputEnabled(false);
//        }

//        if (cutsceneCamera != null)
//        {
//            cutsceneCamera.gameObject.SetActive(true);
//            Camera.main.gameObject.SetActive(false);
//        }

//        // Start cutscene coroutine
//        StartCoroutine(PlayCutscene());
//    }

//    private IEnumerator PlayCutscene()
//    {
//        yield return new WaitForSeconds(cutsceneDuration);

//        EndCutscene();
//    }

//    private void EndCutscene()
//    {
//        if (cutsceneCamera != null)
//        {
//            cutsceneCamera.gameObject.SetActive(false);
//        }
//        if (Camera.main != null)
//        {
//            Camera.main.gameObject.SetActive(true);
//        }

//        // Re-enable player input
//        if (_playerInteract != null)
//        {
//            _playerInteract.InputEnabled(true);
//        }
//    }
//}
