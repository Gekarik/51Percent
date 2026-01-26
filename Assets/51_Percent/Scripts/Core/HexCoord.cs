public readonly struct HexCoord
{
    public readonly int Q;
    public readonly int R;

    public HexCoord(int q, int r)
    {
        Q = q;
        R = r;
    }

    public int CubeX => Q - (R - (R & 1)) / 2;
    public int CubeZ => R;
    public int CubeY => -CubeX - CubeZ;

    public bool IsOddRow => (R & 1) == 1;

    public override string ToString() => $"({Q}, {R})";
}
