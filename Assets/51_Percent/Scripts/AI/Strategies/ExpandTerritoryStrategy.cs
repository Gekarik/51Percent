using UnityEngine;

public class ExpandTerritoryStrategy : IBotStrategy
{
    private const int ScanDirections = 8;
    private const int ScanSteps = 5;
    private const float ScanStepDistance = 2f;

    public StrategyType Type => StrategyType.ExpandTerritory;

    private Vector3 _outwardDirection;
    private Vector3 _lateralDirection;
    private bool _hasDirection;
    private int _phaseOneLength;
    private bool _inLateralPhase;

    public float? Evaluate(BotContext context)
    {
        if (context.TrailLength >= context.Personality.MaxTrailLength)
            return null;

        float utility = context.Personality.TerritorialFocus * 3f;

        if (!context.HasTrail)
            utility += 1f;

        // Бонус за продолжение петли — не даём ReturnHome перебить на полпути
        if (context.HasTrail && _hasDirection)
            utility += 2f;

        return utility;
    }

    public Vector3 GetTargetPosition(BotContext context)
    {
        if (!_hasDirection)
        {
            PlanLoop(context);
        }

        // Переход в латеральную фазу по длине trail
        if (!_inLateralPhase && context.TrailLength >= _phaseOneLength)
        {
            _inLateralPhase = true;
            Debug.Log($"[{context.BotTransform.name}] Expand: Outward → Lateral (trail={context.TrailLength})");
        }

        Vector3 currentDirection = _inLateralPhase ? _lateralDirection : _outwardDirection;

        // Проверяем край карты
        Vector3 checkPos = context.Position + currentDirection * 3f;
        IHex hexAhead = context.Grid.GetHexAt(checkPos);

        if (hexAhead == null)
        {
            if (!_inLateralPhase)
            {
                // Край карты в фазе outward — досрочный переход в lateral
                _inLateralPhase = true;
                currentDirection = _lateralDirection;

                Vector3 lateralCheck = context.Position + _lateralDirection * 3f;
                if (context.Grid.GetHexAt(lateralCheck) == null)
                {
                    _lateralDirection = -_lateralDirection;
                    currentDirection = _lateralDirection;
                }

                Debug.Log($"[{context.BotTransform.name}] Expand: Edge hit → Lateral");
            }
            else
            {
                _lateralDirection = -_lateralDirection;
                currentDirection = _lateralDirection;
            }
        }

        return context.Position + currentDirection * 5f;
    }

    public bool IsBlocking(BotContext context) => false;

    public void OnActivate(BotContext context)
    {
        if (!_hasDirection)
        {
            PlanLoop(context);
        }
    }

    public void OnDeactivate(BotContext context) { }

    private void PlanLoop(BotContext context)
    {
        _outwardDirection = FindBestExpansionDirection(context);

        // Латеральное направление: перпендикуляр к outward, случайно влево или вправо
        float lateralSign = Random.value > 0.5f ? 1f : -1f;
        _lateralDirection = new Vector3(
            -_outwardDirection.z * lateralSign,
            0f,
            _outwardDirection.x * lateralSign
        );

        _phaseOneLength = Mathf.Max(2,
            Mathf.RoundToInt(context.Personality.MaxTrailLength * Random.Range(0.25f, 0.4f)));

        _inLateralPhase = false;
        _hasDirection = true;

        Debug.Log($"[{context.BotTransform.name}] Expand: PlanLoop dir={_outwardDirection}, " +
                  $"lateral={_lateralDirection}, phaseOneLen={_phaseOneLength}");
    }

    /// <summary>
    /// Сканирует 8 направлений и выбирает то, где больше пустых/вражеских гексов.
    /// Это не даёт боту бесконечно бродить по своей же территории.
    /// </summary>
    private Vector3 FindBestExpansionDirection(BotContext context)
    {
        Vector3 fromCenter = context.Position - context.GetTerritoryCenter();
        fromCenter.y = 0f;
        Vector3 centerDir = fromCenter.magnitude > 0.5f ? fromCenter.normalized : Vector3.zero;

        Vector3 bestDirection = Vector3.forward;
        float bestScore = float.MinValue;

        for (int i = 0; i < ScanDirections; i++)
        {
            float angle = i * (360f / ScanDirections) * Mathf.Deg2Rad;
            Vector3 candidate = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            float score = 0f;

            for (int step = 1; step <= ScanSteps; step++)
            {
                Vector3 checkPos = context.Position + candidate * step * ScanStepDistance;
                IHex hex = context.Grid.GetHexAt(checkPos);

                if (hex == null)
                {
                    score -= 10f;
                    break;
                }

                if (hex.State == HexState.Empty)
                    score += 3f;
                else if (hex.State == HexState.Busy && hex.Owner != context.Character)
                    score += 2f;
                // Свои Busy гексы не дают очков — направление через свою территорию не привлекает
            }

            // Небольшой бонус за направление от центра территории
            if (centerDir.sqrMagnitude > 0.01f)
                score += Vector3.Dot(candidate, centerDir) * 0.5f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidate;
            }
        }

        // Случайное отклонение для естественности
        float randomAngle = Random.Range(-20f, 20f) * Mathf.Deg2Rad;
        return new Vector3(
            bestDirection.x * Mathf.Cos(randomAngle) - bestDirection.z * Mathf.Sin(randomAngle),
            0f,
            bestDirection.x * Mathf.Sin(randomAngle) + bestDirection.z * Mathf.Cos(randomAngle)
        );
    }

    public void ResetDirection()
    {
        _hasDirection = false;
        _inLateralPhase = false;
    }
}
