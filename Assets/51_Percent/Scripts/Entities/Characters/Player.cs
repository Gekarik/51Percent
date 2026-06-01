using UnityEngine;

public class Player : CharacterBase
{
    [SerializeField] private KeyCode _activateBoosterKey = KeyCode.Space;

    private Camera _camera;
    public override bool IsHuman => true;
    public Camera Camera => _camera;

    protected override void OnInit()
    {
        _camera = Camera.main;

        if (_camera != null && _camera.TryGetComponent<CameraFollower>(out var follower))
            follower.Init(transform);
    }

    protected override void OnBoosterCollected(Booster booster) =>
        StorePendingBooster(booster.CreateEffect());

    private void Update()
    {
        if (Input.GetKeyDown(_activateBoosterKey))
            ActivatePendingBooster();
    }
}
