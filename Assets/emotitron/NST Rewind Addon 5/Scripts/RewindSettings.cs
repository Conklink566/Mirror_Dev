//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.NST
{
#if UNITY_EDITOR
	[HelpURL(HELP_URL)]
#endif
	public class RewindSettings : SettingsScriptableObject<RewindSettings>
	{

		[Tooltip("Sets the size of the non-alloc arrays for raycast/overlap operations in rewind. Make sure this value is greater than the most hits you would expect from any raycast, including ALL colliders on the included cast layers, not just NST objects. This value has no effect on the amount or type of network traffic, only the how much memory is preallocated for casts.")]
		public int maxCastHits = 64;

		[Tooltip("This is the physics layer that will be reserved for rewind hit tests. No scene objects can be using this layer. Leave as 'default' and it will automatically find the first unused (unnamed) physics layer.")]
		public SingleUnityLayer physicsLayer = new SingleUnityLayer();

		[Tooltip("Automatically adds NSTRewindEngine to  NST gameobject that is touched or created during development. NSTRewindEngine will be created at runtime as needed, but it may be desireable to not wait for a rewind call to create the NSTGhost object and initialize the rewind engine.")]
		public bool autoAddRewindEngine;

#if UNITY_EDITOR
		public override string SettingsName { get { return "Rewind Settings (Add-On)"; } }
#endif
		public static bool initialized;

		public override void Initialize()
		{
			base.Initialize();

			if (initialized)
				return;

			initialized = true;

			if (HitGroupSettings.Single && Single)
				Rewind.GhostEngine.SetUpRewindLayers(HitGroupSettings.Single.hitGroupTags, Single.physicsLayer.LayerIndex);
		}

#if UNITY_EDITOR

		public const string HELP_URL = "https://docs.google.com/document/d/1nPWGC_2xa6t4f9P0sI7wAe4osrg4UP0n_9BVYnr5dkQ/edit#bookmark=kix.u2oy8ljv04db";
		public override string HelpURL { get { return HELP_URL; } }
		
		/// <summary>
		/// Simplifed method to call Single.DrawGui() for use with GetType() checks in NSTSettings - only called
		/// if Rewind-Addon exists.
		/// </summary>
		public static void StaticDrawGui(Object target, bool asFoldout, bool includeScript, bool initializeAsOpen, bool asWindow)
		{
			Single.DrawGui(target, asFoldout, includeScript,  initializeAsOpen, asWindow);
		}

		public static void StaticDrawFoldoutGui(Object target)
		{
			Single.DrawGui(target, true, false, true, false);
		}

		public static void StaticDrawWindowGui(Object target)
		{
			Single.DrawGui(target, true, false, true, true);
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(RewindSettings))]
	[CanEditMultipleObjects]
	public class RewindSettingsEditor : SettingsSOBaseEditor<RewindSettings>
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			RewindSettings.Single.DrawGui(target, false, true, true, false);
		}
	}
#endif

}



