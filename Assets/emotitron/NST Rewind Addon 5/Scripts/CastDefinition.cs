//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Utilities.BitUtilities;
using emotitron.Utilities.GUIUtilities;
using emotitron.Utilities.GenericCast;
using emotitron.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.NST
{
	public enum Replicate { None, CastOnly, Hits, HitsWithHitGroups }

	/// <summary>
	/// This is the inspector element that creates a Rewind CastDefinition element in the NST.
	/// </summary>
	[System.Serializable]
	public class CastDefinition
	{
		public const int MAX_DEFINITIONS = 16;
		public const int MAX_REW_HITS = 32;

		public NetworkSyncTransform nst;

		public int id; // this is not reliable until after start
		public string name;
		public AuthorityModel authorityModel;
		/// <summary>
		/// Returns the castdef authority model selection. If set to default returns the NetLib default Authority Model setting.
		/// </summary>
		public AuthorityModel AuthorityModel {
			get
			{
				return (authorityModel == AuthorityModel.GlobalDefault) ? (AuthorityModel)NetLibrarySettings.single.defaultAuthority : authorityModel;
			}
		}

		[Tooltip("Object that cast and overlap tests will use as the origin. Leave this empty to use this gameobject as the castsource.")]
		public GameObject sourceObject; // Source of ray on prefab
		public GameObject ghostGO; // Ghost equivalent

		public Replicate ownerToAuthority = Replicate.Hits;
		public Replicate authorityToAll = Replicate.Hits;

		[Tooltip("This cast/overlap test will be done on the initiating client against colliders on these layers. "+
			"Exclude layers that won't include any NetworkSyncTransforms (such as walls) or objects with NSTs that you don't want to test.")]
		public LayerMask castHitMask;
		public bool useOffset;
		public Vector3 offset1; // Used for half-extents of box and first sphere of capsule
		public bool useOffset2;
		public Vector3 offset2; // Used for second sphere of capsule
		public Vector3 orientation;
		public CastType castType = CastType.Raycast;
		public float distance = 100f;
		public float radius;

		[Range(1, MAX_REW_HITS)]
		[Tooltip("The maximum number of hit NST object that can be transmitted. Non-NST objects are ignored.")]
		public int maxNstHits = 7;
		[HideInInspector] public int bitsNeededForTotal;
		[Tooltip("Enabling this will send a bitmask of a fixed size (determined by maxNstHits) rather than a sequence of hitIds. "+
			"Resulting bit sizes can be seen in the helpbox below.")]
		public bool sendHitsAsMask = false;

		[Tooltip("Show a ray in the editor when this is cast (fired). No need to turn this off for release builds, it is [Conditional]ly removed for you during compile.")]
		public bool showDebugRay = true;
		public bool reportToLog = true;

		[SerializeField] private float drawerHeight;

		public void Initialize(NetworkSyncTransform _nst, int _id, GameObject _sourceObject)
		{
			id = _id;
			nst = _nst;

			if (sourceObject == null)
				sourceObject = _sourceObject;

			Validate();

			bitsNeededForTotal = ((uint)maxNstHits).BitsNeededForMaxValue();
		}

		public void Validate()
		{
			if (NetLibrarySettings.Single.defaultAuthority == DefaultAuthority.OwnerAuthority)
				if (authorityToAll < Replicate.Hits)
				{
					XDebug.LogWarning("CastDefinition " + name + " has has Authority To All set to " + authorityToAll + ", which is invalid in an Onwer Authority environment - " +
						"as all clients need to be informed of hits in order to react to them. Using replication setting of 'Hits & Hit Groups' instead.", true, true);
					authorityToAll = Replicate.HitsWithHitGroups;
				}
		}

		public override string ToString()
		{
			return "Cast Definition [" + id + "] <b>'" + name + "'</b> on '" + nst.name + "'";
		}





	}
	

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(CastDefinition))]
	public class CastDefinitionDrawer : PropertyDrawer
	{
		public static GUIStyle lefttextstyle = new GUIStyle
		{
			alignment = TextAnchor.UpperLeft,
			richText = true
		};

		public override void OnGUI(Rect r, SerializedProperty _property, GUIContent _label)
		{
			EditorGUI.BeginProperty(r, GUIContent.none, _property);

			int startingIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			SerializedProperty name = _property.FindPropertyRelative("name");
			SerializedProperty authorityModel = _property.FindPropertyRelative("authorityModel");
			SerializedProperty sourceObject = _property.FindPropertyRelative("sourceObject");
			SerializedProperty ownerToAuthority = _property.FindPropertyRelative("ownerToAuthority");
			SerializedProperty authorityToAll = _property.FindPropertyRelative("authorityToAll");
			SerializedProperty castHitMask = _property.FindPropertyRelative("castHitMask");

			SerializedProperty useOffset = _property.FindPropertyRelative("useOffset");
			SerializedProperty offset1 = _property.FindPropertyRelative("offset1");
			SerializedProperty offset2 = _property.FindPropertyRelative("offset2");
			SerializedProperty castType = _property.FindPropertyRelative("castType");
			SerializedProperty distance = _property.FindPropertyRelative("distance");
			SerializedProperty radius = _property.FindPropertyRelative("radius");

			SerializedProperty orientation = _property.FindPropertyRelative("orientation");
			SerializedProperty drawerHeight = _property.FindPropertyRelative("drawerHeight");

			SerializedProperty maxNstHits = _property.FindPropertyRelative("maxNstHits");
			SerializedProperty sendHitsAsMask = _property.FindPropertyRelative("sendHitsAsMask");
			SerializedProperty showDebugRay = _property.FindPropertyRelative("showDebugRay");
			SerializedProperty reportToLog = _property.FindPropertyRelative("reportToLog");

			float currentline = r.yMin;
			CastType casttype = (CastType)castType.intValue;

			GUI.Box(new Rect(6, currentline, r.width + 8, r.height - 4), GUIContent.none, (GUIStyle)"IN ThumbnailShadow");
			GUI.Box(new Rect(6, currentline, r.width + 8, r.height - 4), GUIContent.none, (GUIStyle)"HelpBox");
			//GUI.Box(new Rect(4, currentline, r.width + 10, r.height - 4), GUIContent.none, "ProjectBrowserTextureIconDropShadow");
			//GUI.Box(new Rect(5, currentline + 1, r.width + 8, r.height - 2), GUIContent.none, "HelpBox");
			EditorGUI.DrawRect(new Rect(6 + 1, currentline + 1, r.width + 8 - 2, 24), new Color(.3f, .2f, .1f));

			currentline += 4;
			EditorGUI.LabelField(new Rect(13, currentline + 1, 100, 16), new GUIContent("<color=white><b>Cast Def Name:</b></color>"), lefttextstyle);

			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), name, new GUIContent(" "));
			if(name.stringValue.Trim() == "")
			{
				string tooltip = "You must name this in order to be able to reference it by code. Without a string name, you can still call this by id directly (castDefIds are assigned in the order in which they appear on the gameobject in the heirarchy) - but why?";
				EditorUtils.CreateErrorIconF(EditorGUIUtility.labelWidth - 4, currentline, tooltip);
			}

			currentline += 6;

			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), authorityModel);

			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), castType);

			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), castHitMask);

			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), sourceObject);

			if (NetLibrarySettings.Single.defaultAuthority != DefaultAuthority.OwnerAuthority)
			{
				//currentline += 17;
				//EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), ownerToAuthority);

				currentline += 17;
				GUIContent[] enumpop = /*(NetLibrarySettings.Single.defaultAuthority == DefaultAuthority.OwnerAuthority) ?*/
					new GUIContent[3] {
						new GUIContent ("Cast Only"),
						new GUIContent ("Hits"),
						new GUIContent ("Hits & Hit Groups")
					};
				Rect rec = new Rect(13, currentline, r.width - 4, 16);
				ownerToAuthority.intValue = EditorGUI.Popup(rec, new GUIContent("Owner To Auth"), ownerToAuthority.intValue - 1, enumpop) + 1;
			}


			currentline += 17;
			Rect rect = new Rect(13, currentline, r.width - 4, 16);

			// If we are running owner authority, then the the authority MUST send at least Hits - or else clients will never know they were hit
			if (NetLibrarySettings.Single.defaultAuthority == DefaultAuthority.OwnerAuthority)
			{
				GUIContent[] enumpop = /*(NetLibrarySettings.Single.defaultAuthority == DefaultAuthority.OwnerAuthority) ?*/
					new GUIContent[2] {
						new GUIContent ("Hits"),
						new GUIContent ("Hits & Hit Groups")
					};
				authorityToAll.intValue = EditorGUI.Popup(rect, new GUIContent(authorityToAll.displayName), authorityToAll.intValue - 2, enumpop) + 2;
			}
			else
			{
				EditorGUI.PropertyField(rect, authorityToAll);
			}

			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), maxNstHits);

			// Show dist for any of the casts
			if (casttype.IsCast())
			{
				currentline += 17;
				EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), distance);
			}

			// Radius types
			if (casttype.UsesRadius())
			{
				currentline += 17;
				EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), radius, new GUIContent("Radius:"));
			}

			// Offset (all but capsule)
			if (!casttype.IsCapsule())
			{
				currentline += 17;
				EditorGUI.LabelField(new Rect(13 + 16, currentline, 100, 16), new GUIContent("Offset:"));
				useOffset.boolValue = EditorGUI.Toggle(new Rect(13, currentline, 32, 16), useOffset.boolValue);

				if (useOffset.boolValue)
					EditorGUI.PropertyField(new Rect(13 + 100, currentline, r.width - 100 - 4, 16), offset1, GUIContent.none);

			}

			// Show Point1 and Point2 for capsule.
			if (casttype.IsCapsule())
			{
				currentline += 17;
				EditorGUI.LabelField(new Rect(13, currentline, 100, 16), new GUIContent("Offset1:"));
				EditorGUI.PropertyField(new Rect(13 + 100, currentline, r.width - 100 - 4, 16), offset1, GUIContent.none);

				currentline += 17;
				EditorGUI.LabelField(new Rect(13, currentline, 100, 16), new GUIContent("Offset2:"));
				EditorGUI.PropertyField(new Rect(13 + 100, currentline, r.width - 100 - 4, 16), offset2, GUIContent.none);

			}
			else if (casttype.IsBox())
			{
				currentline += 17;
				EditorGUI.LabelField(new Rect(13, currentline, 100, 16), new GUIContent("Half Extents:"));
				EditorGUI.PropertyField(new Rect(13 + 100, currentline, r.width - 100 - 4, 16), offset2, GUIContent.none);

				currentline += 17;
				EditorGUI.LabelField(new Rect(13, currentline, 100, 16), new GUIContent("Orientation:"));
				EditorGUI.PropertyField(new Rect(13 + 100, currentline, r.width - 100 - 4, 16), orientation, GUIContent.none);

			}

			//// Make sure the singleton exists.
			//NSTSettings.EnsureExistsInScene(NSTSettings.DEFAULT_GO_NAME);
			currentline += 8;
			// usemask bool
			if (HeaderSettings.Single.MaxNSTObjects <= 64)
			{
				currentline += 17;
				EditorGUI.PropertyField(new Rect(13, currentline, r.width - 4, 16), sendHitsAsMask, new GUIContent("Send As Mask:"));
			}

			currentline += 17;

			int bitsForHitcount = ((uint)maxNstHits.intValue).BitsNeededForMaxValue();
			uint bitsForNSTMask = HeaderSettings.single.MaxNSTObjects;
			int bitsPerHitmask = HitGroupSettings.Single.hitGroupTags.Count - 1;
			int maxBitsSentNSTs = maxNstHits.intValue * HeaderSettings.single.BitsForNstId;
			//int maxBitsSentNSTs = (((uint)maxNstHits.intValue).BitsNeededForMaxValue() + maxNstHits.intValue * NSTMasterSettings.single.bitsForNstId);

			string str = "Casts will send " +
				((HeaderSettings.single.MaxNSTObjects <= 64 && sendHitsAsMask.boolValue) ?
				// use mask
				bitsForNSTMask + " bits for hitmask \n(based on 'bitsForNstId' in NSTMasterSettings)." :
				// not use mask
				(bitsForHitcount + " bits for the hitcount, \n") +
				"plus " + HeaderSettings.single.BitsForNstId + " per hit w/ max " + maxBitsSentNSTs + " bits (for " + maxNstHits.intValue + " hits).") +
				
				((ownerToAuthority.intValue == (int)Replicate.HitsWithHitGroups) ? "\nplus " + bitsPerHitmask + " for hitgroup mask from clients." : "") +
				((authorityToAll.intValue == (int)Replicate.HitsWithHitGroups) ? "\nplus " + bitsPerHitmask + " for hitgroup mask from authority source." : "")
				;

			EditorGUI.HelpBox(new Rect(11, currentline, r.width - 4, 52), str, MessageType.None);

			currentline += 54;

			EditorGUI.PropertyField(new Rect(13, currentline, r.width, 17), showDebugRay);
			currentline += 17;
			EditorGUI.PropertyField(new Rect(13, currentline, r.width, 17), reportToLog);
			currentline += 17;

			currentline += 8 - r.yMin;
			drawerHeight.floatValue = currentline;
			EditorGUI.indentLevel = startingIndent;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty drawerHeight = property.FindPropertyRelative("drawerHeight");

			return drawerHeight.floatValue + 4;  // assuming original is one row
		}
	}

	//this line fixes EditorGUI flickering bug for the rects
	[CanEditMultipleObjects] [CustomEditor(typeof(MonoBehaviour), true)] public class MonoBehaviour_DummyCustomEditor : Editor { }

#endif
}





