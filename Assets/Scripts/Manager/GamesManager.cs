using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamesManager : MonoBehaviour {

	private static GamesManager GAMES_MANAGER;
	private PlayerUI playerUI {
		get{
			return FindObjectOfType<PlayerUI>();
		}set{
			playerUI = value;
		}
	}
	private InputController player {
		get{
			return FindObjectOfType<InputController>();
		}set{
			player = value;
		}
	}

	private WeaponHandler weaponHandler {
		get{
			return FindObjectOfType<WeaponHandler>();
		}set{
			weaponHandler = value;
		}
	}

	void Awake() {
		if(GAMES_MANAGER == null){
			GAMES_MANAGER = this;
		}else{
			if(GAMES_MANAGER !=this){
				Destroy(gameObject);
			}
		}
	}

	void Update() {
		UIUpdate();
	}

	void UIUpdate(){
		if(player){
			if(playerUI){
				if(weaponHandler){
					if(playerUI.ammoTxt){
						if(weaponHandler.curWeapon == null){
							playerUI.ammoTxt.text = "Unarmed";
						}else{
							playerUI.ammoTxt.text = weaponHandler.curWeapon.ammo.clipAmmo + "/"+weaponHandler.curWeapon.ammo.carryAmmo;
						}
					}	
				}
				if(playerUI.healthBar && playerUI.healthTxt){
					playerUI.healthTxt.text = Mathf.Round(playerUI.healthBar.value).ToString();
				}
			}
		}
	}
}
