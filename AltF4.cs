using UnityEngine;
public class AltF4 : MonoBehaviour {
	void Update () {
        if (Input.GetKey(KeyCode.LeftAlt) && !Application.isEditor)
        {
            if (Input.GetKey(KeyCode.F4))
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
