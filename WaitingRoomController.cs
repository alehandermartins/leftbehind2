using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoomController : MonoBehaviourPunCallbacks
{
    private PhotonView myPhotonView;

    [SerializeField]
    private int multiplayerSceneIndex;
    [SerializeField]
    private int menuSceneIndex;
    private int playerCount;
    private int roomSize;
    [SerializeField]
    private int minPlayersToStart;
    private bool readyToStart;

    public GameObject[] Characters;
    public GameObject[] Containers;

    // Start is called before the first frame update
    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
        UpdatePlayers();
    }

    void UpdateMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Containers[0].SetActive(true);
            Containers[1].SetActive(false);
        }
        else
        {
            Containers[0].SetActive(false);
            Containers[1].SetActive(true);
        }
    }

    void UpdatePlayers()
    {

        UpdateMaster();
        PlayerCountUpdate();

        foreach (GameObject character in Characters)
        {
           character.GetComponent<Image>().sprite = LoadAvatar(0);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Characters[player.ActorNumber - 1].GetComponent<Image>().sprite = LoadAvatar(player.ActorNumber);
        }
    }

    private Sprite LoadAvatar(int actorNumber)
    {
        string avatarPath = "Characters/";
        switch (actorNumber)
        {
            case 0:
                avatarPath += "empty";
                break;
            case 1:
                avatarPath += "captain";
                break;
            case 2:
                avatarPath += "pilot";
                break;
            case 3:
                avatarPath += "mechanic";
                break;
            case 4:
                avatarPath += "scientist";
                break;
        }

        return Resources.Load<Sprite>(avatarPath);
    }

    void PlayerCountUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerCount >= minPlayersToStart)
            readyToStart = true;
        else
            readyToStart = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayers();
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(multiplayerSceneIndex);
    }

    public void Cancel()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(menuSceneIndex);
    }
}
