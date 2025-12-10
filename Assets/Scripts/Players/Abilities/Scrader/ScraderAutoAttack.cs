using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScraderAutoAttack : MonoBehaviour
{
    [SerializeField] private SpellMoveScraderTo spellMoveTo;

    public void OnAutoAttackAnimationHitScrader() => spellMoveTo.OnAutoAttackAnimationHit();
    public void OnAutoAttackAnimationEndScrader() => spellMoveTo.OnAutoAttackAnimationEnd();
}
