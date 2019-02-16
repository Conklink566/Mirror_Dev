using UnityEngine;
using emotitron.Debugging;
using System.Collections.Generic;

#if PUN_2_OR_NEWER
using Photon;
using Photon.Pun;
using Photon.Realtime;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities.Example
{
	
	/// <summary>
	/// This is a very basic PUN implementation I have supplied to make it easy to quicky get started.
	/// It doesn't make use of a lobby so it only uses one scene, which eliminates the need to add any
	/// scenes to the build. Your actual game using PUN likely will want to have multiple scenes and you
	/// will want to replace all of this code with your own.
	/// </summary>
	public class PUNSampleLauncher :
#if PUN_2_OR_NEWER
		
		MonoBehaviourPunCallbacks
#else
		MonoBehaviour
#endif
	{
		[Tooltip("The prefab to use for representing the player")]
		public GameObject playerPrefab;
		public bool autoSpawnPlayer = true;
		public KeyCode spawnPlayerKey = KeyCode.P;
		public KeyCode unspawnPlayerKey = KeyCode.O;

#if PUN_2_OR_NEWER

		

		public static GameObject localPlayer;
		public List<Transform> spawnPoints = new List<Transform>();

		private void Awake()
		{
			/// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			Photon.Pun.PhotonNetwork.AutomaticallySyncScene = true;

			if (playerPrefab)
			{
				PhotonView pv = playerPrefab.GetComponent<PhotonView>();
				if (pv == null)
					playerPrefab.AddComponent<PhotonView>();
			}
		}

		void Start()
		{
			Connect();
		}


		public override void OnConnectedToMaster()
		{
			JoinOrCreateRoom();
		}

		public override void OnJoinedLobby()
		{
			base.OnJoinedLobby();
		}

		public override void OnJoinedRoom()
		{
			if (autoSpawnPlayer)
				SpawnLocalPlayer();
			else
				Debug.Log("<b>Auto-Create for player is disabled on component '" + this.GetType().Name + "'</b>. Press '" + spawnPlayerKey + "' to spawn a player. '" + unspawnPlayerKey + "' to unspawn.");
		}

		public void Update()
		{
			if (Input.GetKeyDown(spawnPlayerKey))
				SpawnLocalPlayer();

			if (Input.GetKeyDown(unspawnPlayerKey))
				UnspawnLocalPlayer();
		}

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.IsConnected)
			{
				JoinOrCreateRoom();
			}
			else
			{
				PhotonNetwork.ConnectUsingSettings();
			}
		}

		private void JoinOrCreateRoom()
		{
			PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions() { MaxPlayers = 8 }, null);
		}

		private void SpawnLocalPlayer()
		{

			// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
			if (playerPrefab/* && !localPlayer*/)
			{
				Transform tr = spawnPoints.Count > 0 ? spawnPoints[Random.Range(0, spawnPoints.Count)] : null;
				Vector3 pos = (tr) ? tr.position : Vector3.zero;
				Quaternion rot = (tr) ? tr.rotation : Quaternion.identity;

				localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, pos, rot, 0);
				localPlayer.transform.parent = null;


			}
			else
				Debug.LogError("No PlayerPrefab defined in " + this.GetType().Name);

		}

		private void UnspawnLocalPlayer()
		{
			if (localPlayer.GetComponent<PhotonView>().IsMine && PhotonNetwork.IsConnected)
				PhotonNetwork.Destroy(localPlayer);
		}

#endif
	}


//#if UNITY_EDITOR

//	[MenuItem("Window/NST/Add PUN Bootstrap", false, 1)]

//	public static void AddPUNLauncher()
//	{
//		if (Single)
//			return;

//		EnsureExistsInScene("NST PUN Launcher", true);
//	}
//#endif


#if UNITY_EDITOR

	[CustomEditor(typeof(PUNSampleLauncher))]
	[CanEditMultipleObjects]
	public class PUNSampleLauncherEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
#if PUN_2_OR_NEWER
			EditorGUILayout.HelpBox("Sample PUN launcher code that creates a PUN room and spawns players.", MessageType.None);
#else
			EditorGUILayout.HelpBox("PUN2 not installed.", MessageType.Warning);
#endif

		}
	}

#endif

}