using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;

public class NetworkManager : MonoBehaviour
{
    public static bool IsServer { get; private set; }

    private static Socket listener;

    private static List<Socket> connections;


    public void StartServer(int port)
    {
        //Settings
        IsServer = true;
        connections = new List<Socket>();

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.  
        listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(localEndPoint);
        listener.Listen(100);

        Debug.Log($"Server started on ({ipAddress.ToString()})");
    }

    public void StartClient(string host, int port)
    {
        IsServer = false;

        IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);

        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        listener.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), listener);
        Debug.Log($"Connecting to host {host} - {ipAddress.ToString()}");
    }

    public void Disconnect()
    {
        if (listener != null && listener.Connected)
        {
            listener.Disconnect(false);
            listener.Shutdown(SocketShutdown.Both);
        }
        listener = null;
    }

    bool accepting = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            StartServer(5555);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            StartClient("127.0.0.1", 5555);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Disconnect();
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        if(listener != null)
        {
            if (IsServer)
            {
                if(!accepting)
                {
                    accepting = true;
                    Debug.Log("Listening for connections");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                }
                foreach (var connection in connections)
                {

                }
            }
            else
            {

            }
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        var connection = listener.EndAccept(ar);

        connections.Add(connection);

        Debug.Log($"Connection received from {connection.RemoteEndPoint.ToString()}");

        accepting = false;
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;

        // Complete the connection.  
        client.EndConnect(ar);

        Debug.Log($"Socket connected to {client.RemoteEndPoint.ToString()}");
    }

    private void OnDisable()
    {
        Disconnect();
    }
}