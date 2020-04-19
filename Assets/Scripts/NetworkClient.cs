using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
public class NetworkClient : SocketIOComponent
{
    public static NetworkClient Instance { set; get; }
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        setupEvents();
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void setupEvents()
    {
        On("open", (E) =>
        {
            Debug.Log("Connection made to server");
        });

        On("joined", (E) =>
        {
            try
            {
                Client c = FindObjectOfType<Client>();
                c.HandleJoining(E.data);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        });

        On("data", (E) =>
        {
            Client c = FindObjectOfType<Client>();
            c.OnIncomingData(E.data);
        });
    }

}
