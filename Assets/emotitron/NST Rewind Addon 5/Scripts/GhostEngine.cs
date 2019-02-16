//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.BitUtilities;
using System.Collections.Generic;
using emotitron.Debugging;

namespace emotitron.NST.Rewind
{
	/// <summary>
	/// This class contains the static vars for the Physics layer that rewind will use for testing and the associated mask. It also contains the utility methods
	/// for setting up the physics layer and for creating ghost objects.
	/// </summary>
	public static class GhostEngine
	{
		/// <summary>
		/// The layer that will be used for rewind physics, particulary the NetworkCast() calls.
		/// </summary>
		public static int rewindLayer;

		/// <summary>
		/// List of all ghosts in game. Used by rewind when rewinding all.
		/// </summary>
		public static List<NSTGhost> ghosts = new List<NSTGhost>();

		/// <summary>
		/// The rewindLayer index turned into a mask.
		/// </summary>
		public static int rewindMask;

		private const string NoRewindLayerErrorText = "Attempting to use Rewind without a dedicated physics layer named NSTRewindEngine. You must add a dedicated physics layer named <b>'NSTRewindEngine'</b> to <b>Edit/Project Settings/Tags and Layers</b>, or make sure there is an unused physics layer in order to make use of rewind ray test methods.";

		/// <summary>
		/// Take the values for Rewind Layers in the NSTSettings and create the arrays and masks used for rewinding.
		/// </summary>
		/// <param name="settingsRewindLayers"></param>
		public static void SetUpRewindLayers(List<string> settingsRewindLayers, int rewindPhysicsLayer)
		{
			int startSearchForEmptyAtLayer = 8;

			// If rewind layer is set to default in NSTSettings, we need to find an unused layer.
			if (rewindPhysicsLayer < 1)
			{
				for (int i = startSearchForEmptyAtLayer; i < 32; i++)
				{
					if (LayerMask.LayerToName(i) == "")
					{
						// Set this layer and mask to the new found physics layer
						rewindLayer = i;
						rewindMask = 1 << i;
						startSearchForEmptyAtLayer = i + 1;

						XDebug.Log(
							("Rewind will use empty physics layer " + rewindLayer + " with a mask of " + rewindMask + ". This was the first unused layer found. " +
							 "To specify the physics layer for rewind add a dedicated physics layer named <b>'NSTRewindEngine'</b> to <b>Edit/Project Settings/Tags and Layers</b>."), true, true);
						break;
					}
				}

				// If no empty layers could be found, rewind is not possible.
				if (rewindLayer < 1)
				{
					XDebug.LogError(!XDebug.logErrors ? null : ("No 'NSTRewindEngine' layer found in layers and no unused (unnamed) physics layer could be found."));
					return;
				}
			}
			else
			{
				rewindLayer = rewindPhysicsLayer;
				rewindMask = 1 << rewindPhysicsLayer;
			}

			// Clear all physics layers from interacting with NSTRewindEngine layer
			if (rewindLayer > 0)
				for (int i = 0; i < 32; i++)
				{
					Physics.IgnoreLayerCollision(rewindLayer, i);
				}
		}

		private static List<Collider> reusableColliderList = new List<Collider>(1);
		/// <summary>
		/// Replicate entire gameobject as only empty gameobjects and colliders.
		/// </summary>
		internal static NSTGhost CreateRewindGhost(NSTRewindEngine nstRewindEngine, GameObject srcGO)
		{
			/// Create the root of the Ghost
			GameObject ghostGO = new GameObject("REWIND " + srcGO.name);
			ghostGO.transform.parent = srcGO.transform.parent;

			/// Add and initialize the NSTGhost component
			NSTGhost nstGhost = ghostGO.AddComponent<NSTGhost>();
			ghosts.Add(nstGhost);
			nstGhost.Initialize(nstRewindEngine);

			nstRewindEngine.nst.ghostGO = ghostGO;

			/// Start the recursion process
			CloneChildrenAndColliders(nstRewindEngine, srcGO, ghostGO);
			
			return nstGhost;
		}


		/// <summary>
		/// Make a barebones copy of an object including only empty gameobjects and colliders.
		/// </summary>
		private static void CloneChildrenAndColliders(NSTRewindEngine nstRewind, GameObject srcGO, GameObject copyGO)
		{
			// Copy any components flagged with the IIncludeOnGhost interface to the ghost
			IIncludeOnGhost[] compsToCopy = srcGO.GetComponents<IIncludeOnGhost>();
			for (int ii = 0; ii < compsToCopy.Length; ii++)
				(compsToCopy[ii] as Component).ComponentCopy(copyGO);

			// Let elements know that a ghost object has been created, so they can associate themselves with game objects that mirror their gameobject
			foreach (ICreateRewindGhost callbacks in nstRewind.iCreateRewindGhost)
				callbacks.OnCreateGhost(srcGO, copyGO);

			// Check if we are replicating an object that is designated as a cast element
			for (int i = 0; i < nstRewind.castDefs.Length; i++)
			{
				if (srcGO == nstRewind.castDefs[i].sourceObject)
					nstRewind.castDefs[i].ghostGO = copyGO;
			}

			//ghostGO.transform.parent = srcGO.transform.parent;
			copyGO.transform.localPosition = srcGO.transform.localPosition;
			copyGO.transform.localScale = srcGO.transform.localScale;
			copyGO.transform.localRotation = srcGO.transform.localRotation;
			copyGO.layer = rewindLayer;

			/// Clone the colliders on this node
			CloneColliders(srcGO, copyGO, nstRewind.includeLayers);

			/// Find all children and repeat this cloning process for each child
			for (int i = 0; i < srcGO.transform.childCount; i++)
			{
				Transform orig = srcGO.transform.GetChild(i);
				Transform copy = new GameObject(orig.name).transform;

				copy.parent = copyGO.transform;

				CloneChildrenAndColliders(nstRewind, srcGO.transform.GetChild(i).gameObject, copy.gameObject);
			}
		}

		private static void CloneColliders(GameObject sourceObj, GameObject copyObj, LayerMask includedLayerMask)
		{
			sourceObj.GetComponents(reusableColliderList);

			for (int i = 0; i < reusableColliderList.Count; ++i)
			{
				if (reusableColliderList[i] != null && includedLayerMask.value.GetBitInMask(sourceObj.gameObject.layer))  //((src.gameObject.layer & nst.rewindHitLayers.value) > 0))
				{
					Collider newcol = copyObj.gameObject.AddColliderCopy(reusableColliderList[i]);
					newcol.isTrigger = true;
				}
			}
		}

		public static void RegisterGhost(NSTGhost ghost)
		{
			ghosts.Add(ghost);
		}
		public static void UnregisterGhost(NSTGhost ghost)
		{
			ghosts.Remove(ghost);
		}

		public static void SetGhostToFrame(NSTGhost nstGhost, Frame frame)
		{
			/// TODO: if we make use of this, in the future reverse the recursion to start with branch tips, to reduce transform costs
			var ee = nstGhost.srcNst.nstElementsEngine;

			var tes = ee.transformElements;

			int count = ee.elementCount;
			for (int i = 0; i < count; ++i)
			{
				var te = tes[i];
				te.Apply(te.frames[frame.frameid].xform, te.ghostGO);
			}

			nstGhost.transform.position = frame.rootPos;
			nstGhost.transform.rotation = frame.RootRot;
		}
	}
}



