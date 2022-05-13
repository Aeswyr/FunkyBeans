using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameHandler : Singleton<GameHandler>
{
    [Header("Gameplay grids")]
    [SerializeField] private Grid m_currentLevel;
    public Tilemap floorGrid {
        get;
        private set;
    }
    public Tilemap wallGrid {
        get;
        private set;
    }
    public Grid currentLevel {
        get {return m_currentLevel;}
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject combatManagerPrefab;
    [SerializeField] private GameObject textPrefab;


    void Start() {
        floorGrid = m_currentLevel.transform.Find("Collision").GetComponent<Tilemap>();
        wallGrid = m_currentLevel.transform.Find("Walls").GetComponent<Tilemap>();
    }

    List<GameObject> textList = new List<GameObject>();
    public void DrawText(Vector3 pos, string text) {
        GameObject txt = Instantiate(textPrefab, pos, Quaternion.identity);
        txt.GetComponent<TextMeshPro>().text = text;
        textList.Add(txt);
    }

    public void ClearText() {
        foreach (var txt in textList)
            Destroy(txt);
        textList.Clear();
    }

    public CombatManager CreateCombatManager() {
        return Instantiate(combatManagerPrefab, Vector3.zero, Quaternion.identity).GetComponent<CombatManager>();
    }

}
