using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to the Panel GameObject (the one that gets dragged). It must also have
/// a Graphic component (e.g. Image, can be transparent) with "Raycast Target" ON so it
/// receives drag events - Unity's drag interfaces need something raycastable to hit.
///
/// Behaviour (Hill Climb Racing style):
///  - Drag the panel -> it follows your finger/mouse 1:1
///  - Release -> it keeps moving with momentum (inertia), gradually slowing down (Damping)
///  - Once it's slow enough, it automatically snaps so the nearest button lands on 'center'
///  - Tapping any button also snaps it directly to center (original behaviour kept)
/// </summary>
public class WorldCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Original References")]
    public RectTransform panel;       // The Scroll Panel (this object, or the one being moved)
    public Button[] bttn;
    public RectTransform center;      // Fixed point cards should snap toward

    [Header("Inertia / Snap Settings")]
    public float damping = 4f;        // higher = stops sooner after release
    public float snapSpeed = 10f;     // higher = snaps into place faster
    public float snapVelocityThreshold = 40f; // below this speed (px/sec), start snapping instead of coasting

    [Header("Optional Focus Scaling")]
    public bool enableFocusScaling = false;
    public float focusedScale = 1.00f;
    public float unfocusedScale = 0.02f;

    // Distance tracking (kept from your original script)
    private float[] distance;
    private int minButtonNum;

    // Drag / inertia state
    private bool dragging = false;
    private float velocityX = 0f;      // current horizontal speed, px/sec
    private Vector2 lastPointerPos;
    private bool snapping = false;
    private Vector2 targetPosition;

    void Start()
    {
        distance = new float[bttn.Length];
    }

    void Update()
    {
        UpdateDistances();

        if (dragging)
            return; // panel position is driven directly by OnDrag while dragging

        if (Mathf.Abs(velocityX) > snapVelocityThreshold)
        {
            // --- Inertia phase: coast and decay, same idea as SimulateInertia() ---
            panel.anchoredPosition += new Vector2(velocityX * Time.deltaTime, 0f);
            velocityX -= Mathf.Sign(velocityX) * damping * 100f * Time.deltaTime;

            // Once it decays enough, hand off to the snap phase
            if (Mathf.Abs(velocityX) <= snapVelocityThreshold)
            {
                velocityX = 0f;
                SnapToButton(minButtonNum);
            }
        }
        else if (snapping)
        {
            // --- Snap phase: smoothly glide the closest card onto 'center' ---
            panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            if (Vector2.Distance(panel.anchoredPosition, targetPosition) < 0.5f)
            {
                panel.anchoredPosition = targetPosition;
                snapping = false;
            }
        }

        if (enableFocusScaling)
            UpdateFocusScaling();
    }

    // ----- Distance tracking (from your original script) -----
    private void UpdateDistances()
    {
        float smallestDistance = float.MaxValue;
        for (int i = 0; i < bttn.Length; i++)
        {
            distance[i] = Mathf.Abs(center.position.x - bttn[i].transform.position.x);
            if (distance[i] < smallestDistance)
            {
                smallestDistance = distance[i];
                minButtonNum = i;
            }
        }
    }

    // ----- Drag handlers (replace ScrollRect's own dragging) -----
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        snapping = false;
        velocityX = 0f;
        lastPointerPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPointerPos;
        panel.anchoredPosition += new Vector2(delta.x, 0f);

        // Track instantaneous speed so we can carry it into inertia on release
        velocityX = delta.x / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPointerPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        // velocityX already holds the last frame's drag speed - inertia picks up in Update()
    }

    // ----- Snap logic (kept from your original script) -----
    private void SnapToButton(int index)
    {
        float offsetX = center.position.x - bttn[index].transform.position.x;
        targetPosition = panel.anchoredPosition + new Vector2(offsetX, 0f);
        snapping = true;
    }

    /// <summary>
    /// Wire this to each button's OnClick to let tapping a card snap it to center directly.
    /// </summary>
    public void OnButtonTapped(int index)
    {
        velocityX = 0f;
        SnapToButton(index);
    }

    // ----- Optional: scale the centered card up, others down (Disney-carousel look) -----
    private void UpdateFocusScaling()
    {
        for (int i = 0; i < bttn.Length; i++)
        {
            float t = Mathf.Clamp01(distance[i] / (Screen.width * 0.5f));
            float scale = Mathf.Lerp(focusedScale, unfocusedScale, t);
            bttn[i].transform.localScale = Vector3.one * scale;
        }
    }
}