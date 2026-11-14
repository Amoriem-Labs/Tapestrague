using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    private Vector2 _direction;
    private Rigidbody2D _body;

    public Vector2 Direction => _direction;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    public void HandleMoveInput(Vector2 direction)
    {
        _direction = direction;
    }
    
    void FixedUpdate()
    {
        Vector2 offset = new Vector2(_direction.x, _direction.y) * (_speed * Time.fixedDeltaTime);
        _body.MovePosition(_body.position + offset);
    }
}