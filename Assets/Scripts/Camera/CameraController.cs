using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour {

    public Transform target;
    public bool autoTargetPlayer;
    public LayerMask wallLayers;

    public enum Shoulder
    {
        Right, Left
    }
    public Shoulder shoulder;

    [System.Serializable]
    public class CameraSettings
    {
        [Header("-Positioning-")]
        public Vector3 cameraPositionOffsetLeft;
        public Vector3 cameraPositionOffsetRight;

        [Header("-Camera Options-")]
        public float mouseXSensitivity = 5.0f;
        public float mouseYSensitivity = 5.0f;
        public float minAngel = -30.0f;
        public float maxAngel = 70.0f;
        public float rotationSpeed = 5.0f;
        public float maxCheckDistance = 0.1f;

        [Header("-Aim-")]
        public float fieldOfView = 70.0f;
        public float zoomFieldOfView = 30.0f;
        public float zoomSpeed = 3.0f;

        [Header("-Visual Options-")]
        public float hideMeshWhenDistance = 0.5f;
    }
    [SerializeField]
    public CameraSettings cameraSettings;

    [System.Serializable]
    public class MovementSetting
    {
        public float movementLerpSpeed = 5.0f;
    }
    [SerializeField]
    MovementSetting movementSetting;

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Mouse Y";
        public string horizontalAxis = "Mouse X";
        public string aimBtn = "Aim";
        public string switchShoulderBtn = "ChangeShoulder";
    }
    [SerializeField]
    InputSettings inputSettings;

    Transform pivot;
    Camera mainCamera;
    float newX = 0.0f;
    float newY = 0.0f;

    // Use this for initialization
    void Start () {
        mainCamera = Camera.main;
        pivot = transform.GetChild(0);// get gameobject pivot with index 0
	}

    // Update is called once per frame
    void Update () {
        if (target)
        {
            if (Application.isPlaying)
            {
                //lock screen+ hide cursor
                RotateCamera();
                CheckWall();
                CheckMeshRenderer();
                Aiming(Input.GetButton(inputSettings.aimBtn));
                if (Input.GetButton(inputSettings.switchShoulderBtn))
                {
                    SwitchShoulder();
                }
            }
        }
	}

    void LateUpdate()
    {
        if (!target)
        {
            TargetPlayer();
        }
        else
        {
            Vector3 targetPosition = target.position;
            Quaternion targetRotation = target.rotation;
            FollowTarget(targetPosition, targetRotation);
        }
    }

    void FollowTarget(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!Application.isPlaying)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            Vector3 newPosition = Vector3.Lerp(transform.position,
                targetPosition,
                Time.deltaTime * movementSetting.movementLerpSpeed);
            transform.position = newPosition;
        }
    }

    void TargetPlayer()
    {
        if (autoTargetPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                Transform playerTransform = player.transform;
                target = playerTransform;
            }
        }
    }

    public void SwitchShoulder()
    {
        switch (shoulder)
        {
            case Shoulder.Left:
                shoulder = Shoulder.Right;//pindah ke shoulder right
                break;
            case Shoulder.Right:
                shoulder = Shoulder.Left;//pindah ke shoulder left
                break;
        }
    }

    void Aiming(bool isAiming)
    {
        if (!mainCamera)
        {
            return;
        }
        if (isAiming)
        {
            float newFieldOfView = Mathf.Lerp(mainCamera.fieldOfView,
                cameraSettings.zoomFieldOfView,
                Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = newFieldOfView;
        }
        else
        {
            float originalFieldOfiew = Mathf.Lerp(mainCamera.fieldOfView,
                cameraSettings.fieldOfView,
                Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = originalFieldOfiew;
        }
    }

    void CheckMeshRenderer()
    {
        if(!mainCamera || !target)
        {
            return;
        }
        SkinnedMeshRenderer[] skinnedMeshRenderers = 
            target.GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraPosition = mainCameraTransform.position;
        Vector3 targetPosition = target.position;
        float distance = Vector3.Distance(mainCameraPosition, 
            (targetPosition + target.up));

        if (skinnedMeshRenderers.Length > 0)
        {
            for(int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if(distance <= cameraSettings.hideMeshWhenDistance)
                {
                    skinnedMeshRenderers[i].enabled = false;
                }
                else
                {
                    skinnedMeshRenderers[i].enabled = true;
                }
            }
        }
    }

    void CheckWall()
    {
        if(!pivot || !mainCamera)
        {
            return;
        }
        RaycastHit hit;

        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraPosition = mainCameraTransform.position;
        Vector3 pivotPosition = pivot.position;

        Vector3 start = pivotPosition;
        Vector3 direction = mainCameraPosition - pivotPosition;

        float distance = Mathf.Abs(shoulder == Shoulder.Left ? 
            cameraSettings.cameraPositionOffsetLeft.z : //jika shoulder left, akan menjalankan ini
            cameraSettings.cameraPositionOffsetRight.z);//jika shoulder right, akan menjalankan ini

        if(Physics.SphereCast(start, 
            cameraSettings.maxCheckDistance, 
            direction, out hit, distance, wallLayers))
        {
            MoveCameraUp(hit, pivotPosition, 
                direction, mainCameraTransform);
        }
        else
        {
            switch (shoulder)
            {
                case Shoulder.Left:
                    PositionCamera(cameraSettings.cameraPositionOffsetLeft);//memindahkan kamera kekiri
                    break;
                case Shoulder.Right:
                    PositionCamera(cameraSettings.cameraPositionOffsetRight);//memindahkan kamera kekanan
                    break;
            }
        }
    }

    void MoveCameraUp(RaycastHit hit, Vector3 pivotPosition, 
        Vector3 direction, Transform cameraTransform)
    {
        float hitDistance = hit.distance;
        Vector3 sphereCastCenter = pivotPosition + 
            (direction.normalized * hitDistance);
        cameraTransform.position = sphereCastCenter;
    }

    void PositionCamera(Vector3 cameraPosition)
    {
        if (!mainCamera)
        {
            return;
        }
        Transform mainCameraTransform = mainCamera.transform;
        Vector3 mainCameraPosition = mainCameraTransform.localPosition;
        Vector3 newPosition = Vector3.Lerp(mainCameraPosition, 
            cameraPosition, 
            Time.deltaTime * movementSetting.movementLerpSpeed);
        mainCameraTransform.localPosition = newPosition;
    }

    void RotateCamera()
    {
        if (!pivot)
        {
            return;
        }
        newX += cameraSettings.mouseXSensitivity * Input.GetAxis(inputSettings.horizontalAxis);
        newY += cameraSettings.mouseYSensitivity * Input.GetAxis(inputSettings.verticalAxis);

        Vector3 eulerAngelAxis = new Vector3();
        eulerAngelAxis.x = newY;
        eulerAngelAxis.y = newX;

        newX = Mathf.Repeat(newX, 360);
        newY = Mathf.Clamp(newY, 
            cameraSettings.minAngel, 
            cameraSettings.maxAngel);

        Quaternion newRotation = Quaternion.Slerp(pivot.localRotation, 
            Quaternion.Euler(eulerAngelAxis), 
            Time.deltaTime * cameraSettings.rotationSpeed);

        pivot.localRotation = newRotation;
    }
}
