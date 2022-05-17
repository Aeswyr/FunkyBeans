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

    [SerializeField] private Image hpBar;
    [SerializeField] private Image mpBar;

    public void SetHP(int val, int max) {
        hp.text = val.ToString();
        hpBar.fillAmount = ((float)val) / max;
    }

    public void SetMP(int val, int max) {
        mp.text = val.ToString();
        mpBar.fillAmount = ((float)val) / max;
    }

    public void SetNametag(string name) {
        nametag.text = name;
    }

}
