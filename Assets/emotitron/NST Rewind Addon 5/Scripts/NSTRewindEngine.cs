//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.NST.Rewind;
using UnityEngine.Events;
using emotitron.Compression;
using emotitron.Utilities.BitUtilities;
using emotitron.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.NST
{

	// Define the custom UnityEvent for CastResults
	// TODO find a home for this.
	[System.Serializable]
	public class UnityEventCastResults : UnityEvent<CastResults, ApplyTiming> { }

	public class NSTRewindEngine : NSTRootSingleton<NSTRewindEngine>, IOfftickSrc, INstGenerateUpdateType, INstBitstreamInjectThird, INstStart, INstOnSndUpdate, INstOnRcvUpdate, INstOnSnapshotToRewind, INstOnStartInterpolate, INstOnEndInterpolate
	{
		[Header("Rewind Ghost")]

		[Tooltip("Colliders on these layers will be cloned onto the rewind ghost. Only include layers you want to act as hitboxes.")]
		[SerializeField]
		public LayerMask includeLayers = ~0;
		public static List<NSTRewindEngine> allNstRewinds = new List<NSTRewindEngine>();

		/// <summary>
		/// Find a NSTRewindEngine by its NSTid
		/// </summary>
		public static Dictionary<uint, NSTRewindEngine> rewindLookup = new Dictionary<uint, NSTRewindEngine>();

		[HideInInspector] public HistoryFrame[] history;
		[HideInInspector] public CastResults[][] rewindCastResults; // [frameid][castdefId]
		[HideInInspector] public int[] wasCastMask;

		[HideInInspector] public CastDefinition[] castDefs;
		[HideInInspector] public int castDefIdBitcount;
		[HideInInspector] private Dictionary<string, int> castIdLookup;
		[HideInInspector] private Dictionary<string, CastDefinition> castDefLookup;

		// IOfftick interface
		public int OffticksPending { get { return rewindCastQueue.Count; } }
		public NetworkSyncTransform Nst { get { return nst; } }

		public int CastDefIdLookup(string castDefName)
		{
			int val;
			if (castIdLookup.TryGetValue(castDefName, out val))
				return val;

			XDebug.LogError(!XDebug.logErrors ? null : ("There is no NSTCastDefinition named '" + castDefName + "' on gameobject '" + name + "'. Check the NSTCastDefinition and your code to be sure they match."));
			return -1;
		}

		public CastDefinition CastDefLookup(string castDefName)
		{
			CastDefinition val;
			if (castDefLookup.TryGetValue(castDefName, out val)) return val;
			XDebug.LogError(!XDebug.logErrors ? null : (castDefName + " does not exist as a NSTCastDefinition in scene. Make sure you named it correctly and typed it correctly."));
			return null;
		}

		[HideInInspector] public NSTGhost ghost;

		// Collected interfaces on gameobject
		private IRewind[] iRewind;
		private IRewindGhostsToFrame[] iRewindGhostsToFrame;
		public INstCast[] iOnCast;
		private INstCastResults[] iNstCastResults;
		public ICreateRewindGhost[] iCreateRewindGhost;

		public HistoryFrame latestRewindFrame;

		public Queue<int> rewindCastQueue;

		// cache some values
		private int defCount;
		int frameCount;
		FrameBuffer buffer;

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();
			InitializeAwake();
		}

		private bool isAwakeInitialized;
		private void InitializeAwake()
		{
			if (isAwakeInitialized)
				return;
			isAwakeInitialized = true;

			allNstRewinds.Add(this);

			rewindCastQueue = new Queue<int>();

			// Collect all of the cast definitions
			NSTCastDefinition[] defs = GetComponentsInChildren<NSTCastDefinition>(true);
			defCount = defs.Length;

			castDefs = new CastDefinition[defCount];
			castDefIdBitcount = ((uint)castDefs.Length).BitsNeededForMaxValue();
			castIdLookup = new Dictionary<string, int>(defCount);
			castDefLookup = new Dictionary<string, CastDefinition>(defCount);

			for (int i = 0; i < defCount; ++i)
			{
				castDefs[i] = defs[i].castDefinition;
				castDefs[i].Initialize(nst, i, defs[i].gameObject);

				// Add to lookup dic
				if (castDefs[i].name != "" && !castIdLookup.ContainsKey(castDefs[i].name))
				{
					castIdLookup.Add(castDefs[i].name, i);
					castDefLookup.Add(castDefs[i].name, castDefs[i]);
				}
				else
					XDebug.LogError(!XDebug.logErrors ? null : 
						("Cast Definition '" + castDefs[i].name + "' found on NST object '" + name + "' that either has no name or the name already exists somewhere else on that model. Give it a name or rename it or it will be inaccessible."));
			}
			// Collect all cast result interfaces on this NST object
			iRewind = GetComponentsInChildren<IRewind>(true);
			iRewindGhostsToFrame = GetComponentsInChildren<IRewindGhostsToFrame>(true);
			iOnCast = GetComponentsInChildren<INstCast>(true);
			iNstCastResults = GetComponentsInChildren<INstCastResults>(true);
			iCreateRewindGhost = GetComponentsInChildren<ICreateRewindGhost>(true);
		}

		public void OnNstStart()
		{
			Initialize();
		}

		private bool initalized;
		public void Initialize()
		{
			if (initalized)
				return;
			initalized = true;

			frameCount = nst.frameCount;
			buffer = nst.buffer;

			// Add to lookup
			rewindLookup.Add(nst.NstId, this);

			history = new HistoryFrame[frameCount + 1];

			for (int i = 0; i < history.Length ; ++i)
			{
				history[i] = new HistoryFrame(i, cachedTransform.position, cachedTransform.rotation);
			}

			latestRewindFrame = history[0];

			if (MasterNetAdapter.NET_MODEL != NetworkModel.PeerToPeer) // na.IAmActingAuthority)
			{
				if (ghost == null)
					ghost = GhostEngine.CreateRewindGhost(this, gameObject);
			}

			// Alloc storage for results for all frames - most of these will remain null
			rewindCastResults = new CastResults[frameCount + 1][];

			for (int frameId = 0; frameId < rewindCastResults.Length; ++frameId)
			{
				rewindCastResults[frameId] = new CastResults[defCount];

				for (int i = 0; i < defCount; ++i)
					// Only alloc for cast results if the castdef indicates a need for confirmed.
					if (castDefs[i].authorityToAll > Replicate.CastOnly || castDefs[i].ownerToAuthority > Replicate.CastOnly)
					{
						rewindCastResults[frameId][i] = new CastResults(castDefs[i]);
						rewindCastResults[frameId][i].frame = buffer.frames[frameId];
					}
			}
			wasCastMask = new int[frameCount + 1];
		}

		void OnDestroy()
		{
			// If this was never initialized (likely an orphan being destroyed on startup), don't try to shut down - will throw nulls.
			if (!initalized)
				return;

			allNstRewinds.Remove(this);

			if (nst != null)
				rewindLookup.Remove(nst.NstId);

			// Destroy the ghost for this NST
			if (ghost)
				Destroy(ghost);
		}

		/// <summary>
		/// Primary call to trigger a cast definition to run its hitscan/overlap test and broadcast the results.
		/// </summary>
		/// <param name="castDefId"></param>
		public void NetworkCast(int castDefId)
		{
			if (na.IsMine)
			{
				rewindCastQueue.Enqueue(castDefId);
			}
			else
				XDebug.LogError(!XDebug.logErrors ? null : 
					("You are attempting to initiate a rewind cast/overlap from an object that client (or server) doesn't have authority over. You can test for authority with nst.hasAuthority."));
		}

		/// <summary>
		/// Primary call to trigger a cast definition to run its hitscan/overlap test and broadcast the results. This overload
		/// uses the cast definition name rather than id. It is recommended that you get the id from CastDefLookup(name) and cache the int id it returns
		/// and use that rather than the name.
		/// </summary>
		public void NetworkCast(string castDefName)
		{
			if (castIdLookup.ContainsKey(castDefName))
				rewindCastQueue.Enqueue(castIdLookup[castDefName]);
			else
				XDebug.LogError(!XDebug.logErrors ? null : ("No cast definition named " + castDefName + " exists for NST on " + name));
		}


		#region NST Callbacks

		// Set the ghost position to the rootPos if this is a cast (we need the shooters ghost in position)
		public void NSTBitstreamIncomingThird(Frame frame, ref UdpBitStream bitstream, ref UdpBitStream outstream, bool mirror)
		{
			if (frame.updateType.IsRewindCast())
			{
				RewindCastTools.ReadCasts(this, ref bitstream, ref outstream, mirror, frame);

				//if (NetworkServer.active && frame.updateType.IsRewindCast())
				//{
				//	ghostGO.transform.position = frame.rootPos;
				//}
			}
		}

		/// <summary>
		/// Write cast event into the next outgoing frame. The frame will have the updateType of RewindCast as a result of the initial
		/// rewindCastQueue.Enqueue.
		/// </summary>
		public void NSTBitstreamOutgoingThird(Frame frame, ref UdpBitStream bitstream)
		{
			if (frame.updateType.IsRewindCast())
				RewindCastTools.DequeueAndWriteCasts(nst, this, ref bitstream, frame);
		}

		/// <summary>
		/// Write sent updates to the rewind buffer history.
		/// </summary>
		public void OnSnd(Frame frame)
		{
			// If this is an authority NST and on the server, rewind history won't come from interpolate, so it is added here.
			//if (/*na.IAmActingAuthority*/ MasterNetAdapter.isServerClient && MasterNetAdapter.ServerIsActive && frame.frameid != frameCount)
			//{
				OnSnapshotToRewind(frame);
			//}
		}

		public void Callbacks(Frame f, ApplyTiming timing)
		{
			int fid = f.frameid;
			int castmask = wasCastMask[fid];

			CastResults[] results = rewindCastResults[fid];

			for (int castid = 0; castid < defCount; ++castid)
			{
				CastResults result = results[castid];
				if ((castmask & (1 << castid)) != 0)
					foreach (INstCastResults cb in iNstCastResults)
					{
						cb.OnCastResults(result, timing);
					}
			}
		}

		/// <summary>
		/// Casts are checked on the server as soon as they arrive currently. The results may be held and acted on in time with the
		/// frame being applied, but the cast itself happens immediately.
		/// </summary>
		public void OnRcv(Frame f)
		{
			if (!f.updateType.IsRewindCast())
				return;

			Callbacks(f, ApplyTiming.OnReceiveUpdate);
			//int fid = f.frameid;
			//int castmask = wasCastMask[fid];
			//Debug.Log(fid +  " OnRcv cast mask " + emotitron.Utilities.BitUtilities.BitTools.PrintBitMask((ushort)castmask));
			//CastResults[] results = rewindCastResults[fid];

			//for (int castid = 0; castid < results.Length; castid++)
			//{
			//	CastResults result = results[castid];
			//	if ((castmask & (1 << castid)) != 0)
			//		foreach (INstCastResults cb in iNstCastResults)
			//		{
			//			cb.OnCastResults(result, ApplyTiming.OnReceiveUpdate);
			//		}
			//}

					
		}

		public void OnStartInterpolate(Frame f, bool lateArrival = false, bool midTeleport = false)
		{
			if (!f.updateType.IsRewindCast())
				return;

			Callbacks(f, ApplyTiming.OnStartInterpolate);

			//for (int castid = 0; castid < rewindCastResults[0].Length; castid++)
			//	foreach (INstCastResults i in iNstCastResults)
			//		i.OnCastResults(rewindCastResults[f.frameid][castid], ApplyTiming.OnStartInterpolate);
		}

		public void OnEndInterpolate(Frame f)
		{
			if (!f.updateType.IsRewindCast())
				return;

			Callbacks(f, ApplyTiming.OnEndInterpolate);

			//for (int castid = 0; castid < rewindCastResults[0].Length; castid++)
			//	foreach (INstCastResults i in iNstCastResults)
			//		i.OnCastResults(rewindCastResults[f.frameid][castid], ApplyTiming.OnEndInterpolate);
		}

		public void OnGenerateUpdateType(Frame frame, ref UdpBitStream bitstream, ref bool forceKey)
		{
			if (rewindCastQueue.Count > 0)
			{
				frame.updateType |= UpdateType.RewindCast;
				// TODO cache these
				forceKey |= (nst.sendOnEvent.OnRewindCast() && frame.updateType.IsRewindCast());
			}
		}

		/// <summary>
		/// Apply this when the end of an interpolation has happened, or when the local object sends out its regular frame update.
		/// </summary>
		public void OnSnapshotToRewind(Frame frame)
		{
			// Don't bother writing history if we aren't the authoritative server
			if (NetLibrarySettings.Single.defaultAuthority != DefaultAuthority.ServerAuthority || !MasterNetAdapter.ServerIsActive || frame.frameid == frameCount)
				return;

			HistoryFrame hframe = history[frame.frameid];
			hframe.endTime = Time.time;

			//TODO: untested change to use rb.position
			hframe.rootPosition = (nst.rb) ? nst.rb.position : cachedTransform.position;

			latestRewindFrame = hframe;

			DebugWidget.Move(cachedGameObject, hframe.rootPosition, frame.RootRot, nst.debugXform, DebugXform.HistorySnapshot);

		}

		#endregion

		#region Rewind

		/// <summary>
		/// Command the rewind ghost to confrom with the transforms described in the frame.
		/// </summary>
		/// <returns>Return false if there is no rewind ghost.</returns>
		public bool RewindGhostsToFrame(Frame frame)
		{
			if (ghost == null)
				return false;

			Debug.Log("Rewinding ghost to frame " + frame);
			ghost.transform.position = frame.rootPos;

			// Command elements to rewind their ghosts
			foreach (IRewindGhostsToFrame cb in iRewindGhostsToFrame)
				cb.OnRewindGhostsToFrame(frame);

			return true;
		}

		public HistoryFrame Rewind(float t, bool applyToGhost = true)
		{
			if (na.IAmActingAuthority == false)
			{
				XDebug.LogWarning(!XDebug.logWarnings ? null : ("Attempting to use Rewind on a non-server/master client. Are you sure this was your intention? Usually rewind is done on the server(Unet) or master client (PUN)."));
			}

			float timeSinceRewindSnapshot = Time.time - latestRewindFrame.endTime;
			float timeBeforeSnapshot = t - timeSinceRewindSnapshot;
			float rewindByXFrames = timeBeforeSnapshot * nst.invFrameUpdateInterval;

			int rewindByXWholeFrames = (int)rewindByXFrames;
			float remainder = rewindByXFrames - (int)rewindByXFrames;
			
			Vector3 targetPos;
			int startId, endId;
			HistoryFrame lerpStartFrame;
			HistoryFrame lerpEndFrame;
			if (timeBeforeSnapshot > 0)
			{
				startId = buffer.Increment(latestRewindFrame.frameid, -(rewindByXWholeFrames + 1));
				endId = buffer.Increment(startId, 1);

				lerpStartFrame = history[startId];
				lerpEndFrame = history[endId];

				targetPos = Vector3.Lerp(lerpEndFrame.rootPosition, lerpStartFrame.rootPosition, remainder);

			}
			else
			{
				startId = latestRewindFrame.frameid;
				endId = 0;
				lerpStartFrame = latestRewindFrame;
				lerpEndFrame = null;
				targetPos = Vector3.Lerp(latestRewindFrame.rootPosition, cachedTransform.position, -remainder);
			}

			if (applyToGhost)
				ghost.transform.position = targetPos;

			// Command elements to rewind their ghosts
			foreach (IRewind callbacks in iRewind)
				callbacks.OnRewind(history[0], startId, endId, timeBeforeSnapshot, remainder, applyToGhost);

			return history[0];
		}

		public HistoryFrame RewindWithNst(NetworkSyncTransform playerNst, bool applyToGhost = true, bool includeClientBuffer = true, bool includeClientInterpolation = true)
		{
			float casterRTT = MasterRTT.GetRTT(playerNst.na.ClientId);

			// Don't factor in buffers and interpolation if the shooter is the host.
			float clientOffset = (casterRTT == 0) ? 0 :
				((includeClientBuffer && na.IsMine) ? nst.desiredBufferMS : 0) +
				((includeClientInterpolation && na.IsMine) ? nst.frameUpdateInterval : 0);

			return Rewind(casterRTT + clientOffset, applyToGhost);
		}

		public HistoryFrame RewindWithRTT(float casterRTT, bool applyToGhost = true, bool includeClientBuffer = true, bool includeClientInterpolation = true)
		{
			// Don't factor in buffers and interpolation if the shooter is the host.
			float clientOffset = (casterRTT == 0) ? 0 :
				((includeClientBuffer && na.IsMine) ? nst.desiredBufferMS : 0) +
				((includeClientInterpolation && na.IsMine) ? nst.frameUpdateInterval : 0);

			return Rewind(casterRTT + clientOffset, applyToGhost);
		}

		#endregion
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTRewindEngine))]
	[CanEditMultipleObjects]
	public class NSTRewindEngineEditor : NSTHeaderEditorBase
	{

		public override void OnEnable()
		{
			headerName = HeaderRewindEngineName;
			headerColor = HeaderEngineColor;
			base.OnEnable();

		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			RewindSettings.Single.DrawGui(target, true, false);
		}

	}
#endif
}



