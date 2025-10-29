using UnityEngine;

public class testPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 720f; // degrees per second

    void Update()
    {
        // Get WASD input
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Direction relative to world
        Vector3 direction = new Vector3(h, 0f, v).normalized;

        // Move and rotate if there's input
        if (direction.magnitude >= 0.1f)
        {
            // Calculate target angle and smooth rotate
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.deltaTime);

            // Move forward
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
}
