using UnityEngine;
using System.Collections;

public class Reel3D : MonoBehaviour
{
    [Header("Reel Setup")]
    public int symbolsCount = 4; // number of icons on the cylinder (e.g. 4)

    [Header("Spin Settings")]
    public float spinSpeed = 720f;     // Degrees per second
    public float deceleration = 600f;  // Slow down rate

    private bool spinning = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private int targetSymbolIndex = 0;
    private Coroutine spinRoutine;

    // Fixed Y and Z rotation (kept exactly as in your script)
    private const float fixedY = -90f;
    private const float fixedZ = 45f;  // fixed Z rotation

    // Detect whether to rotate on X or Z
    private bool useXAxis = true;

    void Start()
    {
        // Detect which local axis aligns most with world right
        float dotX = Mathf.Abs(Vector3.Dot(transform.right, Vector3.right));
        float dotZ = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.right));
        useXAxis = dotX > dotZ;
    }

    // Choose a random symbol index (0..symbolsCount-1)
    public void ChooseRandomSymbol()
    {
        if (symbolsCount <= 0) symbolsCount = 4;
        targetSymbolIndex = Random.Range(0, symbolsCount);
        targetAngle = targetSymbolIndex * (360f / symbolsCount);
    }

    public void StartSpin()
    {
        if (spinRoutine != null)
            StopCoroutine(spinRoutine);

        spinRoutine = StartCoroutine(SpinRoutine());
    }

    public void StopSpin()
    {
        spinning = false;
    }

    IEnumerator SpinRoutine()
    {
        spinning = true;
        float speed = spinSpeed;
        currentAngle = 0f;

        while (true)
        {
            currentAngle += speed * Time.deltaTime;
            currentAngle %= 360f;

            // exactly as your script: rotate on detected axis, keep fixedY/fixedZ
            if (useXAxis)
            {
                transform.localEulerAngles = new Vector3(currentAngle, fixedY, fixedZ);
            }
            else
            {
                transform.localEulerAngles = new Vector3(0f, fixedY, fixedZ);
                transform.Rotate(0f, 0f, currentAngle, Space.Self);
            }

            if (!spinning)
            {
                speed = Mathf.MoveTowards(speed, 0, deceleration * Time.deltaTime);
                if (speed <= 5f)
                {
                    currentAngle = targetAngle;

                    if (useXAxis)
                        transform.localEulerAngles = new Vector3(currentAngle, fixedY, fixedZ);
                    else
                    {
                        transform.localEulerAngles = new Vector3(0f, fixedY, fixedZ);
                        transform.Rotate(0f, 0f, currentAngle, Space.Self);
                    }

                    yield break;
                }
            }

            yield return null;
        }
    }

    // Returns the symbol index currently in center (0..symbolsCount-1)
    public int GetCenterSymbolID()
    {
        if (symbolsCount <= 0) symbolsCount = 4;
        float normalizedAngle = (currentAngle % 360f + 360f) % 360f;
        int index = Mathf.RoundToInt(normalizedAngle / (360f / symbolsCount)) % symbolsCount;
        if (index < 0) index += symbolsCount;
        return index;
    }

    public bool IsSpinning()
    {
        return spinning;
    }
}
