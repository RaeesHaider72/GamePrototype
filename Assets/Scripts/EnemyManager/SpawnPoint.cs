using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Matches WaveEntry.spawnGroupTag — empty means usable by any wave")]
    public string groupTag = "";

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position,
            transform.position + transform.forward * 0.8f);
    }
}