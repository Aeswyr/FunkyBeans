using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float maxTime;
    [SerializeField] private float startFade;
    float time;
    
    void Awake() {
        time = maxTime;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (time > 0)
            time -= Time.fixedDeltaTime;
        else
            Destroy(gameObject);
        Vector3 pos = this.transform.position;
        pos += 0.03f * time / maxTime * Vector3.up;
        this.transform.position = pos;

        if (time < startFade) {
            var c = text.color;
            c.a = time / startFade;
            text.color = c;
        }
    }
}
