using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotMachine3D : MonoBehaviour
{
    [Header("Slot Machine Setup")]
    public Reel3D[] reels;       // 3 reels in the scene
    public Button spinButton;
    public Text resultText;

    [Header("Spin Timing")]
    public float spinDuration = 2.5f;      // Total spin time before stopping
    public float delayBetweenStops = 0.7f; // Delay between each reel stop

    [Header("Sound Effects")]
    public AudioSource audioSource;     // Single shared audio source
    public AudioClip spinLoopSound;     // Continuous spinning sound

    [Header("Payout Settings")]
    public int baseBet = 10;                 // Base bet per spin
    public int[] payoutMultipliers = new int[] { 2, 3, 5, 10 }; 
    // Index-based multipliers for triple matches

    private bool canSpin = true;
    private bool isSoundPlaying = false;

    private int totalBalance = 1000; // Starting credits for player

    void Start()
    {
        if (spinButton)
            spinButton.onClick.AddListener(OnSpinClicked);

        if (resultText)
            resultText.text = $"Balance: {totalBalance}";
    }

    void Update()
    {
        HandleSpinSound();
    }

    void HandleSpinSound()
    {
        bool anySpinning = false;

        foreach (var reel in reels)
        {
            if (reel != null && reel.IsSpinning())
            {
                anySpinning = true;
                break;
            }
        }

        if (anySpinning && !isSoundPlaying)
            PlaySpinSound();
        else if (!anySpinning && isSoundPlaying)
            StopSpinSound();
    }

    void PlaySpinSound()
    {
        if (audioSource && spinLoopSound)
        {
            audioSource.clip = spinLoopSound;
            audioSource.loop = true;
            audioSource.Play();
            isSoundPlaying = true;
        }
    }

    void StopSpinSound()
    {
        if (audioSource)
        {
            audioSource.Stop();
            isSoundPlaying = false;
        }
    }

    void OnSpinClicked()
    {
        if (!canSpin) return;
        if (totalBalance < baseBet)
        {
            resultText.text = "❌ Not enough balance!";
            return;
        }

        canSpin = false;
        totalBalance -= baseBet;
        resultText.text = $"Spinning...\nBalance: {totalBalance}";

        StartCoroutine(SpinAllReels());
    }

    IEnumerator SpinAllReels()
    {
        // Step 1: Randomize target symbol for each reel
        foreach (var reel in reels)
            reel.ChooseRandomSymbol();

        // Step 2: Start spinning all reels
        foreach (var reel in reels)
            reel.StartSpin();

        // Step 3: Wait total duration before stopping
        yield return new WaitForSeconds(spinDuration);

        // Step 4: Stop each reel sequentially
        for (int i = 0; i < reels.Length; i++)
        {
            yield return new WaitForSeconds(delayBetweenStops);
            reels[i].StopSpin();
        }

        // Step 5: Wait for all reels to stop
        yield return new WaitUntil(() => AllReelsStopped());

        // Step 6: Get final reel results
        int[] results = new int[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            results[i] = reels[i] ? reels[i].GetCenterSymbolID() : -1;
        }

        // Step 7: Calculate payout
        int payout = CalculatePayout(results, out string rewardReason);

        totalBalance += payout;

        // Step 8: Display results
        resultText.text =
            $"Results: {results[0]} | {results[1]} | {results[2]}\n" +
            $"➡️ {rewardReason}\n" +
            $"Payout: {payout}\n" +
            $"💰 Balance: {totalBalance}";

        canSpin = true;
    }

    bool AllReelsStopped()
    {
        foreach (var reel in reels)
        {
            if (reel != null && reel.IsSpinning())
                return false;
        }
        return true;
    }

    int CalculatePayout(int[] results, out string reason)
    {
        reason = "No match.";
        if (results.Length < 3) return 0;

        int a = results[0], b = results[1], c = results[2];

        // ✅ Triple Match
        if (a == b && b == c && a >= 0)
        {
            int multiplier = GetMultiplierForID(a);
            reason = $"Triple match! ID {a} → x{multiplier}";
            return baseBet * multiplier;
        }

        // ✅ Double Match
        if (a == b || b == c || a == c)
        {
            reason = "Pair matched — small win!";
            return Mathf.RoundToInt(baseBet * 0.5f);
        }

        // ❌ No Match
        return 0;
    }

    int GetMultiplierForID(int id)
    {
        if (payoutMultipliers != null && id >= 0 && id < payoutMultipliers.Length)
            return payoutMultipliers[id];
        return 1;
    }
}
