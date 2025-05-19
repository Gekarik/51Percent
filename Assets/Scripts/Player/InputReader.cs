using UnityEngine;

public class InputReader
{
    private const string Horizontal = nameof(Horizontal);
    private const string Vertical = nameof(Vertical);

    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }

    public void UpdateInput()
    {

        MoveX = Input.GetAxis(Horizontal);
        MoveZ = Input.GetAxis(Vertical);
    }
}
