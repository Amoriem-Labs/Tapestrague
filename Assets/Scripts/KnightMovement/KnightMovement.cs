using System;
using PrototypeLevel;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    private Vector2 _direction;
    private Rigidbody2D _body;
    private Collider2D[] _overlapResults = new Collider2D[10];

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

    public bool TryInteract()
    {
        var size = _body.Overlap(ContactFilter2D.noFilter, _overlapResults);
        for (int i = 0; i < size; i++)
        {
            var coll = _overlapResults[i];
            if (!coll || !coll.TryGetComponent<IInteractablePrototype>(out var interactable)) continue;
            interactable.Interact();
            return true;
        }

        return false;
    }
}