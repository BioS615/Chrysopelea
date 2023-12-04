using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerAircraftController : MonoBehaviour
{
    [Header("Controls")]
    public InputActionAsset inputAsset;
    private InputAction accelerate;
    private InputAction decelerate;
    private InputAction pitchControl;
    private InputAction rollControl;

    [Header("Movement Settings")]
    public float pitchSpeed = 2f;
    public float rollSpeed = 3f;
    public float maxSpeed = 10f;
    public float deltaVRampupTime = 1f;

    public float acceleration = 1f; // These two need to be combined into one
    public float deceleration = 1f; // and use that for velocity delta

    public float deltaV = 0f;
    

    private float currentSpeed = 0f;
    private bool isAccelerating = false;
    private bool isDecelerating = false;
    private bool isRolling = false;
    private bool isPitch = false;
    private Vector3 previousPosition; // Store the previous position
    private float elapsedTime = 0f;

    [Header("UI")]
    public TextMeshProUGUI speedometerText; // Reference to the UI Text element
    public TextMeshProUGUI accelerationText;

    private void OnEnable()
    {
        InputActionMap aircraft = inputAsset.FindActionMap("Aircraft");

        accelerate = aircraft.FindAction("Accelerate");
        accelerate.Enable();
        accelerate.performed += _ => BeginAcceleration();
        accelerate.canceled += _ => StopAcceleration();

        decelerate = aircraft.FindAction("Decelerate");
        decelerate.Enable();
        decelerate.performed += _ => BeginDeceleration();
        decelerate.canceled += _ => StopDeceleration();

        rollControl = aircraft.FindAction("Roll");
        rollControl.Enable();
        rollControl.started += _ => StartRolling();
        rollControl.performed += ctx => HandleRollInput(ctx.ReadValue<float>());
        rollControl.canceled += _ => StopRolling();

        pitchControl = aircraft.FindAction("Pitch");
        pitchControl.Enable();
        pitchControl.started += _ => StartPitch();
        pitchControl.performed += ctx => HandlePitchInput(ctx.ReadValue<float>());
        pitchControl.canceled += _ => StopPitch();
    }

    private void OnDisable()
    {
        accelerate.Disable();
        decelerate.Disable();
        pitchControl.Disable();
        rollControl.Disable();
    }

    private void Update()
    {
        HandleMovement();
        if (isAccelerating)
        {
            Accelerate();
        }

        if (isDecelerating)
        {
            Decelerate();
        }

        if (isRolling)
        {
            HandleRollInput(rollControl.ReadValue<float>());
        }

        if (isPitch)
        {
            HandlePitchInput(pitchControl.ReadValue<float>());
        }

        if (!isAccelerating && !isDecelerating && deltaV > 0)
        {
            elapsedTime += Time.deltaTime;
            deltaV = Mathf.Lerp(deltaV, 0, elapsedTime / deltaVRampupTime);
        }

        CalculateAndDisplaySpeed();
    }

    private void CalculateAndDisplaySpeed()
    {
        float speed = CalculateSpeed();
        speedometerText.text = speed.ToString("F1") + " m/s";
        accelerationText.text = deltaV.ToString("F1");
    }

    private float CalculateSpeed()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, previousPosition);
        float speed = distance / Time.deltaTime;
        previousPosition = currentPosition;
        return speed;
    }

    private void BeginAcceleration()
    {
        elapsedTime = 0;
        isAccelerating = true;
    }

    private void Accelerate()
    {
        elapsedTime += Time.deltaTime;
        deltaV = Mathf.Lerp(deltaV, acceleration, elapsedTime / deltaVRampupTime);
        currentSpeed = Mathf.Clamp(currentSpeed + deltaV * Time.deltaTime, 0f, maxSpeed);
    }

    private void StopAcceleration()
    {
        elapsedTime = 0;
        isAccelerating = false;
    }

    private void BeginDeceleration()
    {
        elapsedTime = 0;
        isDecelerating = true;
    }

    private void Decelerate()
    {
        elapsedTime += Time.deltaTime;
        deltaV = Mathf.Lerp(deltaV, acceleration, elapsedTime / deltaVRampupTime);
        currentSpeed = Mathf.Clamp(currentSpeed - deltaV * Time.deltaTime, 0f, maxSpeed);
    }

    private void StopDeceleration()
    {
        elapsedTime = 0;
        isDecelerating = false;
    }

    private void HandleMovement()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    private void StartRolling()
    {
        isRolling = true;
    }

    private void StopRolling()
    {
        isRolling = false;
    }

    private void HandleRollInput(float rollInput)
    {
        float roll = rollInput * rollSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, roll);
    }

    private void StartPitch()
    {
        isPitch = true;
    }

    private void StopPitch()
    {
        isPitch = false;
    }

    private void HandlePitchInput(float pitchInput)
    {
        float pitch = pitchInput * pitchSpeed * Time.deltaTime;
        transform.Rotate(pitch, 0f, 0f);
    }

}
