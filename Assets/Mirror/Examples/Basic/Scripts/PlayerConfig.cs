using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace App.Player
{
    public class PlayerConfig : NetworkBehaviour
    {
        /// <summary>
        /// Components that will be disabled
        /// </summary>
        [SerializeField]
        private Behaviour[] _DisabledComponents;

        /// <summary>
        /// Get this from scene
        /// </summary>
        private Camera _MainCamera;

        /// <summary>
        /// Player Mimic
        /// </summary>
        [SerializeField]
        private GameObject _PlayerMimic;

        /// <summary>
        /// Start this instance
        /// </summary>
        private void Start()
        {
            if(!isLocalPlayer)
            {
                for(int i = 0; i < this._DisabledComponents.Length; i++)
                {
                    this._DisabledComponents[i].enabled = false;
                }
            }
            else
            {
                this._MainCamera = Camera.main;
                if (this._MainCamera != null)
                    this._MainCamera.gameObject.SetActive(false);
                HUDManager.Instance.MessageText.enabled = true;
                HUDManager.Instance.MessageToggle(false);
                HUDManager.Instance.InputOutputText.SetActive(true);
            }
        }

        /// <summary>
        /// When this gameobject is disabled
        /// </summary>
        private void OnDisable()
        {
            if (this._MainCamera != null)
                this._MainCamera.gameObject.SetActive(true);
            if (isLocalPlayer)
            {
                HUDManager.Instance.MessageText.enabled = false;
                HUDManager.Instance.ClearInputOutput();
                HUDManager.Instance.InputOutputText.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

