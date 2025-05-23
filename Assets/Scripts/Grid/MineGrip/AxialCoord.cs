using System;

[Serializable]
public struct AxialCoord : IEquatable<AxialCoord>
{
    public int Q, R;

    public AxialCoord(int q, int r)
    {
        Q = q;
        R = r;
    }

    public bool Equals(AxialCoord other) => Q == other.Q && R == other.R;
    public override bool Equals(object obj) => obj is AxialCoord other && Equals(other);
    public override int GetHashCode() => (Q * 397) ^ R;
    public override string ToString() => $"({Q}, {R})";

    public static readonly (int dq, int dr)[] Directions =
    {
        (0, +1), (+1, 0), (+1, -1),
        (0, -1), (-1, 0), (-1, +1)
    };


    public static AxialCoord operator +(AxialCoord a, (int dq, int dr) b)
    => new AxialCoord(a.Q + b.dq, a.R + b.dr);
}
