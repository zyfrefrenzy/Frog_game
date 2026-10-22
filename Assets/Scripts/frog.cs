using System.Collections;
using UnityEngine;

public class frog : MonoBehaviour
{
    public Transform frogVisual;   // the frog image child
    public float riseHeight = 1.2f; // how far down the frog hides (make it taller than the frog)
    public float moveSpeed = 8f;    // higher = faster pop up/down
    public AudioClip hitSound;      // splat/pop sound
    public GameManager gameManager;   // drag the GameManager object here

    public bool IsUp { get; private set; }

    Vector3 shownPos, hiddenPos, startScale;
    Collider2D col;
    SpriteRenderer sr;
    Coroutine routine;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = frogVisual.GetComponent<SpriteRenderer>();

        // the position you placed the frog at in the scene = "up" position
        shownPos = frogVisual.localPosition;
        hiddenPos = shownPos - new Vector3(0, riseHeight, 0);
        startScale = frogVisual.localScale;

        ResetFrog();
    }

    // Called by GameManager to make the frog pop out
    public void Pop(float stayTime)
    {
        if (IsUp) return;
        routine = StartCoroutine(PopRoutine(stayTime));
    }

    IEnumerator PopRoutine(float stayTime)
    {
        IsUp = true;
        col.enabled = true;                          // can be clicked now
        yield return Move(hiddenPos, shownPos);      // rise
        yield return new WaitForSeconds(stayTime);   // wait for player to hit it

        // player was too slow
        col.enabled = false;
        yield return Move(shownPos, hiddenPos);      // hide again
        IsUp = false;
        GameManager.Instance.FrogMissed();           // lose a life
    }

    // Called by GameManager when the player clicks this frog
    public void Hit()
    {
        if (!IsUp) return;
        StopCoroutine(routine);
        col.enabled = false;

        GameManager.Instance.FrogKilled();
        if (hitSound) AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);

        routine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        // quick "splat": flash red and squash, then drop back into the hole
        sr.color = Color.red;
        frogVisual.localScale = new Vector3(startScale.x * 1.2f, startScale.y * 0.6f, startScale.z);
        yield return new WaitForSeconds(0.15f);

        sr.color = Color.white;
        frogVisual.localScale = startScale;
        yield return Move(frogVisual.localPosition, hiddenPos);
        IsUp = false;
    }

    // Smoothly slides the frog between two positions
    IEnumerator Move(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            frogVisual.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        frogVisual.localPosition = to;
    }

    // Puts the frog back to hidden (used on game start/end)
    public void ResetFrog()
    {
        StopAllCoroutines();
        frogVisual.localPosition = hiddenPos;
        frogVisual.localScale = startScale;
        sr.color = Color.white;
        col.enabled = false;
        IsUp = false;
    }
}