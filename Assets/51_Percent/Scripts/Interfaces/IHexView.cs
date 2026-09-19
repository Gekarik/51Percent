using UnityEngine;

public interface IHexView
{
    void SetMesh(Mesh mesh);
    void SetOutline(bool visible);
    void SetColorInstantly(Color color);
    void SetColorSlowly(Color color);
    void ResetColor();
    void Pulse();
    void Reset();
}
