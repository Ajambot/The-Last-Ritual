using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RitualAltarInteraction : MonoBehaviour
{
    public RitualTimer ritualTimer;             // drag your RitualTimer here
    public TextMeshProUGUI promptText;          // main text for instructions
    public TextMeshProUGUI sequenceText;        // optional: shows what was pressed

    [TextArea] public string startPrompt = "Press E to start the ritual";
    [TextArea] public string choicePrompt = "Press 1 = Candle, 2 = Salt, 3 = Book";
    [TextArea] public string successText = "The ritual begins...";
    [TextArea] public string failText = "The ritual failed. Press E to try again.";

    enum Offering { Candle, Salt, Book }

    Offering[] correctOrder = new Offering[]
    {
        Offering.Salt,
        Offering.Book,
        Offering.Candle
    };

    bool playerInZone = false;
    bool ritualStarted = false;
    bool ritualCompleted = false;

    List<Offering> enteredOrder = new List<Offering>();
    HashSet<Offering> usedOfferings = new HashSet<Offering>();

    void Start()
    {
        if (promptText != null) promptText.text = "";
        if (sequenceText != null) sequenceText.text = "";
    }

    void Update()
    {
        if (!playerInZone || ritualCompleted)
            return;

        if (!ritualStarted)
        {
            if (promptText != null)
                promptText.text = startPrompt;

            if (Input.GetKeyDown(KeyCode.E))
            {
                ritualStarted = true;
                enteredOrder.Clear();
                usedOfferings.Clear();

                if (promptText != null)
                    promptText.text = choicePrompt;

                if (sequenceText != null)
                    sequenceText.text = "Chosen: ";
            }
        }
        else
        {
            HandleOfferingInput();
        }
    }

    void HandleOfferingInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddOffering(Offering.Candle);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            AddOffering(Offering.Salt);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            AddOffering(Offering.Book);
    }

    void AddOffering(Offering offering)
    {
        // cannot press the same option again
        if (usedOfferings.Contains(offering))
            return;

        usedOfferings.Add(offering);
        enteredOrder.Add(offering);

        if (sequenceText != null)
            sequenceText.text = "Chosen: " + string.Join(" -> ", enteredOrder);

        // when all 3 pressed, check sequence
        if (enteredOrder.Count == correctOrder.Length)
        {
            bool correct = true;
            for (int i = 0; i < correctOrder.Length; i++)
            {
                if (enteredOrder[i] != correctOrder[i])
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                ritualCompleted = true;
                ritualStarted = false;

                if (promptText != null)
                    promptText.text = successText;

                if (sequenceText != null)
                    sequenceText.text = "";

                if (ritualTimer != null)
                    ritualTimer.StartFinalRitual();   // ✅ method call
                                                      // starts your timer → spawns skeletons
            }
            else
            {
                // wrong order – reset, allow retry
                ritualStarted = false;
                enteredOrder.Clear();
                usedOfferings.Clear();

                if (promptText != null)
                    promptText.text = failText;

                if (sequenceText != null)
                    sequenceText.text = "";
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;

            if (!ritualStarted && !ritualCompleted && promptText != null)
                promptText.text = startPrompt;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            ritualStarted = false;
            enteredOrder.Clear();
            usedOfferings.Clear();

            if (promptText != null)
                promptText.text = "";

            if (sequenceText != null)
                sequenceText.text = "";
        }
    }
}
