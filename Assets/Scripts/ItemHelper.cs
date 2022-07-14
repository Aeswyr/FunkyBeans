using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHelper : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    public EquipmentItem GenerateItem() {
        long id = (long) (UnityEngine.Random.value * long.MaxValue);
        return new EquipmentItem(
            ItemType.weapon,
            "Default Item",
            0,
            new Stats(),
            new List<SkillID>(),
            id
        );
    }

    public Sprite GetSprite(int id) {
        return sprites[id];
    }
}
