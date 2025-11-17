using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;           // for HP
    public PriestAttackSystem priestAttack;   // for charge

    [Header("Health Crosses (left -> right)")]
    public Image cross1;
    public Image cross2;
    public Image cross3;

    [Header("Power Icons (left -> right)")]
    public Image[] powerIcons;           // set to size 4 in Inspector
    public Sprite normalPowerSprite;     // blue ball
    public Sprite fullPowerSprite;       // shiny ball

    void Awake()
    {
        // Auto-hook if left empty
        if (!player) player       = FindObjectOfType<PlayerController>();
        if (!priestAttack) priestAttack = FindObjectOfType<PriestAttackSystem>();

        // Start with all power icons hidden
        if (powerIcons != null)
        {
            foreach (var img in powerIcons)
                if (img) img.enabled = false;
        }
    }

    void Update()
    {
        if (player != null) UpdateHealth();
        if (priestAttack != null) UpdatePowerIcons();
    }

    void UpdateHealth()
    {
        int hp = player.Health;

        cross3.enabled = hp > 66;
        cross2.enabled = hp > 33;
        cross1.enabled = hp > 0;
    }

    void UpdatePowerIcons()
    {
        if (powerIcons == null || powerIcons.Length == 0) return;

        float current = priestAttack.CurrentCharge;
        float max = priestAttack.MaxCharge;

        // No charge or invalid max hide all
        if (max <= 0f || current <= 0f)
        {
            foreach (var img in powerIcons)
                if (img) img.enabled = false;
            return;
        }

        // 0..1
        float t = Mathf.Clamp01(current / max);

        int total = powerIcons.Length;           // 4
        int filled = Mathf.Clamp(Mathf.CeilToInt(t * total), 0, total);
        bool full = priestAttack.IsChargeFull;

        // If not full: show 1..4 normal icons
        if (!full)
        {
            for (int i = 0; i < total; i++)
            {
                var img = powerIcons[i];
                if (!img) continue;

                if (i < filled)
                {
                    img.enabled = true;
                    if (normalPowerSprite) img.sprite = normalPowerSprite;
                }
                else
                {
                    img.enabled = false;
                }
            }
        }
        else
        {
            // Full: all visible + shiny
            for (int i = 0; i < total; i++)
            {
                var img = powerIcons[i];
                if (!img) continue;

                img.enabled = true;
                if (fullPowerSprite) img.sprite = fullPowerSprite;
            }
        }
    }
}

