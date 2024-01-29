using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fish : MonoBehaviour
{
   

    public Enums.TypeOfFish thisTypeOfFish;
    public float speed; //fish movement speed
    public List<Transform> wayPoints; //the spots that the fish can go to when patrolling
    public PhotonView myView;


    private int randomSpot; //where the fish moves to
    private bool isCaught = false; //bool used to disable other fish functions if it was caught


    public void Start()
    {
        myView = GetComponent<PhotonView>();
        // Search for all waypoints.
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        // Store the transforms.
        for (int i = 0; i < gameObjects.Length; i++)
        {
            wayPoints.Add(gameObjects[i].transform);
        }
        PickNewDestination();
    }

   



    /// <summary>
    /// selects a new WayPoint to go to
    /// </summary>
    private void PickNewDestination()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            randomSpot = Random.Range(0, wayPoints.Count);
            myView.RPC("RPC_PickNewDestination", RpcTarget.All, randomSpot);
        }
       
        
    }



    /// <summary>
    /// called from the Lure object when the fish touches the hook to see if it can be caught
    /// </summary>
    public void CheckCatchFish(int viewId, bool shouldCatch)
    {
        if (shouldCatch) //After the RTP logic is done, if this fish needs to be caught, it will be
        {
            Debug.Log("need to catch");
            myView.RPC("RPC_NotifyFishCaught", RpcTarget.All, viewId);
            return;
        }
        bool caughtThisFish = false;
        switch (thisTypeOfFish)
        {
            case Enums.TypeOfFish.goldFish: //easiest to catch
                if (Random.value > 0.3) //%70 chance to catch
                {
                    caughtThisFish = true;
                }
                break;
            case Enums.TypeOfFish.yellowFish: // medium difficulty to catch
                if (Random.value > 0.5) //%50 chance to catch
                {
                    caughtThisFish = true;
                }
                break;
            case Enums.TypeOfFish.greenFish: //hardest to catch
                if (Random.value > 0.7) //%30 chance to catch
                {
                    caughtThisFish = true;
                }
                break;
            default:
                break;
        }

        if (caughtThisFish)
        {
            myView.RPC("RPC_NotifyFishCaught", RpcTarget.All, viewId);
        }
       
        
    }


    #region RPCs


    /// <summary>
    /// tells the clients when this fish has been caught
    /// </summary>
    /// <param name="viewId"></param>
    /// <param name="fishId"></param>
    [PunRPC]
    public void RPC_NotifyFishCaught(int viewId)
    {
        isCaught = true;
        GameObject playerObj = PhotonView.Find(viewId).gameObject; //get reference to the player who caught the fish
        FishingLine line = playerObj.GetComponent<ThrowLure>().myLine; //get reference to the line of the player who caught the fish
        transform.SetParent(line.transform); //parent the fish to the lure
        line.caughtFish = true; //mark line as having caught a fish to avoid catching a second fish
        line.currentCaughtFish = this; //assign the current caught fish
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 270); //set rotation of the fish
        transform.localPosition = Vector2.zero; //set it in the mirror of the hook
    }


    /// <summary>
    /// Only master sets destination for the fish and sends this RPC to update the data on both clients so the fish sync
    /// </summary>
    /// <param name="spot"></param>
    [PunRPC]
    public void RPC_PickNewDestination(int spot)
    {
        randomSpot = spot;
        if (randomSpot <= wayPoints.Count - 1)
        {
            if (wayPoints[randomSpot].position.x <= transform.position.x) //if fish will move to the right, rotate it for visual realism
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

    }


    #endregion


    private void Update()
    {
        if (!isCaught)
        {

            transform.position = Vector2.MoveTowards(transform.position, wayPoints[randomSpot].position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, wayPoints[randomSpot].position) < 0.2f)
            {
                PickNewDestination();
            }


        }

    }
}
