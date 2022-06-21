using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nametag;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI mp;
    [SerializeField] private TextMeshProUGUI ar;
    [SerializeField] private TextMeshProUGUI ev;

    [SerializeField] private Image hpBar;
    [SerializeField] private Image mpBar;

    [SerializeField] private GameObject armorDisp;
    [SerializeField] private GameObject evasionDisp;

    public void SetHP(int val, int max) {
        hp.text = val.ToString();
        if (max > 0)
            hpBar.fillAmount = ((float)val) / max;
        else 
            hpBar.fillAmount = 1;
    }

    public void SetMP(int val, int max) {
        mp.text = val.ToString();
        if (max > 0)
            mpBar.fillAmount = ((float)val) / max;
        else
            mpBar.fillAmount = 1;
    }

    public void SetArmor(int val) {
        armorDisp.SetActive(val > 0);
        ar.text = val.ToString();
    }
    
    public void SetEvasion(int val) {
        evasionDisp.SetActive(val > 0);
        ev.text = val.ToString();
    }

    public void SetNametag(string name) {
        nametag.text = name;
    }

}
