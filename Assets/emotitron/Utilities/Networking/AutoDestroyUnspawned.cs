//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
#if PUN_2_OR_NEWER

#else
using Mirror;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities.Networking
{
	/// <summary>
	/// Destroys objects with NetworkIdentity on them if they exist in the scene while there is no network connection.
	/// This allows prefab copies to exist in the scene while editing, without having to delete them every time you build out.
	/// </summary>
	public class AutoDestroyUnspawned : MonoBehaviour
	{

		// Hacky test for the UNET player object being in scene without being spawned there.
		bool markForDestroy;

		// Constructor
		public AutoDestroyUnspawned()
		{
#if PUN_2_OR_NEWER


#else
#pragma warning disable CS0618 // UNET is obsolete
			// If this was in the scene at startup, and was not spawned - destroy it.
			if (!NetworkServer.active && !NetworkClient.active)
				markForDestroy = true;
#pragma warning restore CS0618 // UNET is obsolete
#endif
		}

		public void Awake()
		{
#if PUN_2_OR_NEWER
			if (!Photon.Pun.PhotonNetwork.IsConnected)
				Destroy(gameObject);

#else
			// Destroy this if it was not Network Spawned
			if (markForDestroy)
				Destroy(gameObject);
#endif
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(AutoDestroyUnspawned))]
	[CanEditMultipleObjects]
	public class AutoDestroyUnspawnedEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Destroys this gameobject if network is not active when object is constructed. " +
				"Allows prefabs to be left in scene at build/play time, as a development convenience.",
				MessageType.None);
		}
	}

#endif
}





