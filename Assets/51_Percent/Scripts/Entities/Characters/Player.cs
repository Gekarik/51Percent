using UnityEngine;

public class Player : CharacterBase
{
    private Camera _camera;
    public Camera Camera => _camera;

    protected override void OnInit()
    {
        _camera = Camera.main;

        if (_camera != null && _camera.TryGetComponent<CameraFollower>(out var follower))
            follower.Init(transform);
    }
}
