using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    Player player;
    Vector2 moveDir;
    Vector2 lookDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        moveDir = Vector2.zero;
        lookDir = Vector2.zero;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {


        if (moveDir != Vector2.zero)
        {
            Vector2 newPosition = player.rigidBody.position + moveDir * player.MoveSpeed * Time.fixedDeltaTime;
            player.rigidBody.MovePosition(newPosition);
            lookDir = moveDir;
            // player.spriteFlipper.FlipByDirection(moveDir);
        }
        else
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = mouseWorldPos - transform.position;
            direction.Normalize();

            lookDir = direction;

            if (player.animator)
            {
                if (player.animator.GetBool("Walk") == true)
                {
                    player.animator.SetBool("Walk", false);
                }
                player.animator.SetFloat("X", lookDir.x);
                player.animator.SetFloat("Y", lookDir.y);
            }
        }

        Debug.Log($"LookDIr: {lookDir}");
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
        if (player.animator)
        {
            player.animator.SetFloat("X", moveDir.x);
            player.animator.SetFloat("Y", moveDir.y);

            player.animator.SetBool("Walk", true);
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        //Vector2 input = context.ReadValue<Vector2>();
        //if(input != Vector2.zero)
        //{
        //    lookDir = input;
        //    if (player.animator)
        //    {
        //        player.animator.SetFloat("X", lookDir.x);
        //        player.animator.SetFloat("Y", lookDir.y);
        //    }
        //}

    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started && !player.attackOnCooldown)
        {
            // play animation
            player.animator.SetBool("Attack", true);
            player.attackOnCooldown = true;
            StartCoroutine(DelayAttack());
        }
    }

    public void DealDamage()
    {
        player.animator.SetBool("Attack", false);
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(player.AttackCooldownTime);
        player.attackOnCooldown = false;
    }

}
