using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
public class CameraEditor : Editor {

    CameraController cameraController;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        cameraController = (CameraController)target;

        EditorGUILayout.LabelField("Camera Helper");
        if(GUILayout.Button("Save Camera position now"))
        {
            Camera camera = Camera.main;
            if (camera)
            {
                Transform cameraTransform = camera.transform;
                Vector3 cameraPosition = cameraTransform.localPosition;
                Vector3 cameraRight = cameraPosition;
                Vector3 cameraLeft = cameraPosition;
                cameraLeft.x = -cameraPosition.x;
                cameraController.cameraSettings.cameraPositionOffsetRight = cameraRight;
                cameraController.cameraSettings.cameraPositionOffsetLeft = cameraLeft;
            }
        }
    }

}
