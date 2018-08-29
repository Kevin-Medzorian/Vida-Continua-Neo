using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SettingsUIManager : MonoBehaviour{

    public AudioClip menuClick;
    [System.NonSerialized]
    public int anti, texture, vsync;

    [System.NonSerialized]
    public float audioVolume, voipVolume, musicVolume;

    string[] antis = { "NONE", "2X MSAA", "4X MSAA", "8X MSAA" };

    string[] textures = { "ULTRA", "HIGH", "MEDIUM", "LOW" };

    string[] vsyncs = { "OFF", "FANCY", "FAST"};

    Resolution[] resolutions;
    int currentResolution = 0;

    public GameObject[] tabGroups;
    public Text[] controlTexts;

    public Text displayResolution;

    public Slider audioG;
    public Slider audioM;
    public Slider audioV;

    void Start()
    {
        audioG.value = (float)Game.GetSetting("audio");
        audioM.value = (float)Game.GetSetting("music");
        audioV.value = (float)Game.GetSetting("voip");

        resolutions = Screen.resolutions;
        currentResolution = System.Array.IndexOf(resolutions, Screen.currentResolution);

        if (currentResolution == -1)
            currentResolution = 0;

        displayResolution.text = "" + resolutions[currentResolution].width + "X" + resolutions[currentResolution].height;

        anti = (int) Game.GetSetting("anti");
        texture = (int) Game.GetSetting("text");
        vsync = (int) Game.GetSetting("vsync");
        audioVolume = (float) Game.GetSetting("audio");
        voipVolume = (float) Game.GetSetting("voip");
        musicVolume = (float) Game.GetSetting("music");

        QualitySettings.antiAliasing = (anti == 0) ? 0 : (int)Math.Pow(2, anti);
        QualitySettings.masterTextureLimit = texture;
        QualitySettings.vSyncCount = vsync;

        foreach (Text t in controlTexts)
        {
            t.text = Game.GetControl(t.gameObject.name).ToString();
        }
    }

    public void SaveGraphics()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        if (!Screen.currentResolution.Equals(resolutions[currentResolution]))
        {
            Screen.SetResolution(resolutions[currentResolution].width, resolutions[currentResolution].height, true);
        }

        Game.SavePlayerGraphics(this);
    }

    public void SaveAudio()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        Game.SavePlayerAudio(this);
    }

    public void SaveControl(GameObject sender)
    {
        StartCoroutine(WaitForKeypress(sender));
    }

    public IEnumerator WaitForKeypress(GameObject item)
    {
        while (true)
        {
            foreach (KeyCode c in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(c))
                {
                    item.GetComponentInChildren<Text>().text = c.ToString().ToUpper();
                    Game.SavePlayerControl(item.name, c.ToString());
                    yield break;
                }
            }
            yield return null;
        }
    }

    public void SwitchTab(string n)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        foreach (GameObject g in tabGroups)
        {
            if (g.name == n)
                g.SetActive(true);
            else
                g.SetActive(false);
        }

        if(n == "VideoGroup")
        {
            GameObject.Find("Antialiasing quality").GetComponent<Text>().text = antis[anti];
            GameObject.Find("Texture quality").GetComponent<Text>().text = textures[texture];
            GameObject.Find("vsync quality").GetComponent<Text>().text = vsyncs[vsync];
        }
    }

    public void updateAudio(GameObject sender)
    {
        float val = sender.GetComponent<Slider>().value;
        sender.GetComponent<Text>().text = "" + Mathf.Round(val * 100);

        if (sender.name == "GameVolume")
            audioVolume = val;
        else if (sender.name == "VOIPVolume")
            voipVolume = val;
        else
            musicVolume = val;
    }
    public void increase(string setting)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        if (setting == "anti")
        {
            anti++;

            anti = (anti > 3) ? 0 : anti;

            QualitySettings.antiAliasing = (anti == 0) ? 0 : (int) Math.Pow(2, anti);
            GameObject.Find("Antialiasing quality").GetComponent<Text>().text = antis[anti];
        }

        if(setting == "text")
        {
            texture--;
            texture = (texture < 0) ? 3 : texture;

            QualitySettings.masterTextureLimit = texture;
            GameObject.Find("Texture quality").GetComponent<Text>().text = textures[texture];
        }
        if (setting == "vsync")
        {
            vsync++;
            vsync = (vsync > 2) ? 0 : vsync;

            QualitySettings.vSyncCount = vsync;
            GameObject.Find("vsync quality").GetComponent<Text>().text = vsyncs[vsync];
        }

        if (setting == "res")
        {
            currentResolution++;
            currentResolution = (currentResolution > resolutions.Length - 1) ? 0 : currentResolution;

            GameObject.Find("display resolution").GetComponent<Text>().text = resolutions[currentResolution].width +"X"+ resolutions[currentResolution].height;
        }



    }
    public void decrease(string setting)
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(menuClick, (float)Game.GetSetting("audio"));
        if (setting == "anti")
        {
            anti--;
            anti = (anti < 0) ? 3 : anti;

            QualitySettings.antiAliasing = (anti == 0) ? 0 : (int)Math.Pow(2, anti);
            GameObject.Find("Antialiasing quality").GetComponent<Text>().text = antis[anti];
        }

        if (setting == "text")
        {
            texture++;
            texture = (texture > 3) ? 0 : texture;

            

            QualitySettings.masterTextureLimit = texture;
            GameObject.Find("Texture quality").GetComponent<Text>().text = textures[texture];
        }
        if (setting == "vsync")
        {
            vsync--;
            vsync = (vsync < 0) ? 2 : vsync;

            QualitySettings.vSyncCount = vsync;
            GameObject.Find("vsync quality").GetComponent<Text>().text = vsyncs[vsync];
        }
        if (setting == "res")
        {
            currentResolution--;
            currentResolution = (currentResolution < 0) ? resolutions.Length-1 : currentResolution;

            GameObject.Find("display resolution").GetComponent<Text>().text = resolutions[currentResolution].width + "X" + resolutions[currentResolution].height;
        }
    }
}
