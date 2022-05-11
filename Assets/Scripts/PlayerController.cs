using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private InputHandler input;
    [SerializeField] private float speed;
    private ContactFilter2D filter = new ContactFilter2D();
    private bool freeMove  = true;
    // Start is called before the first frame update
    void Start()
    {
        filter.SetLayerMask(LayerMask.GetMask(new []{"Hurtbox"}));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (freeMove)
            rbody.velocity = speed * input.dir;
    }



    public void StartBattle() {

        var results = new List<RaycastHit2D>();
        Physics2D.CircleCast(transform.position, 4f, Vector2.right, filter, results, 0);
        Debug.Log($"castin: {results.Count}");

        foreach (var hit in results) {
            GameHandler.Instance.SnapToLevelGrid(hit.collider.transform.parent.gameObject);
        }
        GameHandler.Instance.SnapToLevelGrid(gameObject);

        freeMove = false;
        rbody.velocity = Vector2.zero;
    }
}
