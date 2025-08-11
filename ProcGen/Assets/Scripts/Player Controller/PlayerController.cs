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

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started && !player.attackOnCooldown)
        {
            if(player.weaponManager.CurrentWeaponIsSword() && player.animator.GetBool("Walk") == false)
            {
                // play animation
                player.animator.SetBool("Attack", true);
                player.attackOnCooldown = true;
                StartCoroutine(DelayAttack());

                if (SoundManager.Instance && SoundLibrary.Instance)
                    SoundManager.Instance.PlaySoundEffect(
                        SoundLibrary.Instance.GetAudioClip(SoundLibrary.Player.SWORD_SFX),
                        volumeScale: 1);
            }
            else if (player.weaponManager)
            {
                player.weaponManager.Fire();

            }
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (UIManager.Instance)
            UIManager.Instance.TogglePauseMenu();
    }

    public void DealDamage()
    {
        if (!player || !player.weaponManager)
        {
            Debug.Log("PlayerController.DealDamage: Player or WeaponManager does not exist");
            return;
        }


        player.animator.SetBool("Attack", false);

        // get the sword Collider container transform
        Transform swordColliderParent = player.transform.Find("SwordCollider");

        if (!swordColliderParent || swordColliderParent.childCount == 0)
        {
            Debug.Log("Could not find sword collider parent or it has no child");
            return;
        }

        Transform swordDirectionColliderTransform = null;
        // figure out which dir NE,NW,SE,SW

        string direction = GetStringDirection(new Vector2(player.animator.GetFloat("X"),
                                                          player.animator.GetFloat("Y")));

        if (string.IsNullOrEmpty(direction))
        {
            Debug.Log("Could not determine the direction");
            return;
        }

        swordDirectionColliderTransform = swordColliderParent.Find(direction);

        if (!swordDirectionColliderTransform)
        {
            Debug.Log("Could not find sword collider transform");
            return;
        }

        SwordDirectionalCollider swordDirectionalCollider = swordDirectionColliderTransform.GetComponent<SwordDirectionalCollider>();

        if(!swordDirectionalCollider)
        {
            Debug.Log("sword collider gameobject has not swordDirectionalCollider component");
            return;
        }

        // get sword
        BaseWeapon weapon = player.weaponManager.GetCurrentBaseWeapon();
        if (!weapon)
            return;

        swordDirectionalCollider.DealDamage(weapon.GetDamage());
    }

    private string GetStringDirection(Vector2 direction)
    {
        if (direction.x > 0 && direction.y > 0) return "NorthEast";
        if (direction.x < 0 && direction.y > 0) return "NorthWest";
        if (direction.x < 0 && direction.y < 0) return "SouthWest";
        return "SouthEast";
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(player.AttackCooldownTime);
        player.attackOnCooldown = false;
    }

}
