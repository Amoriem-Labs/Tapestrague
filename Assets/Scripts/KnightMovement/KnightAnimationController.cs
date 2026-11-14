using System;
using UnityEngine;

public class KnightAnimationController : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private KnightMovement _movement;

    private void Update()
    {
        _animator.SetBool(IsRunning, Mathf.Abs(_movement.Direction.x) > 0.1);
    }
}