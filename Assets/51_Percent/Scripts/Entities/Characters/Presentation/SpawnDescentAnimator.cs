using System;
using UnityEngine;

public class SpawnDescentAnimator : MonoBehaviour
{
    [Required] [SerializeField] private SpawnDescentSettings _settings;

    private LandingWindow _landing;
    private Transform _model;
    private Vector3 _groundLocalPosition;

    public void Init(Transform model, LandingWindow landing)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (_settings == null)
            throw new InvalidOperationException(
                $"{nameof(SpawnDescentSettings)} не назначен на {name}: запусти Tools/51 Percent/Configure Spawn Animation");

        _model = model;
        _landing = landing ?? throw new ArgumentNullException(nameof(landing));
        _groundLocalPosition = _model.localPosition;
        _landing.Finished += ReturnToGround;
    }

    private void OnDestroy()
    {
        if (_landing != null)
            _landing.Finished -= ReturnToGround;
    }

    private void LateUpdate()
    {
        if (_landing == null || !_landing.IsLanding)
            return;

        _model.localPosition = _groundLocalPosition + Vector3.up * _settings.GetHeight(_landing.Progress);
    }

    private void ReturnToGround() => _model.localPosition = _groundLocalPosition;
}
