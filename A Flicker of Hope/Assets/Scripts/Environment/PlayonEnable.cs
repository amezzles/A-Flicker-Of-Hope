using UnityEngine;

public class PlayOnEnable : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (ps != null)
        {
            ps.Play();
        }
    }
}
