using UnityEngine;
using System.Collections;

/// <summary>
/// Megsemmisíti a KickAnim prefab-ot miután az animációnak vége.
/// Ez egy Animator Event.
/// </summary>

public class KickAnimController : MonoBehaviour {

	//Megsemmisíti önmagát
	public void Megsemmisit(){
		Destroy(this.gameObject);
	}

}
