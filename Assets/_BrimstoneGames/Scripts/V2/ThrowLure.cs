using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Misc;



/// <summary>
/// Attached to the player script. Handles the throwing of the reel
/// </summary>
public class ThrowLure : MonoBehaviour
{
    public FishingLine myLine; //reference to the Lure object
    public bool isFishing = false; // bool set to true while line is cast so we don't cast again
    public PhotonView myView; //reference to the photonView component
    [SerializeField]
    private PlayerStatsUI myPlayerStatsUI; //the player stats class instance
    
    private class FishingStats
    {
        public int attempts = 0;
        public int goldFish = 0;
        public int yellowFish = 0;
        public int greenFish = 0;

    }
    private FishingStats myStats = new FishingStats();
    private Animator myAnimator;
    

    
    private void Start()
    {
        myView = GetComponent<PhotonView>();
        myAnimator = GetComponent<Animator>();
        //assigns the correct UI panel to each player on both clients
        if (myView.IsMine)
        {   

            if (PhotonNetwork.IsMasterClient)
            {
                myView.RPC("RPC_SetupPlayer", RpcTarget.All, "Player1Stats");
            }
            else
            {
                myView.RPC("RPC_SetupPlayer", RpcTarget.All, "Player2Stats");
            }
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                myView.RPC("RPC_SetupPlayer", RpcTarget.All, "Player2Stats");
            }
            else
            {
                myView.RPC("RPC_SetupPlayer", RpcTarget.All, "Player1Stats");
            }
        }
        
        
    }


   

    /// <summary>
    /// calculates the RTP chance:
    /// - I devide the number of attempts by 10 to split them in division of ten. 
    /// - If the remainder is 7 it means we are in the final 3 attempts of a set of 10
    /// - I get the total attemts and devide them by 10 to see if I have less than 3 fish per 10 throws
    /// - If yes, then the fish that touch the hook will directly be caught
    /// </summary>
    /// <returns></returns>
    public bool GetStats()
    {
        bool result = false;
        if (myStats.attempts % 10 == 0.7f)
        {
            int totalFish = myStats.greenFish + myStats.goldFish + myStats.yellowFish;
            if (totalFish < (myStats.attempts/10 + 1) * 3)
            {
                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// casts the line after the cast animation has ended (Animation event)
    /// </summary>
    private void EnableCast()
    {
        myLine.StartCast();
    }

    /// <summary>
    /// called at the end of the reel animation to check if we caught something
    /// </summary>
    private void CheckReel()
    {
        

    }

    /// <summary>
    /// updates the UI stats
    /// </summary>
    /// <param name="typeOfFish"></param>
    public void UpdateUI(Enums.TypeOfFish typeOfFish)
    {
        switch (typeOfFish)
        {
            case Enums.TypeOfFish.greenFish:
                myStats.greenFish++;
                myPlayerStatsUI.UpdateUI(Enums.TypeOfUI.greenFish, myStats.greenFish.ToString());
                break;
                case Enums.TypeOfFish.goldFish:
                myStats.goldFish++;
                myPlayerStatsUI.UpdateUI(Enums.TypeOfUI.goldFish, myStats.goldFish.ToString());
                break;
                case Enums.TypeOfFish.yellowFish:
                myStats.yellowFish++;
                myPlayerStatsUI.UpdateUI(Enums.TypeOfUI.yellowFish, myStats.yellowFish.ToString());
                break;
        }
    }


    #region RPC
    /// <summary>
    /// RPC to set up the player and assign the correct UI for stats display
    /// </summary>
    /// <param name="objToFind"></param>
    [PunRPC]
    public void RPC_SetupPlayer(string objToFind)
    {
        GameObject stats = GameObject.Find(objToFind);

        myPlayerStatsUI = stats.GetComponent<PlayerStatsUI>();
        myPlayerStatsUI.ResetStats();

    }


    /// <summary>
    /// called when casting the line to sync on both clients
    /// </summary>
    /// <param name="mouseClick"></param>
    [PunRPC]
    public void RPC_LaunchLine(Vector2 mouseClick)
    {
       
        if (!isFishing) //if not fishing
        {
            Debug.Log("Launched the line");
            if (myLine.caughtFish == false) //if we don't have a fish in the hook
            {
                myAnimator.SetBool("cast", true); //play cast animation
                myLine.destination = mouseClick;
                myStats.attempts++;
                myPlayerStatsUI.UpdateUI(Enums.TypeOfUI.attempts, myStats.attempts.ToString()); //update attempts UI
                

            }

        }
        else //if we already have the hook in the water
        {
            Debug.Log("Pulled back line");
            myAnimator.SetBool("cast", false);
            myLine.StartReel(myAnimator); //pull back the line

        }
    }

    #endregion

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameController.canStartGame)
            {
                if (myView.IsMine)
                {
                    Vector2 mouseClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(mouseClick, Vector2.zero);

                    if (hit.collider != null && hit.collider.tag.Equals("Water"))
                    {
                        myView.RPC("RPC_LaunchLine", RpcTarget.All, mouseClick);
                    }
                }

            }

        }
       

        
    }
}
