using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrajectoryLineScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerTossGrowth playerTossGrowth;
    [SerializeField] private Transform grainSpawnPoint;

    [Header("Trajectory Line Smoothness/Length")]
    [SerializeField] private int numSegments = 50;
    [SerializeField] private float curveLength = 3.5f;
    
    private Vector2[] segments;
    private LineRenderer lineRenderer;
    private float projectileSpeed;

    private const float TIME_CURVE_ADDITION = 0.5f;

    private void Start() {
        segments = new Vector2[numSegments];

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numSegments;

        projectileSpeed = playerTossGrowth.cornTossForce;

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

        colorKey[0].color = Color.white;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.white;
        colorKey[1].time = 1.0f;

        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 0.2f;

        gradient.SetKeys(colorKey, alphaKey);
        lineRenderer.colorGradient = gradient;
    }

    private void Update() {
        if (playerTossGrowth.isTossingCornstalk || playerTossGrowth.isThrowingPopcorn) {
            lineRenderer.positionCount = numSegments;
            
            if(playerTossGrowth.isThrowingPopcorn) projectileSpeed = playerTossGrowth.popcornTossForce;
            if(playerTossGrowth.isTossingCornstalk) projectileSpeed = playerTossGrowth.cornTossForce;

            Vector2 startPos = grainSpawnPoint.position;
            segments[0] = startPos;
            lineRenderer.SetPosition(0, startPos);

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePosition - grainSpawnPoint.position).normalized;
            Vector2 startVelocity = direction * projectileSpeed;

            for(int i = 1; i < numSegments; i++) {
                float timeOffset = (i * Time.fixedDeltaTime * curveLength);
                Vector2 gravityOffset = TIME_CURVE_ADDITION * Physics2D.gravity * Mathf.Pow(timeOffset, 2);

                segments[i] = segments[0] + startVelocity * timeOffset + gravityOffset;
                lineRenderer.SetPosition(i, segments[i]);
            }

        } else {
            lineRenderer.positionCount = 0;
        }
    }
}
