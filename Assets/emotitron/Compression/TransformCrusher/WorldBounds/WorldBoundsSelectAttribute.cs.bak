//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace emotitron.Compression
{
	public class WorldBoundsSelectAttributeAttribute : PropertyAttribute
	{

	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(WorldBoundsSelectAttributeAttribute))]
	public class StringListPopupAttributeDrawer : PropertyDrawer
	{
		private static GUIContent[] worldBoundsNames;
		public override void OnGUI(Rect r, SerializedProperty p, GUIContent label)
		{
			//WorldBoundsSelectAttributeAttribute target = (attribute as WorldBoundsSelectAttributeAttribute);
			
			/// Rebuild a list of the Group names for WorldMapBounds
			var worldBoundsSettings = WorldBoundsSO.Single.worldBoundsSettings;
			int cnt = worldBoundsSettings.Count;

			// If the names array doesn't exist or is the wrong size, scrap it and make one that is the correct size.
			if (worldBoundsNames == null || worldBoundsNames.Length != cnt)
			{
				worldBoundsNames = new GUIContent[cnt];
			}

			for (int i = 0; i < cnt; ++i)
			{
				worldBoundsNames[i] = new GUIContent(worldBoundsSettings[i].name);
			}

			r.height = 16;
			EditorGUI.LabelField(r, label, (GUIStyle)"MiniLabel");
			int idx = EditorGUI.Popup(r, new GUIContent(" "), p.intValue, worldBoundsNames);
			if (idx != p.intValue)
			{
				p.intValue = idx;
				p.serializedObject.ApplyModifiedProperties();
			}
			r.yMin += 16;
			r.height = 42;
			EditorGUI.LabelField(r, WorldBoundsSO.Single.worldBoundsSettings[p.intValue].BoundsReport(), (GUIStyle)"HelpBox");
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 56f;// base.GetPropertyHeight(property, label) * 3;
		}
	}
	#endif
}




