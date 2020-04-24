using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;

public class Client : MonoBehaviour
{
    public static Client Instance { set; get; }
    public string clientName;
    public bool isHost;
    public string roomno;
    public int numberOfPlayers;
    public int playerNumber;
    //public NetworkClient network;
    //public GameObject networkClientPrefab;

    private bool socketReady;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private List<GameClient> players = new List<GameClient>();
    private GameClient[] playersPositions = new GameClient[6];

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //NetworkClient nc = Instantiate(networkClientPrefab).GetComponent<NetworkClient>();
        //Debug.Log("INSTANCE:"+nc);
        //network = nc;
    }

    public List<GameClient> getPlayers() {
        return players;
    }

    public GameClient[] getPlayerDetailsArr() {
        return playersPositions;
    }

    //public bool ConnectToServer(string host, int port)
    //{
    //    if (socketReady)
    //        return false;
    //    try
    //    {
    //        socket = new TcpClient(host, port);
    //        stream = socket.GetStream();
    //        writer = new StreamWriter(stream);
    //        reader = new StreamReader(stream);

    //        socketReady = true;
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("Socker error: " + e.Message);
    //    }

    //    return socketReady;
    //}

    private void Update()
    {

    }
     
    public void Send(string data)
    {
        NetworkClient nc = FindObjectOfType<NetworkClient>();
        JSONObject json = new JSONObject();
        json.AddField("data", data);
        json.AddField("room", roomno);
        nc.Emit("data", json);
    }


    public void HandleJoining(JSONObject data)
    {
        roomno = data["roomno"].ToString();
        GameManager.Instance.roomNo.text = roomno;
    }
    public void OnIncomingData(JSONObject json)
    {
        
        string data = json["data"].ToString();
        data = data.Trim('"');
        Debug.Log(data);
        string[] commands = data.Split('!');
        foreach (string command in commands)
        {
            string[] aData = command.Split('|');
            switch (aData[0])
            {
                case "SWHO":
                    for (int i = 1; i < aData.Length - 1; i++)
                    {
                        UserConnected(aData[i]);
                    }
                    Send("CWHO|" + clientName + "|" + playerNumber);
                    break;
                case "SCNN":
                    UserConnected(aData[1]);
                    break;
                case "NUMP":
                    numberOfPlayers = Int32.Parse(aData[1]);
                    playerNumber = Int32.Parse(aData[2]);
                    break;
                case "S_SEL_PEG":
                    Map.Instance.selectPeg(Int32.Parse(aData[1]), Int32.Parse(aData[2]), Int32.Parse(aData[3]), false);
                    break;
                case "S_MOV_PEG":
                    Map.Instance.tryMove(Int32.Parse(aData[1]), Int32.Parse(aData[2]), Int32.Parse(aData[3]), false);
                    break;
                case "S_DONE_MOV":
                    Map.Instance.completeMove(Int32.Parse(aData[1]), Int32.Parse(aData[2]), Int32.Parse(aData[3]), false);
                    break;
                case "S_CANCEL_MOV":
                    Map.Instance.cancelMove(Int32.Parse(aData[1]), false);
                    break;
            }
        }
    }

    private void UserConnected(string name)
    {
        GameClient gc = new GameClient();
        gc.name = name;
        gc.playerNumber = players.Count + 1;

        playersPositions[gc.playerNumber] = gc;

        players.Add(gc);
        Debug.Log("Players: " + numberOfPlayers + ", connected: " + players.Count);
        if (players.Count == numberOfPlayers)
        {
            GameManager.Instance.StartGame();
        }
    }
    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        //socket.Close();

        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
    public int players;
    public int playerNumber;

}