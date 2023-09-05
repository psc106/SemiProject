using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class IdInputSystem : MonoBehaviour
{
    #region private Contants

    const string playerNamePreKey = "PlayerName";

    #endregion
    #region MonoBehaviour Callback

    private void Start()
    {
        string defaultName = string.Empty;
        TMP_InputField _inputField = GetComponent<TMP_InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePreKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePreKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    private void Update()
    {
       // Debug.Log(PlayerPrefs.GetString(playerNamePreKey));
    }

    #endregion

    #region public Method

    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("player name empty");
            return;

        }

        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString(playerNamePreKey, value);
    }
    
    #endregion
}
