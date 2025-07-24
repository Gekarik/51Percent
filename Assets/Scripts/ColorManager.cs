using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [SerializeField] private List<Color> _colors;
    private static List<Color> _availableColors;

    private void Awake()
    {
        InitializeColors();
    }

    private void InitializeColors()
    {
        _availableColors = _colors.Distinct().ToList();

        for (int i = 0; i < _availableColors.Count; i++)
        {
            int r = Random.Range(i, _availableColors.Count);
            var tmp = _availableColors[i];
            _availableColors[i] = _availableColors[r];
            _availableColors[r] = tmp;
        }
    }

    public static Color GetRandomColor()
    {
        if (_availableColors == null || _availableColors.Count == 0)
            throw new System.InvalidOperationException("Все цвета уже использованы или ColorManager не инициализирован.");

        int lastIndex = _availableColors.Count - 1;
        Color color = _availableColors[lastIndex];
        _availableColors.RemoveAt(lastIndex);
        return color;
    }

    public void ResetColors()
    {
        InitializeColors();
    }
}
