using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { set; get; }

    public GameObject mainMenu;
    public GameObject serverMenu;
    public GameObject connectMenu;
    public GameObject createRoomMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;
    public GameObject networkClientPrefab;

    public InputField inputName;
    public InputField inputPlayers;
    public Text roomNo;
    private void Start()
    {
        Instance = this;
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        createRoomMenu.SetActive(false);
        DontDestroyOnLoad(gameObject);

    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
        NetworkClient nc = Instantiate(networkClientPrefab).GetComponent<NetworkClient>();
        Client c = Instantiate(clientPrefab).GetComponent<Client>();
        c.clientName = inputName.text;
        if (c.clientName == "")
        {
            c.clientName = "Client";
        }
    }

    public void HostButton()
    {
        try
        {
            NetworkClient nc = Instantiate(networkClientPrefab).GetComponent<NetworkClient>();
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = inputName.text;
            if (c.clientName == "")
            {
                c.clientName = "Host";
            }
            c.isHost = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
        mainMenu.SetActive(false);
        serverMenu.SetActive(false);
        createRoomMenu.SetActive(true);
    }

    public void CreateRoomButton()
    {
        NetworkClient nc = FindObjectOfType<NetworkClient>();
        Client c = FindObjectOfType<Client>();
        c.numberOfPlayers = Int32.Parse(inputPlayers.text);
        JSONObject json = new JSONObject();
        json.AddField("numberOfPlayers", c.numberOfPlayers);
        nc.Emit("createroom", json);
        createRoomMenu.SetActive(false);
        serverMenu.SetActive(true);
    }

    public void ConnectToServerButton()
    {
        string roomno = GameObject.Find("HostInput").GetComponent<InputField>().text;

        try
        {
            NetworkClient nc = FindObjectOfType<NetworkClient>();
            Client c = FindObjectOfType<Client>();
            c.roomno = roomno;
            JSONObject data = new JSONObject();
            data.AddField("roomno", roomno);
            
            nc.Emit("joinRoom", data);
            Debug.Log("Joining room:" + roomno);
            connectMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        createRoomMenu.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if (s != null)
        {
            s.GetComponent<Server>().StopServer();
            Destroy(s.gameObject);
        }

        Client c = FindObjectOfType<Client>();
        if (c != null)
        {
            Destroy(c.gameObject);
        }

        NetworkClient nc = FindObjectOfType<NetworkClient>();
        if (nc != null)
        {
            nc.socket.Close();
            Destroy(nc.gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
