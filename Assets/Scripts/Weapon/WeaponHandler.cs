using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour {

	Animator animator;

	[System.Serializable]
	public class UserSettings{
		public Transform rightHand;
		public Transform pistolUnequipSpot;
		public Transform rifleUnequipSpot;
	}
	[SerializeField]
	public UserSettings userSettings;

	[System.Serializable]
	public class Animations{
		public string weaponTypeInt = "WeaponType";
		public string reloadingBool = "isReloading";
		public string aimingBool = "isAiming";
	}
	[SerializeField]
	public Animations animations;

	public Weapon curWeapon;
	public List<Weapon> weaponList  = new List<Weapon>();
	public int maxWeapon = 2;
	bool aim;
	bool reload;
	int weaponType;
	bool settingWeapon;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(curWeapon){
			curWeapon.SetEquipped(true);
			curWeapon.SetOwner(this);
			AddWeapon(curWeapon);

			if(curWeapon.ammo.clipAmmo <=0){
				Reloading();
			}
		}
		if(weaponList.Count > 0){
			for(int i = 0; i<weaponList.Count; i++){
				if(weaponList[i] != curWeapon){
					weaponList[i].SetEquipped(false);
					weaponList[i].SetOwner(this);
				}
			}
		}
		Animate();
	}

	void Animate(){
		if(!animator){
			return;
		}
		animator.SetBool(animations.aimingBool,aim);
		animator.SetBool(animations.reloadingBool,reload);
		animator.SetInteger(animations.weaponTypeInt,weaponType);

		if(!curWeapon){
			weaponType=0;
			return;
		}

		switch(curWeapon.weaponType){
			case Weapon.WeaponType.Pistol:
				weaponType=1;
				break;
			case Weapon.WeaponType.Rifle:
				weaponType=2;
				break;
		}
	}

	//add weapon to weaponlist
	void AddWeapon(Weapon weapon){
		if(weaponList.Contains(weapon)){
			return;
		}
		weaponList.Add(weapon);
	}

	//put finger on triger and ask if we pulled
	public void FingerOnTrigger(bool pulling){
		if(!curWeapon){
			return;
		}
		curWeapon.PullTrigger(pulling);
	}

	//reload current weapon
	public void Reloading(){
		if(reload || !curWeapon){
			return;
		}
		if(curWeapon.ammo.carryAmmo <= 0 || curWeapon.ammo.clipAmmo == curWeapon.ammo.maxAmmo){
			return;
		}
		StartCoroutine(StopReload());
	}

	//stop reloading weapon
	IEnumerator StopReload(){
		yield return new WaitForSeconds(curWeapon.weaponSettings.reloadDuration);
		curWeapon.LoadClip();
		reload = false;
	}

	//set aim bool
	public void Aiming(bool aiming){
		aim = aiming;
	}

	//drop current weapon
	public void DropCurWeapon(){
		if(!curWeapon){
			return;
		}
		curWeapon.SetEquipped(false);
		curWeapon.SetOwner(null);
		weaponList.Remove(curWeapon);
	}

	//switch weapon
	public void SwitchWeapon(){
		if(settingWeapon){
			return;
		}
		if(curWeapon){
			int curWeaponIndex = weaponList.IndexOf(curWeapon);
			int nextWeaponIndex = (curWeaponIndex + 1) % weaponList.Count;

			curWeapon = weaponList[nextWeaponIndex];
		}else{
			curWeapon = weaponList[0];
		}

		settingWeapon = true;
		StartCoroutine(StopSettingWeapon());
	}

	IEnumerator StopSettingWeapon(){
		yield return new WaitForSeconds(0.7f);
		settingWeapon = false;
	}

	void OnAnimatorIK(int layerIndex) {
		if(!animator){
			return;
		}
		if(curWeapon && curWeapon.userSettings.leftHandIKTarget &&weaponType==2 && !reload && !settingWeapon){
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,1);
			Transform target = curWeapon.userSettings.leftHandIKTarget;
			Vector3 targetPos = target.position;
			Quaternion targetRot = target.rotation;
			animator.SetIKPosition(AvatarIKGoal.LeftHand,targetPos);
			animator.SetIKRotation(AvatarIKGoal.LeftHand,targetRot);
		}else{
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,0);
		}
	}
}
