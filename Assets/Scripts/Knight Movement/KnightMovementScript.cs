using UnityEngine;
using UnityEngine.InputSystem;

public class KnightMovementScript : MonoBehaviour
{
    // Movement code replicated from Zhongye's Unity Basics presentation.
    // Implemented new player movement scripts using BiteMe Games' tutorial:
    // https://www.youtube.com/watch?v=ONlMEZs9Rgw  <-----------------+

    [SerializeField] private float _speed = 1f;
    public InputActionReference move;
    private Vector3 direction;
    
    // Update is called once per frame
    void Update()
    {
        direction = move.action.ReadValue<Vector3>();

        Vector3 offset = new Vector3(direction.x * (_speed * Time.deltaTime), direction.y * (_speed * Time.deltaTime), 0);
        transform.position += offset;
    }
}
