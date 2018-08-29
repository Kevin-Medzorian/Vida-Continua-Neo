using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProfileUIManager : MonoBehaviour {

    public AudioClip menuClick;
    public GameObject[] tabGroups;
    public GameObject textPrefab;

    public string[] currentInventory;

    public InputField nameField;
    public Text nam;
    public Text money;
    public Text lastServer;

    void Start()
    {
        nam.text = Game.GetName();
        money.text = "$"+ string.Format("{0:n0}", Game.GetMoney());
        lastServer.text = (PlayerPrefs.HasKey("LastServer")) ? PlayerPrefs.GetString("LastServer") : "no server found.";
    }

    public void UpdateName()
    {
        string name = nameField.text;


        if(name.Trim().Length < 2)
            nameField.text = "invalid name";
        else
            Game.SetName(nameField.text);

        nam.text = Game.GetName();
    }

	public void SwitchTab(string n)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float) Game.GetSetting("audio"));

        foreach (GameObject g in tabGroups)
        {
            if (g.name == n)
                g.SetActive(true);
            else
                g.SetActive(false);
        }

        if (GameObject.Find(n + "Scrollbar"))
        {
            foreach (Transform child in GameObject.Find(n + "Scrollbar").transform)
                Destroy(child.gameObject);


            switch (n)
            {
                case "civilian": currentInventory = Game.GetCiv().Split('|'); break;
                case "insurgent": currentInventory = Game.GetInsurgent().Split('|'); break;
                case "opfor": currentInventory = Game.GetOpfor().Split('|'); break;
                case "blufor": currentInventory = Game.GetBlufor().Split('|'); break;

                default: currentInventory = new string[] { "error: could not load inventory." }; break;
            }


            for (int i = 0; i < currentInventory.Length; i++)
            {
                GameObject text = Instantiate(textPrefab);
                text.GetComponent<RectTransform>().SetParent(GameObject.Find(n + "Scrollbar").transform);
                text.GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 1f);
                text.GetComponent<RectTransform>().localPosition = new Vector3(625, -100 - (i * 130), 0);
                text.GetComponent<Text>().text = currentInventory[i];
                text.GetComponent<RectTransform>().localRotation = Quaternion.Euler(text.GetComponent<RectTransform>().localRotation.x, 0.0f, text.GetComponent<RectTransform>().localRotation.x);
            }
        }
    }
}
