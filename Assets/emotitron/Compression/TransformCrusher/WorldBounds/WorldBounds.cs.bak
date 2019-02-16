//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities;
using emotitron.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Compression
{

	public enum FactorBoundsOn { EnableDisable, AwakeDestroy }
	/// <summary>
	/// Put this object on the root of a game map. It needs to encompass all of the areas the player is capable of moving to.
	/// The object must contain a MeshRenderer in order to get the bounds.
	/// </summary>
	//[HelpURL("https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#heading=h.4n2gizaw79m0")]
	[AddComponentMenu("Transform Crusher/World Map Bounds")]
	[ExecuteInEditMode]
	public class WorldBounds : MonoBehaviour
	{

		[Tooltip("Selects which WorldBounds group this object should be factored into.")]
		[WorldBoundsSelectAttribute]
		public int worldBoundsGrp;

		//public enum BoundsTools { Both, MeshRenderer, Collider }
		public bool includeChildren = true;

		[Tooltip("Awake/Destroy will consider a map element into the world size as long as it exists in the scene (You may need to wake it though). Enable/Disable only factors it in if it is active.")]
		[HideInInspector]
		public BoundsTools.BoundsType factorIn = BoundsTools.BoundsType.Both;


		// sum of all bounds (children included)
		[HideInInspector] public Bounds myBounds;
		[HideInInspector] public int myBoundsCount;


		void Awake()
		{
			// When mapobjects are waking up, this likely means we are seeing a map change. Silence messages until Start().
			//muteMessages = true;
			CollectMyBounds();
		}

		public void CollectMyBounds()
		{
			var wbso = WorldBoundsSO.Single;
			if (!wbso)
				return;

			var grp = wbso.worldBoundsSettings[worldBoundsGrp];
			
			myBounds = BoundsTools.CollectMyBounds(gameObject, factorIn, out myBoundsCount, includeChildren, false);

			// Remove this from all Groups then readd to the one it currently belongs to.
			WorldBoundsSO.RemoveWorldBoundsFromAll(this);

			if (myBoundsCount > 0 && enabled)
			{
				if (!grp.activeMapBoundsObjects.Contains(this))
					grp.activeMapBoundsObjects.Add(this);
			}

		}
		private void Start()
		{
			//muteMessages = false;
		}

		private void OnEnable()
		{
			FactorInBounds(true);
		}


		void OnApplicationQuit()
		{
			//muteMessages = true;
			//isShuttingDown = true;
		}

		private void OnDisable()
		{
			FactorInBounds(false);
		}

		private void FactorInBounds(bool b)
		{
			if (this == null)
				return;

			var grp = WorldBoundsSO.Single.worldBoundsSettings[worldBoundsGrp];

			if (b)
			{
				if (!grp.activeMapBoundsObjects.Contains(this))
					grp.activeMapBoundsObjects.Add(this);
			}
			else
			{
				grp.activeMapBoundsObjects.Remove(this);
			}

			grp.RecalculateWorldCombinedBounds();

			// Notify affected classes of the world size change.
			//if (isInitialized && Application.isPlaying)
			//grp.UpdateWorldBounds(); // isInitialized is to silence startup log messages
		}

		private void Update()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				CollectMyBounds();
				return;
			}
#endif
		}

#if UNITY_EDITOR

		public string BoundsReport()
		{
			return ("Contains " + myBoundsCount + " bound(s) objects:\n" +
				"Center: " + myBounds.center + "\n" +
				"Size: " + myBounds.size);
		}
#endif

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(WorldBounds))]
	[CanEditMultipleObjects]
	public class WorldBoundsEditor : Editor
	{

		public override void OnInspectorGUI()
		{

			base.OnInspectorGUI();

			var _target = (WorldBounds)target;
			var factorin = (BoundsTools.BoundsType)EditorGUILayout.EnumPopup("Factor In", _target.factorIn);
			if (_target.factorIn != factorin)
			{
				_target.factorIn = factorin;
				EditorUtility.SetDirty(_target);
				serializedObject.Update();
			}

			EditorGUILayout.HelpBox(
				_target.BoundsReport(),
				MessageType.None);

			//_target.CollectMyBounds();
			WorldBoundsSO.Single.DrawGui(target, true, false, true);
		}

	}

#endif
}

