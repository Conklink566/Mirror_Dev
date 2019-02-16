using System.Collections;
using System.Collections.Generic;
using emotitron.Utilities.GUIUtilities;
using UnityEngine;
using emotitron.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Compression
{
	[System.Serializable]
	public class WorldBoundsSettings
	{
		public const string defaultName = "default";
		public const string newAddName = "Unnamed";

		public string name = defaultName;

		public readonly ElementCrusher crusher = new ElementCrusher(TRSType.Position, false)
		{
			enableLocalSelector = false,
			hideFieldName = true,
			xcrusher = new FloatCrusher(-100f, 100f, 100, Axis.X, TRSType.Position),
			ycrusher = new FloatCrusher(-20f, 20f, 100, Axis.Y, TRSType.Position),
			zcrusher = new FloatCrusher(-100f, 100f, 100, Axis.Z, TRSType.Position)
		};
		public int resolution = 100;

		[System.NonSerialized]
		public List<WorldBounds> activeMapBoundsObjects = new List<WorldBounds>();

		public int ActiveBoundsObjCount { get { return activeMapBoundsObjects.Count; } }

		[System.NonSerialized]
		public Bounds _combinedWorldBounds;

		//[System.NonSerialized]
		//public bool muteMessages;
		//[System.NonSerialized]
		//private bool isShuttingDown;


		public void ResetActiveBounds()
		{
			activeMapBoundsObjects.Clear();
		}

		/// <summary>
		/// Whenever an instance of NSTMapBounds gets removed, the combinedWorldBounds needs to be rebuilt with this.
		/// </summary>
		public void RecalculateWorldCombinedBounds()
		{

			if (activeMapBoundsObjects.Count == 0)
			{
				_combinedWorldBounds = new Bounds(); // (new Vector3(6, 6, 6), new Vector3(6,6,6));

				/// When we have no bounds for a group, default to uncompressed to ensure "always works"
				crusher.xcrusher.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
				crusher.ycrusher.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
				crusher.zcrusher.BitsDeterminedBy = BitsDeterminedBy.Uncompressed;
				return;

			}
			else
			{
				/// When we have bounds for a group, switch back to Resolution mode
				crusher.xcrusher.BitsDeterminedBy = BitsDeterminedBy.Resolution;
				crusher.ycrusher.BitsDeterminedBy = BitsDeterminedBy.Resolution;
				crusher.zcrusher.BitsDeterminedBy = BitsDeterminedBy.Resolution;

				// must have a starting bounds to encapsulate, otherwise it starts encapsulating a 0,0,0 center which may not be desired.
				_combinedWorldBounds = activeMapBoundsObjects[0].myBounds;
				for (int i = 1; i < activeMapBoundsObjects.Count; i++)
				{
					_combinedWorldBounds.Encapsulate(activeMapBoundsObjects[i].myBounds);
				}
			}

			crusher.xcrusher.Resolution = (ulong)resolution;
			crusher.ycrusher.Resolution = (ulong)resolution;
			crusher.zcrusher.Resolution = (ulong)resolution;
			crusher.Bounds = _combinedWorldBounds;

		}
		//public void UpdateWorldBounds(bool mute = false)
		//{
		//	// No log messages if commanded, if just starting up, or just shutting down.
		//	WorldBoundsSO.SetWorldRanges(0, _combinedWorldBounds, muteMessages || mute);
		//}

#if UNITY_EDITOR
		public string BoundsReport()
		{
			return ("Encapsulates " + ActiveBoundsObjCount + " " + typeof(WorldBounds).Name +  "\n" +
				"Combined Center: " + _combinedWorldBounds.center + "\n" +
				"Combined Size: " + _combinedWorldBounds.size);
		}
#endif
	}


#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(WorldBoundsSettings))]
	public class WorldBoundsSettingsDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool haschanged = false;

			float lw = EditorGUIUtility.labelWidth;

			Rect r = position;
			r.xMax = lw - 4;
			r.height = 16;

			var name = property.FindPropertyRelative("name");
			//if (name.stringValue == "default")
			//{
			//	EditorGUI.BeginDisabledGroup(name.stringValue == "default");
			//	EditorGUI.LabelField(r, name.stringValue);
			//	EditorGUI.EndDisabledGroup();
			//}
			//else
			{
				EditorGUI.BeginDisabledGroup(name.stringValue == WorldBoundsSettings.defaultName);
				string n = EditorGUI.DelayedTextField(r, name.stringValue);
				EditorGUI.EndDisabledGroup();
				if (n != name.stringValue)
				{
					Undo.RecordObject(property.serializedObject.targetObject, "Undo World Bounds Group name change.");
					haschanged = true;
					name.stringValue = n;

					property.serializedObject.ApplyModifiedProperties();
					WorldBoundsSO.EnsureNamesAreUnique();
					property.serializedObject.Update();
				}
			}

			r.xMin = lw;
			r.xMax = position.xMax;
			var res = property.FindPropertyRelative("resolution");
			var _res = EditorGUI.IntSlider(r, res.intValue, 1, 1000);
			if (res.intValue != _res)
			{
				Undo.RecordObject(property.serializedObject.targetObject, "Undo World Bounds Group value change.");
				haschanged = true;
				res.intValue = _res;
				property.serializedObject.ApplyModifiedProperties();
			}

			if (haschanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
				//AssetDatabase.SaveAssets();
			}

			//r = position;
			//r.yMin += 16;
			//EditorGUI.HelpBox(r, property.FindPropertyRelative("crusher"), MessageType.None);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}
	}

#endif
}
