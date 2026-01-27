using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorSettings", menuName = "51_Percent/ColorSettings")]
public class ColorSettings : ScriptableObject
{
    [SerializeField] private List<Color> _colors;

    public IReadOnlyList<Color> Colors => _colors;
}
