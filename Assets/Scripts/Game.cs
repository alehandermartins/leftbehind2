using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private PhotonView myPhotonView;
    public int ActorID;

    [SerializeField]
    public Ship ship;

    [SerializeField]
    private Text countDownDisplay;

    public TimeKeeper timeKeeper = new TimeKeeper();
    public Notifier notifier;
    public EventDisplayer eventDisplayer;

    [SerializeField]
    public MainCharacter character;
    [SerializeField]
    private Character[] characters;
    public Dictionary<int, Character> characterIndex = new Dictionary<int, Character>();

    public Action action;
    public CameraShake cameraShake;
    public GameObject blocker;
    public GameObject ending;
    public GameObject endingText;
    public System.Random rnd;

    public int actionCounter = 0;
    public int actionsPerformed = 0;
    public int eventsPerformed = 0;
    public Dictionary<int, Action> actions = new Dictionary<int, Action>();
    public Dictionary<int, GameEvent> gameEvents = new Dictionary<int, GameEvent>();

    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
        rnd = new System.Random(int.Parse(PhotonNetwork.CurrentRoom.Name));

        LoadCharacter(character, PhotonNetwork.LocalPlayer);
        ActorID = character.ActorID;

        ship.Load();
        ship.Locations[character.LocationID].locationButton.Active(true);

        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerListOthers)
        {
            LoadCharacter(characters[i], player);
            i++;
        }

        timeKeeper.AddMainTimer("start", 3f, StartAttack);
        timeKeeper.AddMainTimer("game", 300, EndGame);
        timeKeeper.AddMainTimer("hit", 0, Hit);
        timeKeeper.AddMainTimer("end", 5f, End);
        timeKeeper.AddTimer("event", 10f, SendEventChoice);
        timeKeeper.AddTimer("oxygen", 30, character.Suffocate);
        timeKeeper.AddTimer("suffocate", 5f, character.Suffocate);
        timeKeeper.AddTimer("notification", 0);
        timeKeeper.AddTimer("action", 0);

        timeKeeper.Stop("game");
        timeKeeper.Stop("end");
        timeKeeper.Stop("hit");
        timeKeeper.Stop("oxygen");
        timeKeeper.Stop("suffocate");
        timeKeeper.Stop("event");
    }

    void Update()
    {
        timeKeeper.CountDown();

        if (timeKeeper.Finished("start") && !timeKeeper.Finished("game"))
        {
            DisplayTime();
            if (ship.Locations[character.LocationID].IsRepaired())
                timeKeeper.Stop("oxygen");
            else
                timeKeeper.Resume("oxygen");
        }

        //RunEvents();
        notifier.Notify();
        character.Breath();
        ship.UpdateShields();
        CheckAction();
        PerformActions();
    }

    private void StartAttack()
    {
        timeKeeper.Resume("hit");
        timeKeeper.Resume("game");
        notifier.Add(0, "We are under attack!!");
    }

    private void DisplayTime()
    {
        float timerToEndGame = timeKeeper.Get("game");
        int minutes = Mathf.FloorToInt(timerToEndGame / 60F);
        int seconds = Mathf.FloorToInt(timerToEndGame - minutes * 60);
        string tempTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        countDownDisplay.text = tempTime;
    }

    public void LoadCharacter(Character charac, Player player)
    {
        int locationID = rnd.Next(0, ship.Locations.Length);
        charac.Load(player.ActorNumber, locationID);
        characterIndex.Add(player.ActorNumber, charac);
    }

    private void PerformActions()
    {
        for (int i = actionsPerformed; i < actions.Count; i++)
        {
            if (actions.ContainsKey(i))
            {
                actions[i].Perform();
                actionsPerformed++;
            }
            else if (gameEvents.ContainsKey(i) && gameEvents[i].solvable)
            {
                gameEvents[i].Perform();
                actionsPerformed++;
            }
            else
                break;
        }
    }

    private void CheckAction()
    {
        if (!(action is null))
        {
            if (timeKeeper.Finished("action"))
            {
                if (action.notifiable)
                    SendAction(action);
                else
                    action.Perform();

                action.Success();
                action = null;
            }
            else
                action.Progress();
        }
    }

    [PunRPC]
    private void RPC_AddAction(string data, PhotonMessageInfo info)
    {
        string[] actionData = data.Split(',');
        Action actionToPerform = new Action(this);

        switch (actionData[1])
        {
            case "hitShip":
                actionToPerform = new HitShipAction(this);
                break;
            case "move":
                actionToPerform = new MoveAction(this);
                break;
            case "search":
                actionToPerform = new SearchAction(this);
                break;
            case "repair":
                actionToPerform = new RepairAction(this);
                break;
            case "heal":
                actionToPerform = new HealAction(this);
                break;
            case "boostShields":
                actionToPerform = new BoostShieldsAction(this);
                break;
            case "share":
                actionToPerform = new ShareAction(this);
                break;
            case "suffocate":
                actionToPerform = new SuffocateAction(this);
                break;
        }

        actionToPerform.Deserialize(actionData);
        actions.Add(int.Parse(actionData[0]), actionToPerform);
    }

    [PunRPC]
    private void RPC_AddEvent(string data, PhotonMessageInfo info)
    {
        string[] eventData = data.Split(',');
        GameEvent eventToPerform = new GameEvent(this);

        switch (eventData[1])
        {
            case "fallDown":
                eventToPerform = new FallDownEvent(this);
                break;
        }

        eventToPerform.Deserialize(eventData);
        gameEvents.Add(int.Parse(eventData[0]), eventToPerform);

        if (eventToPerform.players.Contains(ActorID))
        {
            notifier.DisplayEvent();
            eventDisplayer.Display(eventToPerform);
            timeKeeper.Reset("event", 10f);
            ship.gameObject.SetActive(false);
        }
    }

    public void EndEvent()
    {
        eventDisplayer.EndEvent();
        ship.gameObject.SetActive(true);
    }

    [PunRPC]
    private void RPC_NewAction(string data, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string indexeddata = actionCounter + "," + data;
            myPhotonView.RPC("RPC_AddAction", RpcTarget.All, indexeddata);
        }
        actionCounter++;
    }

    [PunRPC]
    private void RPC_EventChoice(string data, PhotonMessageInfo info)
    {
        string[] eventData = data.Split(',');
        GameEvent gameEvent = gameEvents[int.Parse(eventData[0])];
        int player = int.Parse(eventData[1]);
        int choice = int.Parse(eventData[2]);

        gameEvent.SetChoice(player, choice);
    }

    public void RunEvents()
    {
        if (timeKeeper.Get("game") > 290 || eventsPerformed > 0)
            return;

        List<int> players = new List<int>();
        List<int> alivePlayers = new List<int>();
        foreach (Character charac in characterIndex.Values)
        {
            if (charac.IsAlive())
            {
                alivePlayers.Add(charac.ActorID);

                if (!charac.backpack.IsEmpty() && charac.health.currentHealth >= 2)
                    players.Add(charac.ActorID);
            }
        }

        if (players.Count == alivePlayers.Count)
        {
            eventsPerformed++;
            NewEvent(alivePlayers);
        }
    }

    public void NewEvent(List<int> alivePlayers)
    {
        int tempCounter = actionCounter;
        actionCounter += alivePlayers.Count;

        foreach (int player in alivePlayers)
        {
            FallDownEvent fallDownEvent = new FallDownEvent(this);
            fallDownEvent.AssignPlayer(player);

            if (PhotonNetwork.IsMasterClient)
            {
                string indexeddata = tempCounter + "," + fallDownEvent.Serialize();
                myPhotonView.RPC("RPC_AddEvent", RpcTarget.All, indexeddata);
            }
            tempCounter ++;
        }
    }

    public void SendEventChoice()
    {
        myPhotonView.RPC("RPC_EventChoice", RpcTarget.All, eventDisplayer.SerializedChoice());
    }

    public void Hit()
    {
        timeKeeper.Reset("hit", 10f + (float)rnd.NextDouble() * 15.0f);
        if (PhotonNetwork.IsMasterClient)
        {
            HitShipAction hitShip = new HitShipAction(this);
            hitShip.DamageRooms();
            string indexeddata = actionCounter + "," + hitShip.Serialize();
            myPhotonView.RPC("RPC_AddAction", RpcTarget.All, indexeddata);
        }
        actionCounter++;
    }

    public void Shake()
    {
        StartCoroutine(cameraShake.Shake(.5f, 6f));
    }

    public void Die()
    {
        CancelAction();
        timeKeeper.Stop();
        endingText.GetComponent<Text>().text = "You Are Dead";
        TriggerEnding();
    }

    public void Survive()
    {
        CancelAction();
        endingText.GetComponent<Text>().text = "Congratulations, You Survived";
        endingText.GetComponent<Text>().color = Color.blue;
        TriggerEnding();
    }

    public void EndGame()
    {
        timeKeeper.StopAll();
        if (character.IsAlive())
            Survive();
    }

    public void TriggerEnding()
    {
        Image blokerImg = blocker.GetComponent<Image>();
        Color c = blokerImg.color;
        c.a = 0f;
        blokerImg.color = c;
        blocker.SetActive(true);
        timeKeeper.Resume("end");
    }

    private void End()
    {
        ending.SetActive(true);
        cameraShake.gameObject.SetActive(false);
        blocker.SetActive(false);
    }

    public void GoToLocation(int locationID)
    {
        if (ship.DoorsEnabled())
            return;

        if (character.LocationID != locationID)
        {
            ship.Locations[character.LocationID].locationButton.Active(false);
            MoveAction move = new MoveAction(this);
            ship.Locations[locationID].locationButton.Active(true);
            ship.OpenDoors(locationID);
            move.LoadPayload(locationID);
            PerformAction(move);
        }
        else
            ship.OpenDoors(locationID, false);
    }

    public void BackToShip()
    {
        if (ship.DoorsEnabled())
            return;

        CancelAction();
        ship.CloseDoors();
    }

    public void PerformAction(Action action)
    {
        CancelAction();
        this.action = action;
        timeKeeper.Reset("action", action.timeToComplete);
    }

    public void SendAction(Action action)
    {
        myPhotonView.RPC("RPC_NewAction", RpcTarget.All, action.Serialize());
    }

    public void CancelAction()
    {
        //We do not cancel move actions
        if (!(action is null) && action.type != "move")
        {
            action.Cancel();
            action = null;
        }
    }

    public void GameOver()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}
