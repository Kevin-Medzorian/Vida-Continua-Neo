using UnityEngine;
using Lidgren.Network;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetManager : MonoBehaviour
{
    public string HostAddress;
    public int HostPort;
    public string HostName;
    public string HostPassword;
    public int MaxPlayers;
    public Text loadingText;
    public Scrollbar sb;
    public Transform sr;
    public InputField password;
    public InputField direct;
    public GameObject serverListPrefab;
    public GameObject selectedServer;
    public GameObject connecting;
    public AudioClip menuClick;
    MasterTypes.Server[] ServerList;

    public bool cancelConnect = false;
    void Start()
    {

        loadingText.text = "reaching query server...";
        string gameId = Net.AddConnKey(Net.ComboWombo(Application.cloudProjectId, MasterServer.MasterKey));
        Net.client = new NetClient(new NetPeerConfiguration(gameId));
        Net.client.Start();
        Net.client.Connect(MasterServer.MasterIP, MasterServer.MasterPort);

        if (Game.isServer)
        {
            HostPort = (int.TryParse(new Regex("[^0-9]").Replace(Game.GetServerPort(), ""), out HostPort)) ? HostPort : 7777;
            HostAddress = new Regex("[^0-9.]").Replace(Game.GetServerIP(), "").Replace('`', ' ').Replace(';', ' ').Trim();
            HostName = Game.GetServerTitle().Truncate(35).Replace('`', ' ').Replace(';', ' ').Trim();
            HostPassword = Game.GetServerPassword().Replace('`', ' ').Replace(';', ' ').Trim();
            MaxPlayers = (int.TryParse(Game.GetServerMax(), out MaxPlayers)) ? MaxPlayers : 30;

            if (HostPort < 100000 && HostPort > 0 && HostAddress.IndexOf('.') > -1 && HostAddress.IndexOf('.') < HostAddress.LastIndexOf('.') && HostAddress.Length > 4 && HostAddress.Length < 18)
            {
                NetPeerConfiguration config = new NetPeerConfiguration(gameId);
                config.LocalAddress = System.Net.IPAddress.Parse(HostAddress);
                config.Port = HostPort;
                config.MaximumConnections = MaxPlayers;

                Net.server = new NetServer(config);
                Net.server.Start();
            }
        }

        StartCoroutine(WaitForConnection());
    }

    public void Direct(){
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        Regex format = new Regex("[^0-9.:]");
        string address = format.Replace(direct.text, "");

        if (address.Length > 2 && address.Contains(":") && address.IndexOf(":") > address.LastIndexOf(".") && address.IndexOf(':') == address.LastIndexOf(':'))
        {
            string[] info = address.Split(':');
            if(info[1].Length > 0){
                Net.client.Shutdown("Joining Game");

                string countDownTimer = Net.AddConnKey(Net.ComboWombo(Application.cloudProjectId, MasterServer.MasterKey));
                NetPeerConfiguration config = new NetPeerConfiguration(countDownTimer);
                config.ConnectionTimeout = 0;
                Net.client = new NetClient(new NetPeerConfiguration(countDownTimer));
                
                Net.client.Start();
                Net.client.Connect(info[0], int.Parse(info[1]));

                StartCoroutine(ConnectToServer(info[0] +":"+info[1]));
            }
        }
    }

    IEnumerator ConnectToServer(string ip){
        connecting.SetActive(true);

        connecting.transform.GetChild(0).GetComponent<Text>().text = ip;
        connecting.transform.GetChild(1).GetComponent<Text>().text = "...";

        float time = 0.0f;
        int count = 0;
        while(true){
            time += Time.deltaTime;

            if(time > 1){
                count++;
                connecting.transform.GetChild(2).GetComponent<Text>().text += ".";
                if(count == 3)
                    connecting.transform.GetChild(2).GetComponent<Text>().text = "connecting.";
            }

            if(cancelConnect == true){
                Net.client.Shutdown("Joining Game");
                Net.client = new NetClient(new NetPeerConfiguration(Net.AddConnKey(Net.ComboWombo(Application.cloudProjectId, MasterServer.MasterKey))));
                Net.client.Start();
                Net.client.Connect(MasterServer.MasterIP, MasterServer.MasterPort);
                cancelConnect = false;
                yield break;
            }

            if(Net.client.ConnectionStatus == NetConnectionStatus.Connected){
                connecting.transform.GetChild(2).GetComponent<Text>().text = "connected!";
                NetOutgoingMessage message = Net.client.CreateMessage();
                message.Write("RequestEntrance");
                message.Write("");
                Net.client.SendMessage(message, Net.client.ServerConnection, NetDeliveryMethod.ReliableSequenced);
                break;
            }   
            yield return null;
        }  
    }

    void Update()
    {
        if (Net.client != null)
        {
            NetIncomingMessage message;
            while ((message = Net.client.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        switch (message.ReadString())
                        {
                            case "SendServerList": OnListRecieved(message.ReadString()); break;
                            case "DuplicateServer": Debug.LogError("HOST SERVER FAILURE: invalid port forward/host address, or, server ip or server title already in use."); break;
                        }
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        print(message.SenderConnection.Status.ToString());
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        print(message.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        print(message.ReadString());
                        break;
                    default:
                        Debug.Log("unhandled message with type: " + message.MessageType);
                        break;
                }
                Net.client.Recycle(message);
            }
        }
    }
    void OnClientConnected()
    {
        if (Game.isServer)
        {
            if (HostPort < 100000 && HostPort > 0 && HostAddress.IndexOf('.') > -1 && HostAddress.IndexOf('.') < HostAddress.LastIndexOf('.') && HostAddress.Length > 4 && HostAddress.Length < 18)
            {
                NetOutgoingMessage addServer = Net.client.CreateMessage();
                addServer.Write("AddServer");
                addServer.Write(HostName + "`" + HostAddress + "`" + HostPort + "`" + MaxPlayers + "`" + HostPassword);

                Net.client.SendMessage(addServer, Net.client.ServerConnection, NetDeliveryMethod.ReliableSequenced);
            }
        }
        else
        {
            RefreshServerList();
        }
    }
    void OnListRecieved(string list)
    {
        loadingText.text = "server list recieved.";
        if (list.Trim().Length > 0)
        {
            print("List: " + list);
            loadingText.text = "server list collected.";

            foreach (Transform child in sr) { Destroy(child.gameObject); }

            string[] servers = (list.IndexOf(';') > -1) ? list.Split(';') : new string[] { list };

            sr.GetComponent<RectTransform>().sizeDelta = new Vector2(sr.GetComponent<RectTransform>().sizeDelta.x, (servers.Length * 138));

            ServerList = new MasterTypes.Server[servers.Length];


            loadingText.text = "loading server list.";
            for(int i = 0; i < servers.Length; i++)
            {
                string[] serverInfo = servers[i].Split('`');

                if(serverInfo.Length == 6){
                    ServerList[i] = new MasterTypes.Server(serverInfo);
                    StartCoroutine(PingServer(i));
                }
            }
        }
    }

    public void AddServerButton(int index, int ping){
             GameObject go = Instantiate(serverListPrefab);
             go.transform.SetParent(sr);
             go.GetComponent<Button>().onClick.AddListener(() => selectedServer = go);

            foreach (Transform c in go.transform)
            {
                if (c.name == "Title")
                       c.GetComponent<Text>().text = ServerList[index].name;
                 if (c.name == "Players")
                      c.GetComponent<Text>().text = ServerList[index].currentPlayers + "/" + ServerList[index].maxPlayers;
                 if (c.name == "Ping")
                        c.GetComponent<Text>().text = ""+ping;
            }
            
            go.GetComponent<RectTransform>().localRotation = Quaternion.Euler(go.GetComponent<RectTransform>().localRotation.x, 0.0f, go.GetComponent<RectTransform>().localRotation.x);
    }

    public void RefreshServerList()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));

        if (Net.isClientConnected())
        {
            loadingText.text = "fetching server list.";

            NetOutgoingMessage requestList = Net.client.CreateMessage();
            requestList.Write("RequestList");
            Net.client.SendMessage(requestList, Net.client.ServerConnection, NetDeliveryMethod.ReliableSequenced);
        }
    }

    IEnumerator WaitForConnection()
    {
        while (Net.client.ConnectionStatus != NetConnectionStatus.Connected) yield return null;

        loadingText.text = "query server recieved";
        OnClientConnected();
    }
    IEnumerator PingServer(int index)
    {
        Ping p = new Ping(ServerList[index].ip);

        while (!p.isDone)
            yield return null;

        AddServerButton(index, (int) (p.time / 1000));
    }

    float Width(float curPixels)
    {
        return Screen.width / (1600f / curPixels);
    }
    float Height(float curPixels)
    {
        return Screen.height / (900f / curPixels);
    }
}
