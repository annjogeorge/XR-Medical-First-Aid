using UnityEngine;

public class ClingFilmTrigger : MonoBehaviour
{
    private ClingFilmApplicator _applicator;

    void Start()
    {
        _applicator = GetComponentInParent<ClingFilmApplicator>();
    }

    void OnTriggerEnter(Collider other)
        => _applicator?.OnBurnEnter(other);

    void OnTriggerStay(Collider other)
        => _applicator?.OnBurnStay(other);

    void OnTriggerExit(Collider other)
        => _applicator?.OnBurnExit(other);
}