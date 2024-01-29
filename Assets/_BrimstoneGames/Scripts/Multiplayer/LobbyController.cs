using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;


/// <summary>
/// class to handle photon connection
/// </summary>
public class LobbyController : MonoBehaviourPunCallbacks
{
    //UI elements
    [SerializeField]
    private GameObject connectingText;
    [SerializeField]
    private TMP_InputField inputCreate;
    [SerializeField]
    private TMP_InputField inputJoin;
    [SerializeField]
    private int gameplaySceneIndex; //the number for the build index 
    [SerializeField]
    private GameObject createSection;
    [SerializeField]
    private GameObject joinSection;

    /// <summary>
    /// called when connected to the Photon master server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //update UI
        createSection.SetActive(true);
        joinSection.SetActive(true);
        connectingText.SetActive(false);
    }

    /// <summary>
    /// attached to the Create Room button to make a room
    /// </summary>
    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(inputCreate.text))
            PhotonNetwork.CreateRoom(inputCreate.text);
    }

    /// <summary>
    /// attached to the Join Room button to join an existing room
    /// </summary>
    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(inputJoin.text))
            PhotonNetwork.JoinRoom(inputJoin.text);
    }

    /// <summary>
    /// keep track of number of players connect in order to flag the game ready for start
    /// </summary>
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(gameplaySceneIndex);

            if (PhotonNetwork.CurrentRoom.PlayerCount == 2) //if all players connected
            {
                GameController.canStartGame = true;
           }
        
    }

    
}

