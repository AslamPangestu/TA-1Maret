using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour {

    Animator animator;
    CharacterController characterController;

    [System.Serializable]
    public class AnimationSetting
    {
        //nama variabel pada animator
        public string verticalVelovityFloat = "Forward";
        public string horizontalVelovityFloat = "Strafe";
        public string groundedBool = "isGrounded";
        public string jumpingBool= "isJumping";
    }
    [SerializeField]
    public AnimationSetting animationSetting;

    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModifier = 9.81f;
        public float baseGravity = 50.0f;
        public float resetGravityValue = 0.0f;
    }
    [SerializeField]
    public PhysicsSettings physicsSettings;

    [System.Serializable]
    public class MovementSettings
    {
        public float jumpSpeed = 6.0f;
        public float jumpTime = 0.25f;
    }

    [SerializeField]
    public MovementSettings movementSettings;

    bool isJumping;
    bool isGrounded = true;
    bool resetGravity;
    float gravity;

    void Awake() {
        animator = GetComponent<Animator>();
        SetupAnimator();
    }

    // Use this for initialization
    void Start () {
        //Mengambil komponen dari objek
        characterController = GetComponent<CharacterController>();

        SetupAnimator();
	}

    // Update is called once per frame
    void Update () {
        ApplyGravity();
        isGrounded = characterController.isGrounded;
	}

    void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            if (!resetGravity)
            {
                gravity = physicsSettings.resetGravityValue;
                resetGravity = true;
            }
            gravity += Time.deltaTime * physicsSettings.gravityModifier;
        }
        else
        {
            gravity = physicsSettings.baseGravity;
            resetGravity = false;
        }
        Vector3 gravityVector = new Vector3();

        if (!isJumping)
        {
            gravityVector.y -= gravity;
        }
        else
        {
            gravityVector.y = movementSettings.jumpSpeed;
        }

        characterController.Move(gravityVector * Time.deltaTime);
    }

    // public void Move (float forward, float strafe){
    //     ApplyGravity();
    //     Animate(forward,strafe);
    //     Vector3 moveVector = new Vector3(strafe, -gravity, forward);
    //     moveVector.z *= Time.deltaTime;
    //     moveVector.x *= Time.deltaTime;
    //     characterController.Move(moveVector);
    // }
    //animasi pada character
    public void Animate(float forward, float strafe)
    {
        animator.SetFloat(animationSetting.verticalVelovityFloat, forward);
        animator.SetFloat(animationSetting.horizontalVelovityFloat, strafe);
        animator.SetBool(animationSetting.groundedBool, isGrounded);
        animator.SetBool(animationSetting.jumpingBool, isJumping); 
    }

    public void Jump()
    {
        if (isJumping)
        {
            return;
        }
        if (isGrounded)
        {
            isJumping = true;
            StartCoroutine(StopJump());
        }
    }

    IEnumerator StopJump()
    {
        yield return new WaitForSeconds(movementSettings.jumpTime);
        isJumping = false;
    }

    //mengatur animasi dengan child avatar
    void SetupAnimator()
    {
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvatar = wantedAnim.avatar;

        animator.avatar = wantedAvatar;
        Destroy(wantedAnim);
    }
}
