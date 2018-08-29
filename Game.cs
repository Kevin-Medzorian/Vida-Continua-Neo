using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Mail;

public class Game : MonoBehaviour{

    private static string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VidaContinua\\playerConfig.cfg";
    private static string serverPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VidaContinua\\serverConfig.cfg";

    public static bool isServer = false;

   // public InputField email;
  //  public InputField password;
   // public InputField codefield;
   // public Button register;
    public Text loadingDatabase;

    private static string blufor = "Stun Gun x1|Light Ballistic Armor x1|M9 Pistol x1|Map x1|Patrol Officer License";
    private static string opfor = "Stun Gun x1|Light Ballistic Armor x1|P226 Pistol x1|Map x1|Soviet Enforcement License";
    private static string civilian = "Dollars x5|Map x1";
    private static string insurgent = "M9 Pistol x1|Map x1|Small Arms License";
    private static int money = 500000;

   // private static string code = "";
    private static string guid = "";
    //private static string eText;

    
    public void Quit()
    {
        Application.Quit();
    }
    
    public void Awake()
    {
        if (!File.Exists(settingsPath))
        {
            SetupConfigFile();
        }
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }


        if (!PlayerPrefs.HasKey("GUID"))
        {
            PlayerPrefs.SetString("GUID", System.Guid.NewGuid().ToString());
            PlayerPrefs.Save();
        }

        guid = PlayerPrefs.GetString("GUID");


        GetLastInventory();

        if (GetLine(0).Trim().Length <= 0)
            SetName("NameError");

        for(int i = 1; i < 13; i++)
            if(GetLine(i).Trim().Length <= 0)
                loadingDatabase.text = "config file error.";


        int anti = (int) GetSetting("anti");
        int texture = (int) GetSetting("text");
        int vsync = (int) GetSetting("vsync");

        QualitySettings.antiAliasing = (anti == 0) ? 0 : (int)Math.Pow(2, anti);
        QualitySettings.masterTextureLimit = texture;
        QualitySettings.vSyncCount = vsync;
    }

    public void GetLastInventory()
    {
        if (PlayerPrefs.HasKey("lastInv"))
        {
            string[] inv = PlayerPrefs.GetString("lastInv").Split('?');

            blufor = inv[0];
            opfor = inv[1];
            civilian = inv[2];
            insurgent = inv[3];
            money = int.Parse(inv[4]);
        }

        loadingDatabase.text = "player stats recieved.";
    }



  /*  public void Start(){ StartCoroutine(delayedStart()); }
    private IEnumerator delayedStart()
    {
        yield return new WaitForSeconds(0.05f);

      //  if (!isServer)
       //     StartCoroutine(HasTable());
    }*/

    public static string GetCiv() { return civilian; }
    public static string GetOpfor() { return opfor; }
    public static string GetBlufor() { return blufor; }
    public static string GetInsurgent() { return insurgent; }
    public static int GetMoney() {return money;}


  /*  public void SendConfirmation()
    {
        codefield.interactable = register.interactable = false;

        eText = email.text;
        if(!eText.Contains(".") || !eText.Contains("@") || eText.LastIndexOf(".") < eText.LastIndexOf("@"))
        {
            loadingDatabase.text = "invalid email";
        }
        else if(password.text.Trim().Length < 5)
        {
            loadingDatabase.text = "invalid password";
        }
        else
        {
            StartCoroutine(AlreadyRegistered());
        }
    }
    public void Register()
    {
        string input = codefield.text;

        if(input == code)
        {
            StartCoroutine(AddPlayer());
        }
        else
        {
            loadingDatabase.text = "confirmation code invalid";
        }


    }


    private IEnumerator SendCode()
    {
        loadingDatabase.text = "sending confirmation code...";


        code = System.Guid.NewGuid().ToString().Substring(1,6);
        WWW site = new WWW(sendConfirm + "email=" + WWW.EscapeURL(eText) + "&confirm=" + WWW.EscapeURL(code) + "&hash=" + WWW.EscapeURL(Md5Sum(eText + "zCOA9tia3qY")));

        print(site.url);
        yield return site;

        loadingDatabase.text = "a code has been sent to your email";

        codefield.interactable = register.interactable = true;
    }

    private IEnumerator AlreadyRegistered()
    {
        loadingDatabase.text = "checking availability...";
        WWW site = new WWW(hasTable + "email=" + WWW.EscapeURL(Md5Sum(eText+ "2jpR1R9aqgeO89T40Zy0iJaBlZcrCc4h1X8SOAfk74KBjphu16")));

        print(site.url);
        yield return site;

        string val = site.text;

        if (!string.IsNullOrEmpty(val))
        {
            loadingDatabase.text = "email already registered.";
            yield break;
        }
        else
        {
            StartCoroutine(SendCode());
        }

    }


    private IEnumerator HasTable()
    {
        WWW site = new WWW(hasTable + "email=" + WWW.EscapeURL(Md5Sum(eText + "2jpR1R9aqgeO89T40Zy0iJaBlZcrCc4h1X8SOAfk74KBjphu16")));

        yield return site;

        string val = site.text;

        if (!string.IsNullOrEmpty(val))
        {
            loadingDatabase.text = "login invalid or nonexistent";
        }
        else
        {
            loadingDatabase.text = "login successful";
            StartCoroutine(GetPlayer());
        }

    }
    private IEnumerator GetPlayer()
    {

        string encrypted = Md5Sum(eText + "2jpR1R9aqgeO89T40Zy0iJaBlZcrCc4h1X8SOAfk74KBjphu16");

        WWW site = new WWW(getPlayer + "email=" + WWW.EscapeURL(encrypted) + "&password=" + WWW.EscapeURL(Md5Sum(password.text + "yB3J72Y0zcj1y10poKl0nf5p92p5A477fDf54gnPQeaxk245U4")) + "&hash=" + WWW.EscapeURL(Md5Sum(encrypted + "18iiBc9bdh")));

        loadingDatabase.text = "fetching inventory.";
        print(site.url);

        yield return site;

        string val = site.text;

        if (string.IsNullOrEmpty(val))
        {
            loadingDatabase.text = "failed to get inventory.";
            yield return new WaitForSeconds(2.0f);
            loadingDatabase.text = "reattempting.";
            StartCoroutine(GetPlayer());
        }
        else
        {
            string[] allInventorys = val.Split("!"[0]);
            civilian = allInventorys[0];
            opfor = allInventorys[1];
            blufor = allInventorys[2];
            insurgent = allInventorys[3];
            money = int.Parse(allInventorys[4]);
            DatabaseComplete();
        }

    }
    private IEnumerator AddPlayer()
    {
        loadingDatabase.text = "registering account...";

        string encrypted = Md5Sum(eText + "2jpR1R9aqgeO89T40Zy0iJaBlZcrCc4h1X8SOAfk74KBjphu16");

        WWW site = new WWW(addPlayer + "email=" + WWW.EscapeURL(encrypted) +
                                      "&password=" + WWW.EscapeURL(Md5Sum(password.text + "yB3J72Y0zcj1y10poKl0nf5p92p5A477fDf54gnPQeaxk245U4")) +
                                      "&civ=" + WWW.EscapeURL(civilian) +
                                      "&opfor=" + WWW.EscapeURL(opfor) +
                                      "&blufor=" + WWW.EscapeURL(blufor) +
                                      "&insurgent=" + WWW.EscapeURL(insurgent) +
                                      "&money=" + WWW.EscapeURL(money.ToString()) +
                                      "&hash=" + WWW.EscapeURL(Md5Sum(encrypted + "18iiBc9bdh")));
        print(site.url);
        yield return site;
        
        string val = site.text;

        if (!string.IsNullOrEmpty(val.Trim()))
        { 
            loadingDatabase.text = "registration failed. retrying.";
            print(val);
            yield return new WaitForSeconds(2.0f);
            StartCoroutine(AddPlayer());
        }else{
            DatabaseComplete();
        }

    }

    

    private void DatabaseComplete()
    {
        loadingDatabase.text = "login successful.";

        if(GameObject.Find("Loading"))
            GameObject.Find("Loading").SetActive(false);

        Debug.Log(civilian + "\n" + opfor + "\n " + blufor + "\n" + insurgent + "\n" + money);
    }*/

    public static string GetLine(int line)
    {
        String[] lines = File.ReadAllLines(settingsPath);

        return lines[line].Substring(lines[line].IndexOf(':')+1).Trim();
    }

    public static void SetupServerFile()
    {
        string fileText = "Server Title: vida continua default server\n" +
                           "Server IP: 127.0.0.1\n" +
                           "Server Port: 7777\n" +
                           "Max Players: 30\n" +
                           "Password:  \n" +
                           "Default Police Inventory: "+blufor+"\n" +
                           "Default Civilian Inventory: "+civilian+"\n" +
                           "Default Rebel Inventory: "+insurgent+"\n" +
                           "Default Soviet Inventory: "+opfor;

        File.WriteAllText(serverPath, fileText);
    }
    public static string GetDefaultInventory(string faction){
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        switch(faction){
            case "blufor": return File.ReadAllLines(serverPath)[5].Split(':')[1];
            case "civilian": return File.ReadAllLines(serverPath)[6].Split(':')[1];
            case "insurgent": return File.ReadAllLines(serverPath)[7].Split(':')[1];
            case "opfor": return File.ReadAllLines(serverPath)[8].Split(':')[1];
            default : return "";
        }
    }
    public static string GetServerMax()
    {
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        return File.ReadAllLines(serverPath)[3].Split(':')[1];
    }
    public static string GetServerIP()
    {
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        return File.ReadAllLines(serverPath)[1].Split(':')[1];
    }

    public static string GetServerPort()
    {
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        return File.ReadAllLines(serverPath)[2].Split(':')[1];
    }

    public static string GetServerTitle()
    {
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        return File.ReadAllLines(serverPath)[0].Split(':')[1];
    }
    public static string GetServerPassword()
    {
        if (!File.Exists(serverPath))
        {
            SetupServerFile();
        }

        return File.ReadAllLines(serverPath)[3].Split(':')[1];
    }

    public static void SetupConfigFile()
    {
        string fileText = "Player Name: Unnamed\n" +
                            "Antialiasing: 1\n" +
                            "Texture Quality: 3\n" +
                            "VSync: 0\n" +
                            "Audio Volume: 0.75\n" +
                            "VOIP Volume: 0.75\n" +
                            "Music Volume: 0.75\n"+
                            "forward: W\n" +
                            "back: S\n" +
                            "left: A\n" +
                            "right: D\n" +
                            "use: E\n" +
                            "inventory: Tab\n";

        Directory.CreateDirectory(settingsPath.Replace("\\playerConfig.cfg", ""));

        File.WriteAllText(settingsPath, fileText);

    }


    public static string GetName()
    {
        if (!File.Exists(settingsPath))
        {
            SetupConfigFile();
        }

        return GetLine(0);
    }

    public static void SetName(string newName)
    {
        if (!File.Exists(settingsPath))
        {
            SetupConfigFile();
        }

        string[] lines = File.ReadAllLines(settingsPath);
        lines[0] = "Player Name: " + newName;

        File.WriteAllLines(settingsPath, lines);
    }

    public static void SavePlayerGraphics(SettingsUIManager s)
    {
        if (!File.Exists(settingsPath)){
            SetupConfigFile();
        }

        string[] lines = File.ReadAllLines(settingsPath);

        lines[1] = "Antialiasing: " + s.anti;
        lines[2] = "Texture Quality: " + s.texture;
        lines[3] = "VSync: " + s.vsync;

        File.WriteAllLines(settingsPath, lines);
    }

    public static void SavePlayerAudio(SettingsUIManager s)
    {
        if (!File.Exists(settingsPath))
        {
            SetupConfigFile();
        }

        string[] lines = File.ReadAllLines(settingsPath);

        lines[4] = "Audio Volume: " + s.audioVolume;
        lines[5] = "Voip Volume: " + s.voipVolume;
        lines[6] = "Music Volume: " + s.musicVolume;

        File.WriteAllLines(settingsPath, lines);
    }

    public static void SavePlayerControl(string type, string value)
    {
        if (!File.Exists(settingsPath))
        {
            SetupConfigFile();
        }

        string[] lines = File.ReadAllLines(settingsPath);

        for(int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(type))
            {
                lines[i] = type + ": " + value;
                break;
            }
        }

        File.WriteAllLines(settingsPath, lines);
    }

    public static KeyCode GetControl(string type)
    {
        foreach(string s in File.ReadAllLines(settingsPath))
        {
            if (s.StartsWith(type))
                return (KeyCode) Enum.Parse(typeof(KeyCode), s.Substring(s.IndexOf(':') + 2).Trim()); 
        }


        return (KeyCode)Enum.Parse(typeof(KeyCode), "x");
    }
    public static int clampSetting(int min, int max, int val)
    {
        return (val < min) ? max : (val > max) ? min : val;
    }

    public static System.Object GetSetting(string type)
    {
        switch (type)
        {
            case "name": return (System.Object)int.Parse(GetLine(0));

            case "anti": return (System.Object)int.Parse(GetLine(1));

            case "text": return (System.Object)int.Parse(GetLine(2));

            case "vsync": return (System.Object)int.Parse(GetLine(3));

            case "audio": return (System.Object)float.Parse(GetLine(4));

            case "voip": return (System.Object)float.Parse(GetLine(5));

            case "music": return (System.Object)float.Parse(GetLine(6));

            case "forward": return (System.Object)int.Parse(GetLine(7));

            case "back": return (System.Object)int.Parse(GetLine(8));

            case "left": return (System.Object)int.Parse(GetLine(9));

            case "right": return (System.Object)int.Parse(GetLine(10));

            case "use": return (System.Object)int.Parse(GetLine(11));

            case "inventory": return (System.Object)int.Parse(GetLine(12));

            

            default: return -5;
        }
    }

    public static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

}
public static class StringExt
{
    public static string Truncate(this string value, int maxLength)
    {
         if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
    }
}
