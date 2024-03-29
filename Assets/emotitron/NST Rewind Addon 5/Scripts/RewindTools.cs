﻿//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Compression;
using emotitron.Utilities.GenericCast;
using System.Collections.Generic;
using emotitron.Debugging;

namespace emotitron.NST.Rewind
{
	public static class RewindTools
	{
		public static bool TestHitscanAgainstRewind(this NSTRewindEngine targetNstRewind, NetworkSyncTransform casterNst, Ray ray)
		{
			// TODO: all of these nst references should be replaced with a straight up NSTRewindEngine instead
			HistoryFrame rewindFrame = targetNstRewind.RewindWithNst(casterNst, true);

			if (rewindFrame == null)
				return false;

			// Conduct the hit test.
			targetNstRewind.ghost.gameObject.SetActive(true);
			//TODO makes GC
			RaycastHit hit;
			bool wasHit = Physics.Raycast(ray, out hit, 100f, GhostEngine.rewindMask);

			targetNstRewind.ghost.gameObject.SetActive(false);

			XDebug.Log(!XDebug.logInfo ? null : ("Rewind Raycast " + (wasHit ? ("Hit " + hit.collider.name) : ("No Hit"))));

			return wasHit;
		}

		#region Reusables
		private static Collider[] hits = new Collider[RewindSettings.Single.maxCastHits];
		public static RaycastHit[] rayhits = new RaycastHit[RewindSettings.Single.maxCastHits];

		#endregion

		// TODO: this isn't very efficient - rework to bulk rewind to time rather than shooter nstid
		public static void RewindAllNsts(NetworkSyncTransform castingNst, bool setActive = true)
		{
			List<NSTGhost> ghosts = GhostEngine.ghosts;
			int count = ghosts.Count;
			float rtt = MasterRTT.GetRTT(castingNst.na.ClientId);

			for (int i = 0; i < count; ++i)
			{
				NSTGhost ghost = ghosts[i];
				ghost.srcNstRewindEngine.RewindWithRTT(rtt);
				ghost.gameObject.SetActive(setActive);
			}
		}

		public static void SetAllGhostsActive(bool isActive = true)
		{
			List<NSTGhost> ghosts = GhostEngine.ghosts;
			int count = ghosts.Count;
			for (int i = 0; i < count; ++i)
			{
				ghosts[i].gameObject.SetActive(isActive);
			}
		}

		/// <summary>
		/// Test unconfirmed client raycast hits against server rewind and return confirmed results. Results are written to the RewindEngine rewindCastResults
		/// prealloc. The ref returned is the same ref as that prealloc of rewindCastResults[frameid][castDef.id].
		/// </summary>
		public static CastResults ConfirmCasts(this NSTRewindEngine nstRewindEngine, Frame frame, CastResults unconfirmed, CastResults confirmed)
		{
			// We will write the confirmed results the rewindCastResults array in the rewind engine.
			//TODO move all of this to a populate() for the struct
			confirmed.Clear();
			confirmed.CastDef = unconfirmed.CastDef;

			List<NetworkSyncTransform> nsts = unconfirmed.nsts;
			int count = nsts.Count;

			//if (nstRewindEngine.ghostGO && frame.rootBitCullLevel == BitCullingLevel.NoCulling)

			// Move the caster's ghost to its position
			nstRewindEngine.ghost.transform.position = frame.rootPos;
			nstRewindEngine.ghost.transform.rotation = frame.RootRot;

			XDebug.LogWarning("NetworkCast has been fired by " + nstRewindEngine.name + ", without a full position being sent. " +
				"Be sure the Send On Event enum for this nst has " + SendCullMask.OnRewindCast + " selected if this nst is capable of moving, or else the authority will not" +
				"know the correct location of the cast source (this nst) at the time of casting, which is needed for a proper rewind.", frame.rootBitCullLevel != BitCullingLevel.NoCulling);

			// Get the source definition for this cast id. The src gameobject ghost should be correctly updated already
			CastDefinition castDef = unconfirmed.CastDef;

			Transform srcGhostT = castDef.ghostGO.transform;

			float casterRTT = MasterRTT.GetRTT(nstRewindEngine.na.ClientId);

			if (castDef.ownerToAuthority == Replicate.CastOnly)
			{
				RewindAllNsts(nstRewindEngine.nst);
			}
			else
				// Get each claimed hit objects and rewind them.
				for (int i = 0; i < count; ++i)
				{
					// Rewind unconfirmed object and enable it.
				
					NSTRewindEngine targetRewindEngine;
					uint nstid = nsts[i].NstId;

					bool success = NSTRewindEngine.rewindLookup.TryGetValue(nstid, out targetRewindEngine);
					// if the NST we are about to rewind has no NSTRewindEngine on the root of the object, we need to add one
					if (!success)
						targetRewindEngine = AddRewindEngineOnFly(nsts[i]);

					/// TODO cahce some of these
					// Rewind the ghost
					targetRewindEngine.RewindWithRTT(casterRTT, true);
					targetRewindEngine.ghost.gameObject.SetActive(true);
				}

			castDef.GenericCastAndProduceResults(srcGhostT, GhostEngine.rewindMask, confirmed, true);

			//// Set ghosts back to inactive.
			//if (castDef.ownerToAuthority == Replicate.CastOnly)
			//{
			//	SetAllGhostsActive(false);
			//}
			//else
			//{
			//	NSTRewindEngine targetRewindEngine;
			//	for (int i = 0; i < count; ++i)
			//		if (NSTRewindEngine.rewindLookup.TryGetValue(nsts[i].NstId, out targetRewindEngine))
			//			targetRewindEngine.ghostGO.SetActive(false);
			//}

			//Debug.Log(
			XDebug.Log(
				"Cast Hits Confirmed : " + confirmed);

			return confirmed;
		}

		/// <summary>
		/// Add NSTRewindEngine to an NST object at runtime. Kicks off the initialization methods that likely have been missed due to the late start.
		/// </summary>
		/// <param name="nst"></param>
		/// <param name="runStartInitialize">For very late additions of the RewindEngine - OnNstStart need to be simulated as well.</param>
		/// <returns></returns>
		public static NSTRewindEngine AddRewindEngineOnFly(NetworkSyncTransform nst, bool runStartInitialize = true)
		{
			XDebug.LogWarning(!XDebug.logErrors ? null :
				("NST gameobject '" + nst.name + "' doesn't have an NSTRewindEngine component, but requires one. " +
				"One will be added now on the fly, but you should add it to the prefab so you have control over its settings, " +
				"and to avoid any hiccups from building out a rewind ghost mid-game rather than at start-up."));

			NSTRewindEngine re = nst.gameObject.AddComponent<NSTRewindEngine>();

			// Simulate a startup since this component is now late to the game.
			re.OnNstPostAwake();
			Debug.Log("Adding callbacks for " + nst.name + " re");
			// Will have missed NST startup, so NST callbacks need to be registered.
			NSTTools.RegisterCallbackInterfaces(nst, re, false, true);

			// Can't wait for Start() to fire, so need to force initialization now before the test is tried below.
			if (runStartInitialize)
				re.Initialize();

			return re;
		}


		/// <summary>
		/// Initiate the castDef and return the results. The castMask has to be set accordingly for if this is testing against rewind objects, or actual game objects.
		/// </summary>
		public static void GenericCastAndProduceResults(this CastDefinition castDef, Transform srcT, int castMask, CastResults results, bool reset)
		{
			int hitcount = srcT.GenericCastNonAlloc(hits, rayhits, castDef.distance, castDef.radius, castMask, Quaternion.Euler(castDef.orientation), castDef.useOffset, castDef.offset1, castDef.offset2, castDef.castType);

			// Reset the results class (these should all be recycled)
			if (reset)
				results.Clear();

			//results.isConfirmed = asConfirmed;
			results.CastDef = castDef;

			// Add the hit. Should only be one, for loop is for future implementations that may involve rewinding more than one at a time.
			for (int i = 0; i < hitcount; i++)
			{
				INstSource nstHit = hits[i].transform.root.GetComponent<INstSource>();

				if (nstHit == null)
					continue;

				//ignore hits on self
				if (nstHit.Nst == castDef.nst)
					continue;

				NSTHitGroupAssign hg = GetHitGroupOfChild(hits[i].transform);

				// untagged children of any NST are assumed of the default hitGroup
				int hitgroupid = (hg == null) ? 0 : hg.hitGroupSelector.hitGroupTagId;

				int existingIndex = results.nsts.IndexOf(nstHit.Nst);

				// if this NST already is on the list then multiple hits where detected - set the mask bit for this hits layer to true
				if (existingIndex != -1)
				{
					results.hitGroupMasks[existingIndex] |= (1 << hitgroupid);
				}
				else
				{
					results.nsts.Add(nstHit.Nst);
					results.hitGroupMasks.Add(1 << hitgroupid);
				}
			}

		}

		public static NSTHitGroupAssign GetHitGroupOfChild(Transform t)
		{
			NSTHitGroupAssign hg = t.GetComponent<NSTHitGroupAssign>();
			Transform par = t.transform;

			// See if a parent object defines the hitgroup for this child if one didn't exist on the starting transform.
			while (!hg && par.transform.parent)
			{
				par = par.parent;
				hg = par.GetComponent<NSTHitGroupAssign>();

				// if we found a hitgroupassign but it isn't set to propagate to children, ignore it and keep searching up.
				if (hg && !hg.applyToChildren)
					hg = null;
			}
			return hg;
		}

		
	}
}


