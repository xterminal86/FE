using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AutolevelTest : MonoBehaviour 
{
  public GameObject UnitPrefab;

  public TMP_Text Text;

  void Awake()
  {
    var go = (GameObject)Instantiate(UnitPrefab);

    var ub = go.GetComponent<Unit>();

    Text.text = ub.UnitData_.Details;
  }
}
