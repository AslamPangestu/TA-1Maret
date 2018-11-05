using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    CharacterMovement characterMovement;
    WeaponHandler weaponHandler;

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpBtn = "Jump";
        public string reloadBtn = "Reload";
        public string aimBtn = "Aim";
        public string fireBtn = "Fire";
        public string dropWeaponBtn = "DropWeapon";
        public string switchWeaponBtn = "SwitchWeapon";
    }
    [SerializeField]
    InputSettings inputSettings;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 10.0f;
        public bool requiredInputForTurn = true;
        public LayerMask aimDetectionLayer;
    }
    [SerializeField]
    OtherSettings otherSettings;

    Camera mainCamera;

    public bool debugAim;
    public Transform spine;
    bool aiming;

    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        mainCamera = Camera.main;
        weaponHandler = GetComponent<WeaponHandler>();
    }

    // Update is called once per frame
    void Update () {
       CharacterLogic();
       CamLogic();
       WeaponLogic();
    }

    void LateUpdate() {
        if(weaponHandler){
            if(weaponHandler.curWeapon){
                if(aiming){
                    SpinePos();
                }
            }
        }    
    }

    //handel char logic
    void CharacterLogic(){
        if(!characterMovement){
            return;
        }
        characterMovement.Animate(Input.GetAxis(inputSettings.verticalAxis),Input.GetAxis(inputSettings.horizontalAxis));
        if(Input.GetButtonDown(inputSettings.jumpBtn)){
            characterMovement.Jump();
        }
    }

    //hadel cam logic
    void CamLogic(){
        if(!mainCamera){
            return;
        }
        if (otherSettings.requiredInputForTurn)
        {
            if (Input.GetAxis(inputSettings.horizontalAxis) != 0 || Input.GetAxis(inputSettings.verticalAxis) != 0){
                CharacterLook();
            }
        }else{
            CharacterLook();
        }
    }

    //spine pos when aim
    void SpinePos(){
        if(!spine || !weaponHandler.curWeapon || !mainCamera){
            return;
        }
        RaycastHit hit;
        Transform mainCamTransform = mainCamera.transform;
        Vector3 mainCamPosition = mainCamTransform.position;
        Vector3 dir = mainCamTransform.forward;
        Ray ray = new Ray(mainCamPosition,dir);

        if(Physics.Raycast(ray,out hit,100,otherSettings.aimDetectionLayer)){
            Vector3 hitPoint = hit.point;
            spine.LookAt(hitPoint);
        }else{
            spine.LookAt(ray.GetPoint(50));
        }
        Vector3 eulerAngelOffset = weaponHandler.curWeapon.userSettings.spineRotation;
        spine.Rotate(eulerAngelOffset);
    }
    //handle weapon logic
    void WeaponLogic(){
        if(!weaponHandler){
            return;
        }
        aiming = Input.GetButtonDown(inputSettings.aimBtn);

        if(weaponHandler.curWeapon){
            weaponHandler.Aiming(aiming);
            otherSettings.requiredInputForTurn = !aiming;
            weaponHandler.FingerOnTrigger(Input.GetButton(inputSettings.fireBtn));
            if(Input.GetButtonDown(inputSettings.reloadBtn)){
                weaponHandler.Reloading();
            }
            if(Input.GetButtonDown(inputSettings.dropWeaponBtn)){
                weaponHandler.DropCurWeapon();
            }
            if(Input.GetButtonDown(inputSettings.switchWeaponBtn)){
                weaponHandler.SwitchWeapon();
            }
        }
    }
    void CharacterLook()
    {
        Transform mainCameraTransofrm = mainCamera.transform;
        Vector3 mainCameraPosition = mainCameraTransofrm.position;
        Vector3 lookTarget = mainCameraPosition + 
            (mainCameraTransofrm.forward * otherSettings.lookDistance);
        Vector3 thisPosition = transform.position;
        Vector3 lookDirection = lookTarget - thisPosition;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        lookRotation.x = 0;
        lookRotation.z = 0;

        Quaternion newRotation = Quaternion.Lerp(transform.rotation, 
            lookRotation,
            Time.deltaTime * otherSettings.lookSpeed);
        transform.rotation = newRotation;
    }
}
