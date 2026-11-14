using System;
using UnityEngine;

public class KnightAnimationController : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private KnightMovement _movement;

    private bool _isFacingRight = true;
    
    private void Update()
    {
        var isMoving = Mathf.Abs(_movement.Direction.x) > 0.1;
        _animator.SetBool(IsRunning, isMoving);
        if (isMoving)
            _isFacingRight = _movement.Direction.x > 0;

        UpdateFacing();
    }

    private void UpdateFacing()
    {
        var targetRotation = _isFacingRight ? 0 : 180;
        _animator.transform.rotation = Quaternion.Euler(0, targetRotation, 0);
    }
}