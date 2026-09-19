using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

// Принудительный запуск состояния графа в Play Mode: обходит условия переходов,
// чтобы посмотреть конкретную анимацию не воспроизводя игровую ситуацию
public class AnimationStatePlayer
{
    private const int Layer = 0;

    public IReadOnlyList<string> GetStateNames(Animator animator)
    {
        if (animator == null || !(animator.runtimeAnimatorController is AnimatorController controller))
            return new string[0];

        return controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(child => child.state.name)
            .ToArray();
    }

    public void Play(Animator animator, string stateName)
    {
        animator.Play(stateName, Layer, 0f);
    }
}
