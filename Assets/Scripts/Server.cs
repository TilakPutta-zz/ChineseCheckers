using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Server : MonoBehaviour
{
    public int port = 6321;
    public int numberOfPlayers;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void Init(int players)
    {
        numberOfPlayers = players;
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;
            StartListening();
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    public void StopServer()
    {
        server.Stop();
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient c in clients)
        {
            // is the client still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }

        for (int i=0; i < disconnectList.Count - 1; i++)
        {
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Socket error: " + e.Message);
            }
        }
    }

    private void Broadcast(string[] datas, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                foreach (string data in datas)
                {
                    writer.WriteLine(data);
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Debug.Log("Socket error: " + e.Message);
            }
        }
    }

    private void Broadcast(string data, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient>() { c };
        Broadcast(data, sc);
    }

    private void Broadcast(string[] datas, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient>() { c };
        Broadcast(datas, sc);
    }

    private void OnIncomingData(ServerClient c, string data)
    {
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "CWHO":
                c.clientName = aData[1];
                c.playerNumber = Int32.Parse(aData[2]);
                Broadcast("SCNN|" + c.clientName, clients);
                break;
            case "C_SEL_PEG":
                Broadcast(data.Replace('C', 'S'), clients);
                break;
            case "C_MOV_PEG":
                Broadcast(data.Replace('C', 'S'), clients);
                break;
            case "C_DONE_MOV":
                Broadcast(data.Replace('C', 'S'), clients);
                break;
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        string allUsers = "";
        foreach (ServerClient i in clients)
        {
            allUsers += i.clientName + "|";
        }

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        
        clients.Add(sc);

        StartListening();
        Debug.Log("Connected!");
        string data = "NUMP|" + numberOfPlayers + "|" + clients.Count + "!" + "SWHO|" + allUsers;
        Broadcast(data, clients[clients.Count - 1]);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


}

public class ServerClient
{
    public string clientName;
    public int playerNumber;
    public TcpClient tcp;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}