using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    Player player;
    Vector2 moveDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        moveDir = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if(moveDir != Vector2.zero)
        {
            Vector2 newPosition = player.rigidBody.position + moveDir * player.MoveSpeed * Time.fixedDeltaTime;
            player.rigidBody.MovePosition(newPosition);
            player.spriteFlipper.FlipByDirection(moveDir);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

}
