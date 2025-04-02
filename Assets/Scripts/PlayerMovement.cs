using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Fields for player movement
    public float playerSpeed = 14f;
    public float movementDistance = 2f;
    public float jumpForce = 6f;
    
    //Fields for our lane system of movement
    private int currentLane = 1; // 0 = left (-2), 1 = middle (0), 2 = right (2)
    private float[] lanePositions = { -2f, 0f, 2f }; // Explicit lane X positions
    private Vector3 targetPosition;
    public float laneSwitchSpeed = 10f; // Speed of lane transition
    
    //Fields for the jumping of our player
    private Rigidbody rb;
    private bool isGrounded = true;
    
    //Effect variables 
    private float normalSpeed; // Store original speed
    private float slowTimer; // Timer for slow effect
    private bool isSlowed; // Track slow state

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Set to kinematic to avoid physics interference with Translate
        targetPosition = transform.position = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        normalSpeed = playerSpeed; // Initialize normal speed
    }

    // Update is called once per frame
    void Update()
    {
        // Move forward continuously
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);

        // Lane switching
        //When A is pressed
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
        {
            currentLane--;
            UpdateTargetPosition();
        }
        //When D is pressed
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2)
        {
            currentLane++;
            UpdateTargetPosition();
        }

        // Smoothly move to target position
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneSwitchSpeed);
        transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);

        // Jumping when space is pressde
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.isKinematic = false; // Temporarily disable kinematic for jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
        // Handle slow timer
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                playerSpeed = normalSpeed;
                isSlowed = false;
                Debug.Log("Player speed restored to: " + playerSpeed);
            }
        }
    }
    //Update position method 
    private void UpdateTargetPosition()
    {
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }
    //Ground collision 
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
            
    }
    
    public void ApplyBerryEffect(float slowMultiplier, float duration)
    {
        if (!isSlowed) // Prevent stacking slow effects
        {
            normalSpeed = playerSpeed; // Store current speed before slowing
            playerSpeed *= slowMultiplier;
            slowTimer = duration;
            isSlowed = true;
            Debug.Log("Player slowed to: " + playerSpeed);
        }
    }
    

}
