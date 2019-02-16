﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.NST.Rewind;

namespace emotitron.NST
{

	/// <summary>
	/// NSTGhost is automatically 
	/// </summary>
	[AddComponentMenu("")]
	public class NSTGhost : MonoBehaviour, INstSource
	{
		public GameObject srcGO;
		public NetworkSyncTransform srcNst;
		public uint srcNstId;
		public NSTRewindEngine srcNstRewindEngine;

		public GameObject SrcGameObject { get { return srcGO; } }
		public NetworkSyncTransform Nst { get { return srcNst; } }
		public uint NstId { get { return srcNstId;  } }

		public void Initialize(NSTRewindEngine nstRewindEngine)
		{
			srcNst = nstRewindEngine.nst;
			srcGO = srcNst.gameObject;
			srcNstId = srcNst.NstId;
			srcNstRewindEngine = nstRewindEngine;
		}

		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
			GhostEngine.RegisterGhost(this);
		}
		private void OnDestroy()
		{
			GhostEngine.UnregisterGhost(this);
		}
	}
}



