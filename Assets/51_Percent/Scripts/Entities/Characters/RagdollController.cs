using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private const float DeactivateDelay = 2.5f;
    private const float ExplosionForceMin = 4f;
    private const float ExplosionForceMax = 8f;
    private const float ExplosionRadius = 1.5f;
    private const float UpwardModifier = 1.5f;
    private const float TorqueStrength = 3f;

    [SerializeField] private GameObject _deathVfxPrefab;
    [Layer] [SerializeField] private int _ragdollLayer;

    // корневые компоненты персонажа — ищем вверх по иерархии
    private Rigidbody _characterRigidbody;
    private Collider _characterCollider;

    // кости рагдолла — только дочерние объекты CharacterView, корень сюда не попадает
    private Rigidbody[] _ragdollBodies;
    private Collider[] _ragdollColliders;
    private Animator _animator;

    private void Awake()
    {
        _characterRigidbody = GetComponentInParent<Rigidbody>();
        _characterCollider = GetComponentInParent<Collider>();
        _animator = GetComponent<Animator>();

        _ragdollBodies = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdollActive(false);
    }

    public void Activate(Color characterColor, Vector3 externalImpulse = default)
    {
        Vector3 inheritedVelocity = _characterRigidbody.velocity;

        _animator.enabled = false;
        _characterRigidbody.isKinematic = true;
        _characterRigidbody.constraints = RigidbodyConstraints.None;

        SetRagdollActive(true);
        SetRagdollLayer();
        SpawnDeathVfx(characterColor);

        StartCoroutine(ApplyForcesNextFrame(inheritedVelocity, externalImpulse));

        DOVirtual.DelayedCall(DeactivateDelay, () => gameObject.SetActive(false));
    }

    private IEnumerator ApplyForcesNextFrame(Vector3 inheritedVelocity, Vector3 externalImpulse)
    {
        yield return new WaitForFixedUpdate();

        Vector3 explosionCenter = _ragdollBodies[0].position;
        float explosionForce = Random.Range(ExplosionForceMin, ExplosionForceMax);

        foreach (var rb in _ragdollBodies)
        {
            rb.velocity = inheritedVelocity;
            rb.AddExplosionForce(explosionForce, explosionCenter, ExplosionRadius, UpwardModifier, ForceMode.Impulse);
        }

        Vector3 randomTorque = Random.insideUnitSphere * TorqueStrength;
        _ragdollBodies[0].AddTorque(randomTorque, ForceMode.Impulse);

        if (externalImpulse != Vector3.zero)
            _ragdollBodies[0].AddForce(externalImpulse, ForceMode.Impulse);
    }

    private void SetRagdollActive(bool active)
    {
        _characterCollider.enabled = !active;

        foreach (var rb in _ragdollBodies)
            rb.isKinematic = !active;

        foreach (var col in _ragdollColliders)
            col.enabled = active;
    }

    private void SetRagdollLayer()
    {
        foreach (var rb in _ragdollBodies)
            rb.gameObject.layer = _ragdollLayer;
    }

    private void SpawnDeathVfx(Color color)
    {
        if (_deathVfxPrefab == null)
            return;

        Vector3 spawnPosition = _ragdollBodies.Length > 0
            ? _ragdollBodies[0].position
            : transform.position;

        GameObject vfx = Instantiate(_deathVfxPrefab, spawnPosition, Quaternion.identity);

        if (vfx.TryGetComponent<ParticleSystem>(out var ps))
        {
            var main = ps.main;
            main.startColor = color;
        }

        if (vfx.TryGetComponent<ParticleSystem>(out var rootPs))
            Destroy(vfx, rootPs.main.duration + rootPs.main.startLifetime.constantMax);
        else
            Destroy(vfx, 3f);
    }
}
