using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 7f;
    public float gravity = -20f;
    public float rotationSpeed = 200f; // degrees per second

    [Header("Score Settings")]
    public int score = 0;

    [Header("UI settings")]
    public TMP_Text monedasText;

    private CharacterController cc;
    public Vector3 velocity;
    private PlataformaAB plataformaActual;

    // Mechanics
    [SerializeField] private RocketBooster booster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // --- Rotation (A/D) ---
        float rotateInput = Input.GetAxis("Horizontal"); // A=-1, D=1
        transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);

        // --- Forward/back (W/S) ---
        float moveInput = Input.GetAxis("Vertical");
        Vector3 horizontal = transform.forward * moveInput * speed;

        // --- Ground snap ---
        if (cc.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            if (booster) booster.NotifyGrounded(true);
        }
        else
        {
            if (booster) booster.NotifyGrounded(false);
        }

        // --- Jump ---
        if (Input.GetButtonDown("Jump") && cc.isGrounded)
        {
            velocity.y = jumpForce;
            plataformaActual = null;
        }

        // --- Gravity (scaled if boosting) ---
        float gravityScale = booster ? booster.CurrentGravityScale : 1f;
        velocity.y += gravity * gravityScale * Time.deltaTime;

        // --- Compose motion ---
        Vector3 motion = horizontal
                       + (booster ? booster.CurrentBoostVelocity : Vector3.zero)
                       + Vector3.up * velocity.y;

        cc.Move(motion * Time.deltaTime);

        // --- Moving platforms (optional) ---
        if (plataformaActual != null)
        {
            cc.Move(plataformaActual.delta);
        }


        // (Optional) If you want to re-check grounding post-move and clamp again:
        // if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Plataforma"))
        {
            plataformaActual = hit.collider.GetComponent<PlataformaAB>();
        }
    }
    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
        UpdateMonedas(score);
    }

    public void UpdateMonedas(int amount)
    {
        monedasText.text = "MONEDAS: " + amount.ToString();
    }
}

