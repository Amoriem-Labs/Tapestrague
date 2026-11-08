using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    // Script from YouTube tutorial by Anup Prasad.
    
    [SerializeField] private float followSpeed = 1f;
    [SerializeField] private Transform knight;
    [SerializeField] private float yOffset = 1f;
    private float xOffset = 1f;

    public InputActionReference move;
    private Vector3 direction;
    private Vector3 newPos;

    // Update is called once per frame
    void Update()
    {
        direction = move.action.ReadValue<Vector3>();

        if (direction.x < 0) 
        {
            xOffset = -3f;
        }
        else if (direction.x > 0)
        {
            xOffset = 3f;
        }
        else
        {
            xOffset *= 1f;
        };

        Vector3 newPos = new Vector3(knight.position.x + xOffset, knight.position.y + yOffset, -10f);
        transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
    }
}
