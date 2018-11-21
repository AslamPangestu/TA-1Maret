using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor {

	Weapon weapon;
	
	public override void OnInspectorGUI(){
		base.OnInspectorGUI();
		weapon = (Weapon) target;

		EditorGUILayout.LabelField("Wepon Helper");
		if(GUILayout.Button("Save gun equip location")){
			Transform weaponTransform = weapon.transform;
			Vector3 weaponPos = weaponTransform.localPosition;
			Vector3 weaponRot = weaponTransform.localEulerAngles;
			weapon.weaponSettings.equipPos = weaponPos;
			weapon.weaponSettings.equipRot = weaponRot;
		}
		if(GUILayout.Button("Save gun unequip location")){
			Transform weaponTransform = weapon.transform;
			Vector3 weaponPos = weaponTransform.localPosition;
			Vector3 weaponRot = weaponTransform.localEulerAngles;
			weapon.weaponSettings.unequipPos = weaponPos;
			weapon.weaponSettings.unequipRot = weaponRot;
		}

		EditorGUILayout.LabelField("Debug Pos");
		if(GUILayout.Button("Move gun to equip location")){
			Transform weaponTransform = weapon.transform;
			weaponTransform.localPosition = weapon.weaponSettings.equipPos;
			Quaternion eulerAngels = Quaternion.Euler(weapon.weaponSettings.equipRot);
			weaponTransform.localRotation = eulerAngels;
		}
		if(GUILayout.Button("Move gun to unequip location")){
			Transform weaponTransform = weapon.transform;
			weaponTransform.localPosition = weapon.weaponSettings.unequipPos;
			Quaternion eulerAngels = Quaternion.Euler(weapon.weaponSettings.unequipRot);
			weaponTransform.localRotation = eulerAngels;
		}
	}
}
