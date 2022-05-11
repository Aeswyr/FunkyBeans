using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : Singleton<GameHandler>
{
    [SerializeField] private Grid currentLevel;
    public void SnapToLevelGrid(GameObject entity) {
        entity.transform.position = currentLevel.CellToWorld(currentLevel.WorldToCell(entity.transform.position));
    }
}
