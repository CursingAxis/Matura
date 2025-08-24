using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Soll der Checkpoint im Spiel sichtbar bleiben?")]
    public bool isVisible = true;

    private void Start()
    {
        if (!isVisible)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NeoMovement player = other.GetComponent<NeoMovement>();
        if (player != null)
        {
            player.respawnPoint = transform;
            Debug.Log("Checkpoint aktiviert: " + gameObject.name);
        }
    }
}
