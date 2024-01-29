using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// the class that updates the UI based on each player's stats
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI attempts;
    [SerializeField]
    private TextMeshProUGUI goldFish;
    [SerializeField]
    private TextMeshProUGUI yellowFish;
    [SerializeField]
    private TextMeshProUGUI greenFish;

    public void UpdateUI(Enums.TypeOfUI typeOfUI, string value)
    {
        switch (typeOfUI) 
        {
            case Enums.TypeOfUI.attempts:
                attempts.text = value;
                break;
            case Enums.TypeOfUI.goldFish:
                goldFish.text = value;
                break;
            case Enums.TypeOfUI.yellowFish:
                yellowFish.text = value;
                break;
            case Enums.TypeOfUI.greenFish:
                greenFish.text = value;
                break;
        }
    }

    public void ResetStats()
    {
        attempts.text = "0";
        goldFish.text = "0";
        yellowFish.text = "0";
        greenFish.text = "0";

    }
}
