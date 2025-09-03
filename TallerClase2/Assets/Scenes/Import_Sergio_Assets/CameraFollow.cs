using UnityEngine;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour
{
    [Header("Player and Offsets")]
    public Transform player;
    public Vector3 shoulderOffset = new Vector3(1f, 1f, -3f); // right shoulder

    [Header("Camera Rotation / Aim")]
    public float mouseSensitivity = 200f;
    public float pitchMin = -30f;
    public float pitchMax = 70f;

    [Header("Crosshair (UI)")]
    public Image crosshairImage; // assign in Inspector

    private float yaw;
    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairImage != null) crosshairImage.enabled = true;
    }

    private void OnDisable()
    {
        // If you ever disable this component, restore cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshairImage != null) crosshairImage.enabled = false;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Mouse input (aim)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Apply rotation & shoulder offset
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = player.position + rotation * shoulderOffset;

        transform.position = targetPos;
        transform.rotation = rotation;
    }
}
