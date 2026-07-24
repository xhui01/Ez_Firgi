using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Drag-to-snap carousel with Hill Climb Racing-style momentum:
/// - Drag/flick the panel, it keeps gliding after release (inertia)
/// - Friction slows it down, then it eases into a snap on the nearest card
/// - Centered card scales up, side cards scale/fade down (focus effect)
/// Works with Unity UI (RectTransform + EventSystem), not world-space sprites.
/// </summary>
public class WorldCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ---- Original public fields (kept as-is) ----
    public RectTransform panel;   // the Scroll Panel holding all the cards
    public Button[] bttn;
    public RectTransform center;  // where a card should land when "focused"

    [Header("Snap settings")]
    public float snapSpeed = 10f;
    public float snapThreshold = 0.5f;

    [Header("Momentum / Inertia (Hill Climb Racing feel)")]
    [Tooltip("How quickly the flick speed decays per second. Higher = stops sooner.")]
    public float damping = 4f;
    [Tooltip("Below this speed (px/sec), inertia ends and snapping begins.")]
    public float inertiaStopThreshold = 40f;

    [Header("Focus scale/fade effect")]
    public float focusedScale = 1.15f;
    public float unfocusedScale = 0.85f;
    [Tooltip("Optional: fades side cards out. Needs a CanvasGroup on each card.")]
    public bool fadeUnfocusedCards = true;
    public float minAlpha = 0.5f;

    // ---- Private state ----
    private float[] distance;
    private int minButtonNum;

    private bool isDragging = false;
    private bool isInertia = false;
    private bool isSnapping = false;

    private float velocityX = 0f;      // current momentum, px/sec
    private Vector2 lastPointerPos;

    private CanvasGroup[] cardGroups;

    void Start()
    {
        int bttnLength = bttn.Length;
        distance = new float[bttnLength];
        cardGroups = new CanvasGroup[bttnLength];

        for (int i = 0; i < bttnLength; i++)
        {
            int index = i;
            bttn[i].onClick.AddListener(() => OnWorldButtonClicked(index));

            // Cache/create a CanvasGroup per card so we can fade unfocused ones
            if (fadeUnfocusedCards)
            {
                var cg = bttn[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = bttn[i].gameObject.AddComponent<CanvasGroup>();
                cardGroups[i] = cg;
            }
        }
    }

    void Update()
    {
        UpdateDistances();
        minButtonNum = GetNearestButtonIndex();

        if (isInertia)
        {
            ApplyInertia();
        }
        else if (isSnapping)
        {
            ApplySnap();
        }

        UpdateCardVisuals();
    }

    private void UpdateDistances()
    {
        for (int i = 0; i < bttn.Length; i++)
        {
            distance[i] = Mathf.Abs(center.position.x - bttn[i].transform.position.x);
        }
    }

    private int GetNearestButtonIndex()
    {
        int nearest = 0;
        float smallest = float.MaxValue;
        for (int i = 0; i < distance.Length; i++)
        {
            if (distance[i] < smallest)
            {
                smallest = distance[i];
                nearest = i;
            }
        }
        return nearest;
    }

    // ---------------- Drag handlers (replaces Input.GetMouseButton from the found script) ----------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isInertia = false;
        isSnapping = false;
        velocityX = 0f;
        lastPointerPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the panel 1:1 with the finger/mouse
        panel.anchoredPosition += new Vector2(eventData.delta.x, 0);

        // Track speed for momentum once released (px/sec)
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        velocityX = eventData.delta.x / dt;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (Mathf.Abs(velocityX) > inertiaStopThreshold)
        {
            isInertia = true;   // let it glide, like flicking a Hill Climb Racing menu
        }
        else
        {
            isInertia = false;
            isSnapping = true;  // barely moved - snap immediately
        }
    }

    // ---------------- Inertia (equivalent to SimulateInertia() in the found script) ----------------

    private void ApplyInertia()
    {
        // Friction: reduce speed magnitude over time, keep direction
        float frictionThisFrame = damping * 100f * Time.deltaTime; // *100 so 'damping' values feel similar in scale to Inspector-friendly numbers
        if (velocityX > 0)
            velocityX = Mathf.Max(0, velocityX - frictionThisFrame);
        else
            velocityX = Mathf.Min(0, velocityX + frictionThisFrame);

        panel.anchoredPosition += new Vector2(velocityX * Time.deltaTime, 0);

        if (Mathf.Abs(velocityX) <= inertiaStopThreshold)
        {
            isInertia = false;
            isSnapping = true; // hand off to snapping, same as found script's SnapInitialize()
        }
    }

    // ---------------- Snap (equivalent to Snap()/SnapInitialize() in the found script) ----------------

    private void ApplySnap()
    {
        RectTransform target = bttn[minButtonNum].GetComponent<RectTransform>();
        float offset = center.position.x - target.position.x;
        Vector2 targetPos = panel.anchoredPosition + new Vector2(offset, 0);

        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * snapSpeed);

        if (Vector2.Distance(panel.anchoredPosition, targetPos) < snapThreshold)
        {
            panel.anchoredPosition = targetPos;
            isSnapping = false;
        }
    }

    // ---------------- Focus scale/fade (equivalent to UpdateSlideScale() in the found script) ----------------

    private void UpdateCardVisuals()
    {
        float halfViewport = ((RectTransform)panel.parent).rect.width * 0.5f;
        if (halfViewport <= 0f) halfViewport = 500f; // safety fallback

        for (int i = 0; i < bttn.Length; i++)
        {
            float t = Mathf.Clamp01(distance[i] / halfViewport);
            float scale = Mathf.Lerp(focusedScale, unfocusedScale, t);
            bttn[i].transform.localScale = Vector3.one * scale;

            if (fadeUnfocusedCards && cardGroups[i] != null)
            {
                cardGroups[i].alpha = Mathf.Lerp(1f, minAlpha, t);
            }
        }
    }

    // ---------------- Button tap (kept from your original script) ----------------

    private void OnWorldButtonClicked(int index)
    {
        if (isDragging) return; // ignore taps mid-drag

        if (index == minButtonNum)
        {
            Debug.Log($"Load world at index {index}: {bttn[index].name}");
            // TODO: e.g. LevelManager.LoadWorld(index);
        }
        else
        {
            isInertia = false;
            isSnapping = false;
            minButtonNum = index; // force target, ApplySnap() will pick it up next frame
            isSnapping = true;
        }
    }
}