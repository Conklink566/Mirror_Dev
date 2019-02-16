using System.Collections;
using System.Collections.Generic;
using emotitron.Utilities.GUIUtilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Compression
{

	[CreateAssetMenu()]
	public class WorldBoundsSO : SettingsScriptableObject<WorldBoundsSO>
	{

#if UNITY_EDITOR
		public const string HELP_URL = "";
		public override string HelpURL { get { return HELP_URL; } }
#endif


		//[HideInInspector]
		//public List<string> worldBoundsNames = new List<string>() { "default", "hambone" };

		[HideInInspector]
		public List<WorldBoundsSettings> worldBoundsSettings = new List<WorldBoundsSettings>() { new WorldBoundsSettings() };


		//public override void DrawGuiPre()
		//{
		//	NSTMapBounds.boundsPosCrusher.xcrusher.Resolution = (ulong)minPosResolution;
		//	NSTMapBounds.boundsPosCrusher.ycrusher.Resolution = (ulong)minPosResolution;
		//	NSTMapBounds.boundsPosCrusher.zcrusher.Resolution = (ulong)minPosResolution;

		//	EditorGUILayout.Space();

		//	int res = EditorGUILayout.IntSlider(new GUIContent("Map Bounds res:", "The min resolution setting that will be used by NSTMapBounds"), minPosResolution, 0, 500);

		//	if (minPosResolution != res)
		//	{
		//		Undo.RecordObject(this, "MapBounds resolution change");
		//		minPosResolution = res;
		//		EditorUtility.SetDirty(this);
		//		AssetDatabase.SaveAssets();
		//	}

		//	EditorGUILayout.HelpBox(WorldBoundsSummary(), MessageType.None);

		//	EditorGUILayout.HelpBox("Default settings are used when no NSTMapBounds exist in a scene.\n" + NSTMapBounds.ActiveBoundsObjCount + " NSTMapBounds sources currently active.", MessageType.None);

		//}

		public static void RemoveWorldBoundsFromAll(WorldBounds wb)
		{
			for (int i = 0; i < Single.worldBoundsSettings.Count; ++i)
				single.worldBoundsSettings[i].activeMapBoundsObjects.Remove(wb);
		}
#if UNITY_EDITOR

		const float helpboxhght = 42f;

		public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{
			bool isexpanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);

			var so = new SerializedObject(this);
			so.Update();
			EditorGUI.BeginChangeCheck();

			SerializedProperty wbs = so.FindProperty("worldBoundsSettings");

			Rect r = EditorGUILayout.GetControlRect();
			EditorGUI.LabelField(r, "Group Name", (GUIStyle)"MiniLabel");
			//r.xMax -= 16;
			EditorGUI.LabelField(r, "Resolution (x/1 units)", CrusherDrawer.miniLabelRight );

			for (int i = 0; i < wbs.arraySize; ++i)
			{
				r = EditorGUILayout.GetControlRect(false, EditorGUI.GetPropertyHeight(wbs.GetArrayElementAtIndex(i)) + helpboxhght + 4);
				//r = ;
				r.xMax -= 16;

				if (EditorGUI.PropertyField(r, wbs.GetArrayElementAtIndex(i)))
				{
					so.ApplyModifiedProperties();
				}

				r.xMax += 16;
				r.yMin += 16;
				r.height = helpboxhght;
				worldBoundsSettings[i].RecalculateWorldCombinedBounds();

				string summary = (worldBoundsSettings[i].ActiveBoundsObjCount == 0) ?
					"There are no WorldMapBounds components active in the current scene for group '" + worldBoundsSettings[i].name + "'." :
					worldBoundsSettings[i].BoundsReport();
				//r.xMax += 16;
				EditorGUI.LabelField(r, summary, (GUIStyle)"HelpBox");

				EditorGUI.BeginDisabledGroup(i == 0);
				{
					r.yMin -= 16;
					r.xMin = r.xMax -16;
					r.width = 16;
					r.height = 16;
					if (GUI.Button(r, "X"))
					{
						wbs.DeleteArrayElementAtIndex(i);
						so.ApplyModifiedProperties();
					}
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.Space();
			}


			if (GUI.Button(EditorGUILayout.GetControlRect(), "Add Bounds Group"))
			{
				int newidx = wbs.arraySize;
				wbs.InsertArrayElementAtIndex(newidx);
				wbs.GetArrayElementAtIndex(newidx).FindPropertyRelative("name").stringValue = WorldBoundsSettings.newAddName;

				so.ApplyModifiedProperties();
				EnsureNamesAreUnique(newidx);
				so.Update();
			}

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(so.targetObject);
			}

			return isexpanded;
		}

#endif
		///// <summary>
		///// Change the axisranges for the world bounds to a new bounds.
		///// </summary>
		//public static void SetWorldRanges(int index, Bounds bounds, bool silent = false)
		//{
		//	///// TODO: MOVE THIS
		//	//if (WorldBounds.ActiveBoundsObjCount > 0)
		//	//	Single.worldBoundsSettings[index].crusher = NSTMapBounds.boundsPosCrusher;
		//	//else
		//	//	globalPosCrusher = Single.defaultPosCrusher;

		//	////var worldCompSettings = WorldCompressionSettings.Single;
		//	//var worldCrusher = NSTMapBounds.boundsPosCrusher;
		//	////NSTSettings nstSettings = NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
		//	//XDebug.LogWarning(!XDebug.logWarnings ? null :
		//	//	("<b>Scene is missing map bounds</b>, defaulting to a map size of Center:" + NSTMapBounds.combinedWorldBounds.center + " Size:" + NSTMapBounds.combinedWorldBounds.size +
		//	//	". Be sure to add NSTMapBounds components to your scene to define its bounds, or be sure the default bounds in NSTSettings are what you want."),
		//	//	(!silent && Application.isPlaying && NSTMapBounds.ActiveBoundsObjCount == 0 && Time.time > 1)
		//	//	);

		//	//XDebug.LogWarning(!XDebug.logWarnings ? null :
		//	//	("<b>Scene map bounds are very small</b>. Current world bounds are " + bounds.center + " Size:" + bounds.size + ", is this intentional?" +
		//	//	"If not check that your NSTMapBounds fully encompass your world as intended, or if using the Default bounds set in NSTSettings, that it is correct."),
		//	//	(!silent && Application.isPlaying && NSTMapBounds.ActiveBoundsObjCount > 0 && (bounds.size.x <= 1 || bounds.size.y <= 1 || bounds.size.z <= 1))
		//	//	);

		//	////for (int axis = 0; axis < 3; axis++)
		//	////{
		//	////	worldCrusher[axis].SetRange((float)bounds.min[axis], (float)bounds.max[axis]); //, (uint)worldCompSettings.minPosResolution);
		//	////}

		//	//XDebug.Log(
		//	//	("Notice: Change in Map Bounds (Due to an NSTBounds being added or removed from the scene) to \n" +
		//	//	"Center:" + bounds.center + " Size:" + bounds.size + ". Be sure this map change is happening to all networked clients or things will break badly. \n" +
		//	//	"Position keyframes will use x:" + worldCrusher[0].Bits + " bits, y:" + worldCrusher[1].Bits + "bits, and z:" + worldCrusher[2].Bits +
		//	//	" bits at the current minimum resolutons settings (in NST Settings)."), !silent && Application.isPlaying, true);
		//}


#if UNITY_EDITOR
		private static HashSet<string> namecheck = new HashSet<string>();
		public static void EnsureNamesAreUnique(int newestIndex = -1)
		{
			namecheck.Clear();
			var wbs = Single.worldBoundsSettings;
			bool haschanged = false;

			for (int i = 0; i < wbs.Count; ++i)
			{
				var wbsi = wbs[i];
				// Try adding newest changed last, so it will get its named changed, rather than an existing duplicate
				if (i == newestIndex && i != 0)
					continue;

				if (i == 0)
				{
					if (wbsi.name != WorldBoundsSettings.defaultName)
					{
						haschanged = true;
						wbsi.name = WorldBoundsSettings.defaultName;
					}
				}
				else
					while (namecheck.Contains(wbsi.name))
					{
						haschanged = true;
						wbsi.name += "X";
					}

				namecheck.Add(wbsi.name);
			}

			// Change the newest changed name as needed (if supplied and valid) last
			if (newestIndex > 0 && newestIndex < wbs.Count)
				while (namecheck.Contains(wbs[newestIndex].name))
				{
					haschanged = true;
					wbs[newestIndex].name += "X";
				}

			if (haschanged)
			{
				EditorUtility.SetDirty(Single);
			}
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(WorldBoundsSO))]
	public class WorldBoundsSOEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			WorldBoundsSO.Single.DrawGui(target, false, true, true);
		}
	}
#endif
}

