using System.Collections.Generic;
using UnityEngine;

public class TreasurePortalManager : MonoBehaviour
{
    public static TreasurePortalManager Instance { get; private set; }

    private List<GameObject> treasures = new List<GameObject>();
    private List<GameObject> portals = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        HideAll();
    }

    private void HideAll()
    {
        treasures.Clear();
        portals.Clear();

        foreach (var go in GameObject.FindGameObjectsWithTag("Tesoro"))
        {
            treasures.Add(go);
            SetHidden(go, true);
        }

        foreach (var go in GameObject.FindGameObjectsWithTag("Portal"))
        {
            portals.Add(go);
            SetHidden(go, true);
        }
    }

    private void SetHidden(GameObject go, bool hidden)
    {
        // Desactivar renderers (SpriteRenderer, MeshRenderer, CanvasRenderer, etc.)
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            r.enabled = !hidden;

        // Desactivar animators para que la animación no sea visible
        foreach (var a in go.GetComponentsInChildren<Animator>(true))
            a.enabled = !hidden;

        // Desactivar colisiones para que no interactúen
        foreach (var c2 in go.GetComponentsInChildren<Collider2D>(true))
            c2.enabled = !hidden;
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            c.enabled = !hidden;
    }

    public void RevealAll()
    {
        foreach (var go in treasures) SetHidden(go, false);
        foreach (var go in portals) SetHidden(go, false);
    }
}