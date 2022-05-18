using TMPro;
using UnityEngine;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private SkillID associatedSkill;
    private CombatManager manager;

    public void Init(SkillID id, SkillList masterSkillList, CombatManager manager) {
        this.associatedSkill = id;
        Skill skill = masterSkillList.Get(id);
        text.text = skill.name;
        this.manager = manager;
    }
    
    public void OnPress() {
        CombatUIController.Instance.menuState = CombatUIController.MenuState.TARGET;
        manager.SetTargetMode(associatedSkill);
        transform.parent.parent.gameObject.SetActive(false);
    }
}
