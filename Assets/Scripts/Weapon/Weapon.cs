using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour {

	Collider coll;
	Rigidbody rigidBody;
	Animator animator;

	public enum WeaponType
	{
		Pistol,Rifle
	}

	public WeaponType weaponType;

	[System.Serializable]
	public class UserSettings{
		public Transform leftHandIKTarget;
		public Vector3 spineRotation;
	}
	[SerializeField]
	public UserSettings userSettings;
	
	[System.Serializable]
	public class WeaponSettings{
		[Header("-Bullet Options-")]
		public Transform bulletSpawn;
		public float damage = 5.0f;
		public float bulletSpread = 5.0f;
		public float fireRate = 0.2f;
		public LayerMask bulletLayer;
		public float range = 200.0f;

		[Header("-Effects-")]
		public GameObject muzzleFlash;
		public GameObject decal;
		public GameObject shell;
		public GameObject clip;

		[Header("-Other-")]
		public float reloadDuration = 2.0f;
		public Transform shellEjectSpot;
		public float shellEjectSpeed = 7.5f;
		public Transform clipEjectPos;
		public GameObject clipGO;

		[Header("-Positioning-")]
		public Vector3 equipPos;
		public Vector3 equipRot;
		public Vector3 unequipPos;
		public Vector3 unequipRot;

		[Header("-Animation-")]
		public bool useAnim;
		public int fireAnimLayer = 0;
		public string fireAnimName = " Fire";
	}
	[SerializeField]
	public WeaponSettings weaponSettings;

	[System.Serializable]
	public class Ammo{
		public int carryAmmo;
		public int clipAmmo;
		public int maxAmmo;
	}
	[SerializeField]
	public Ammo ammo;

	public Ray shootRay {protected get;set;}
	WeaponHandler handler;
	bool equipped;
	bool pullingTrigger;
	bool resettingCarridge;

	// Use this for initialization
	void Start () {
		coll = GetComponent<Collider>();
		rigidBody = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(handler){
			DisableEnableComponent(false);
			if(equipped){
				if(handler.userSettings.rightHand){
					Equip();
					if(pullingTrigger){
						Fire(shootRay);
					}
				}
			}else{
				Unequip(weaponType);
			}
		}else{
			DisableEnableComponent(true);
			transform.SetParent(null);
		}
	}

	//fire weapon
	void Fire(Ray ray){
		if(ammo.clipAmmo<=0||resettingCarridge||!weaponSettings.bulletSpawn){
			return;
		}
		RaycastHit hit;
		Transform bSpawn = weaponSettings.bulletSpawn;
		Vector3 bSpawnPoint = bSpawn.position;
		Vector3 dir = ray.GetPoint(weaponSettings.range);

		dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;

		if(Physics.Raycast(bSpawnPoint,dir,out hit,weaponSettings.range,weaponSettings.bulletLayer)){
			#region decal
			if(hit.collider.gameObject.isStatic){
				if(weaponSettings.decal){
					Vector3 hitPoint = hit.point;
					Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
					GameObject decal = Instantiate(weaponSettings.decal,hitPoint,lookRotation) as GameObject;
					Transform decalTransform = decal.transform;
					Transform hitTransform = hit.transform;
					decalTransform.SetParent(hitTransform);
					Destroy(decal, Random.Range(30.0f,45.0f));
				}
			}
			#endregion
		}
		#region muzzleFlash
		if(weaponSettings.muzzleFlash){
			Vector3 bulletSpawnPos = weaponSettings.bulletSpawn.position;
			GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash,bulletSpawnPos,Quaternion.identity) as GameObject;
			Transform muzzleTransform = muzzleFlash.transform;
			muzzleTransform.SetParent(weaponSettings.bulletSpawn);
			Destroy(muzzleFlash,1.0f);
		}
		#endregion

		#region shell
		if(weaponSettings.shell){
			if(weaponSettings.shellEjectSpot){
				Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
				Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
				GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot) as GameObject;

				if(shell.GetComponent<Rigidbody>()){
					Rigidbody rigidBodyShell = shell.GetComponent<Rigidbody>();
					rigidBodyShell.AddForce(weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed, ForceMode.Impulse);
				}
				Destroy(shell,Random.Range(30.0f,45.0f));
			}
		}
		#endregion

		if(weaponSettings.useAnim){
			animator.Play(weaponSettings.fireAnimName,weaponSettings.fireAnimLayer);
		}

		ammo.clipAmmo--;
		resettingCarridge = true;
		StartCoroutine(LoadNextBullet());
	}

//load next bullet
	IEnumerator LoadNextBullet(){
		yield return new WaitForSeconds(weaponSettings.fireRate);
		resettingCarridge = false;
	}

	//disable/enable collider n rigidbody
	void DisableEnableComponent(bool enable){
		if(!enable){
			rigidBody.isKinematic = true;
			coll.enabled = false;
		}else{
			rigidBody.isKinematic=false;
			coll.enabled = true;
		}
	}

	//equip this weapon to right hand
	void Equip(){
		if(!handler){
			return;
		}else if(!handler.userSettings.rightHand){
			return;
		}

		transform.SetParent(handler.userSettings.rightHand);
		transform.localPosition = weaponSettings.equipPos;
		Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRot);
		transform.localRotation = equipRot;
	}
	//unequip weapon and place to desire location
	void Unequip(WeaponType wptype){
		if(!handler){
			return;
		}
		switch(wptype){
			case WeaponType.Pistol:
				transform.SetParent(handler.userSettings.pistolUnequipSpot);
				break;
			case WeaponType.Rifle:
				transform.SetParent(handler.userSettings.rifleUnequipSpot);
				break;
		}

		transform.localPosition = weaponSettings.unequipPos;
		Quaternion unequipRot = Quaternion.Euler(weaponSettings.unequipRot);
		transform.localRotation = unequipRot;
	}

	//load clips and calculate ammo
	public void LoadClip(){
		int ammoNeeded = ammo.maxAmmo - ammo.clipAmmo;
		if(ammoNeeded >= ammo.carryAmmo){
			ammo.clipAmmo = ammo.carryAmmo;
			ammo.carryAmmo = 0;
		}else{
			ammo.carryAmmo -= ammoNeeded;
			ammo.clipAmmo = ammo.maxAmmo;
		}
	}

	//set weapons equip state
	public void SetEquipped(bool equip){
		equipped = equip;
	}

	//pull trigger
	public void PullTrigger(bool isPulling){
		pullingTrigger = isPulling;
	}

	//set owner weapon
	public void SetOwner(WeaponHandler weaponHandler){
		handler = weaponHandler;
	}
}
