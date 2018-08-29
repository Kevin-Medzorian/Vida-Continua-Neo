using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Lidgren.Network;

public class NetworkMasterClient : MonoBehaviour
{
    public GameObject connecting;
    public InputField ip;
    public AudioClip menuClick;
    public Scrollbar sb;
    public Transform sr;
    public GameObject prefab;
    private Vector2 lastPos;

    public string MasterServerIpAddress;
    public int MasterServerPort;

    public string gameTypeName;
    public string gameName;
    public string gameIP;
    private int gamePort;
    private int maxPlayers;
    private string password;
    string HostGameType = "";
    string HostGameName = "";

    public int amountt = 50;
    MasterMsgTypes.Room[] hosts = null;

    public NetworkClient client = null;

    static NetworkMasterClient singleton;

	public string consoleOutput = "";


    void Awake()
    {

        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

	public static void StartCons(){
		

	}
    void Start()
    {
        /*if (Game.isServer)
        {
            gamePort = (int.TryParse(Game.GetServerPort(), out gamePort)) ? gamePort : 7777;
            gameIP = new Regex("[^0-9.]").Replace(Game.GetServerIP(), "");
            gameName = Game.GetServerTitle();
            password = Game.GetServerPassword();
            maxPlayers = (int.TryParse(Game.GetServerMax(), out maxPlayers)) ? maxPlayers : 30;


            if (gameIP.IndexOf('.') > -1 && gameIP.IndexOf('.') < gameIP.LastIndexOf('.') && gameIP.Length > 4 && gameIP.Length < 18)
            {
                NetOutgoingMessage addServer = new NetOutgoingMessage();
                addServer.Write("AddServer");
				addServer.Write("");
            }
            //addServer.Write(gameName +"|"+Game.GetServerIP+"|")



        }

        InitializeClient();*/
    }
    void OnListRecieved(string list)
    {
        print("List: " + list);
    }

    public void RefreshServerList()
    {
        if (Net.isClientConnected())
        {
            NetOutgoingMessage requestList = new NetOutgoingMessage();
            requestList.Write("RequestList");
            Net.client.SendMessage(requestList, NetDeliveryMethod.ReliableSequenced);
        }
    }

    public void InitializeClient()
    {
        if (client != null)
        {
            Debug.LogError("Already connected");
            return;
        }

        client = new NetworkClient();
        client.Connect(MasterServerIpAddress, MasterServerPort);

        // system msgs
        client.RegisterHandler(MsgType.Connect, OnClientConnect);
        client.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);
        client.RegisterHandler(MsgType.Error, OnClientError);

        // application msgs
        client.RegisterHandler(MasterMsgTypes.RegisteredHostId, OnRegisteredHost);
        client.RegisterHandler(MasterMsgTypes.UnregisteredHostId, OnUnregisteredHost);
        client.RegisterHandler(MasterMsgTypes.ListOfHostsId, OnListOfHosts);

        //DontDestroyOnLoad(gameObject);
    }

    public void Direct()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        Regex format = new Regex("[^0-9.:]");
        string address = format.Replace(ip.text, "");

        if (address.Length > 2 && address.Contains(":") && address.IndexOf(":") > address.LastIndexOf(".") && address.IndexOf(':') == address.LastIndexOf(':'))
        {
            string[] info = address.Split(':');
            foreach (var v in hosts)
            {
                if (v.hostIp.Contains(info[0]))
                {
                    Connect(v);
                    break;
                }
            }
        }
    }

    public void Connect(MasterMsgTypes.Room server)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        print("connecting to " + ip + ":" + server.hostPort);
        GameObject.Find("loadingText").GetComponent<Text>().text = "connecting at " + server.hostIp + ":" + server.hostPort;
        connecting.SetActive(true);
        foreach (Transform child in connecting.transform)
        {
            if (child.name == "Title")
            {
                child.GetComponent<Text>().text = server.name.ToUpper();
            }
            if (child.name == "Players")
            {
                child.GetComponent<Text>().text = server.currentPlayers + "/" + server.playerLimit;
            }
        }
    }

    public void ResetClient()
    {
        if (client == null)
            return;

        client.Disconnect();
        client = null;
        hosts = null;
    }

    public bool isConnected
    {
        get
        {
            if (client == null)
                return false;
            else
                return client.isConnected;
        }
    }

    // --------------- System Handlers -----------------

    void OnClientConnect(NetworkMessage netMsg)
    {
        Debug.Log("Client Connected to Master");
        GameObject.Find("loadingText").GetComponent<Text>().text = "connected to master server";

        if (Game.isServer)
            RegisterHost(gameTypeName, gameName, "", password, maxPlayers, gamePort);
    }

    void OnClientDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("Client Disconnected from Master");
        ResetClient();
        OnFailedToConnectToMasterServer();
    }

    void OnClientError(NetworkMessage netMsg)
    {
        Debug.Log("ClientError from Master");
        OnFailedToConnectToMasterServer();
    }

    // --------------- Application Handlers -----------------

    void OnRegisteredHost(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<MasterMsgTypes.RegisteredHostMessage>();
        OnServerEvent((MasterMsgTypes.NetworkMasterServerEvent)msg.resultCode);
    }

    void OnUnregisteredHost(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<MasterMsgTypes.RegisteredHostMessage>();
        OnServerEvent((MasterMsgTypes.NetworkMasterServerEvent)msg.resultCode);
    }

    void OnListOfHosts(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<MasterMsgTypes.ListOfHostsMessage>();
        hosts = msg.hosts;
        OnServerEvent(MasterMsgTypes.NetworkMasterServerEvent.HostListReceived);
        GameObject.Find("loadingText").GetComponent<Text>().text = "server list collected.";



        foreach (Transform child in sr) { Destroy(child.gameObject); }

        lastPos = new Vector2(Width(1060), Height(522));
        sb.value = 0;
        sr.GetComponent<RectTransform>().sizeDelta = new Vector2(sr.GetComponent<RectTransform>().sizeDelta.x, 768.6f + ((hosts.Length / 5) * 650));

        foreach (MasterMsgTypes.Room r in hosts)
        //for(int nn= 0; nn < amountt; nn++)
        {
            GameObject go = Instantiate(prefab);
            go.transform.SetParent(sr);
            go.transform.localScale = new Vector3(2, 2, 1);
            go.transform.position = lastPos;
            lastPos.y -= Height(40 * 1.865f);
            go.GetComponent<Button>().onClick.AddListener(() => Connect(r));

            foreach (Transform c in go.transform)
            {
                if (c.name == "Title")
                    c.GetComponent<Text>().text = r.name;
                if (c.name == "Players")
                    c.GetComponent<Text>().text = r.currentPlayers + "/" + r.playerLimit;
                if (c.name == "Ping")
                    StartCoroutine(PingServer(c.GetComponent<Text>(), r.hostIp));
                // c.GetComponent<Text>().text =//new Ping(r.hostIp).time;
            }
            go.GetComponent<RectTransform>().localRotation = Quaternion.Euler(go.GetComponent<RectTransform>().localRotation.x, 0.0f, go.GetComponent<RectTransform>().localRotation.x);

        }

    }
    IEnumerator PingServer(Text t, string ip)
    {
        Regex format = new Regex("[^0-9.]");

        Ping p = new Ping(format.Replace(ip, ""));

        while (true)
        {
            if (p.isDone)
            {
                t.text = "" + p.time;
                break;
            }
            else
            {
                yield return null;
            }
        }

    }

    public void ClearHostList()
    {
        if (!isConnected)
        {
            Debug.LogError("ClearHostList not connected");
            return;
        }
        hosts = null;

    }

    public MasterMsgTypes.Room[] PollHostList()
    {
        if (!isConnected)
        {
            Debug.LogError("PollHostList not connected");
            return null;
        }
        return hosts;
    }

    public void RegisterHost(string gameTypeName, string gameName, string comment, string password, int playerLimit, int port)
    {
        print("registering host");
        if (!isConnected)
        {
            Debug.LogError("RegisterHost not connected");
            return;
        }

        var msg = new MasterMsgTypes.RegisterHostMessage();
        msg.gameTypeName = gameTypeName;
        msg.gameName = gameName;
        msg.comment = comment;
        msg.password = password;
        msg.playerLimit = playerLimit;
        msg.hostPort = port;

        client.Send(MasterMsgTypes.RegisterHostId, msg);

        HostGameType = gameTypeName;
        HostGameName = gameName;
    }

    public void RequestHostList(string gameTypeName)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        if (!isConnected)
        {
            Debug.LogError("RequestHostList not connected");
            GameObject.Find("loadingText").GetComponent<Text>().text = "cannot find master server.";
            return;
        }
        else
        {
            GameObject.Find("loadingText").GetComponent<Text>().text = "fetching server list.";
        }

        var msg = new MasterMsgTypes.RequestHostListMessage();
        msg.gameTypeName = gameTypeName;
        client.Send(MasterMsgTypes.RequestListOfHostsId, msg);
    }

    public void UnregisterHost()
    {
        if (!isConnected)
        {
            Debug.LogError("UnregisterHost not connected");
            return;
        }

        var msg = new MasterMsgTypes.UnregisterHostMessage();
        msg.gameTypeName = HostGameType;
        msg.gameName = HostGameName;
        client.Send(MasterMsgTypes.UnregisterHostId, msg);
        HostGameType = "";
        HostGameName = "";

        Debug.Log("send UnregisterHost");
    }

    public virtual void OnFailedToConnectToMasterServer()
    {
        Debug.Log("OnFailedToConnectToMasterServer");
    }

    public virtual void OnServerEvent(MasterMsgTypes.NetworkMasterServerEvent evt)
    {
        Debug.Log("OnServerEvent " + evt);

        if (evt == MasterMsgTypes.NetworkMasterServerEvent.HostListReceived)
        {
            foreach (var h in hosts)
            {
                Debug.Log("Host:" + h.name + " addr:" + h.hostIp + ":" + h.hostPort);
            }
        }

        if (evt == MasterMsgTypes.NetworkMasterServerEvent.RegistrationSucceeded)
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StartHost();
            }
        }

        if (evt == MasterMsgTypes.NetworkMasterServerEvent.UnregistrationSucceeded)
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StopHost();
            }
        }
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
