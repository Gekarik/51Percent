using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorService
{
    private readonly List<Color> _availableColors;
    private readonly List<Color> _allColors;

    public ColorService(ColorSettings settings)
    {
        _allColors = settings.Colors.Distinct().ToList();
        _availableColors = _allColors.ToList();
        Shuffle(_availableColors);
    }

    public Color GetRandomColor()
    {
        if (_availableColors.Count == 0)
            throw new System.InvalidOperationException("No available colors left");

        int lastIndex = _availableColors.Count - 1;
        Color color = _availableColors[lastIndex];
        _availableColors.RemoveAt(lastIndex);
        return color;
    }

    public void ReturnColor(Color color)
    {
        if (!_availableColors.Contains(color) && _allColors.Contains(color))
        {
            _availableColors.Add(color);
        }
    }

    public void Reset()
    {
        _availableColors.Clear();
        _availableColors.AddRange(_allColors);
        Shuffle(_availableColors);
    }

    private static void Shuffle(List<Color> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
