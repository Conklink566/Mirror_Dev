﻿//Copyright 2018, Davin Carten, All rights reserved

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace emotitron.Compression
{

	[CustomPropertyDrawer(typeof(TransformCrusher))]
	[CanEditMultipleObjects]

	public class TransformCrusherDrawer : CrusherDrawer
	{
		private const float TITL_HGHT = 18f;
		private const float SET_PAD = 2;

		//bool haschanged;
		private static GUIContent gc = new GUIContent();
		
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			gc.text = label.text;
			gc.tooltip = label.tooltip;

			EditorGUI.BeginProperty(r, label, property);
			base.OnGUI(r, property, label);

			//property.serializedObject.ApplyModifiedProperties();
			//property.serializedObject.Update();

			//haschanged = true;
			// Hacky way to get the real object
			TransformCrusher target = (TransformCrusher)DrawerUtils.GetParent(property.FindPropertyRelative("posCrusher"));

			float currentline = r.yMin;

			SerializedProperty pos = property.FindPropertyRelative("posCrusher");
			SerializedProperty rot = property.FindPropertyRelative("rotCrusher");
			SerializedProperty scl = property.FindPropertyRelative("sclCrusher");
			SerializedProperty isExpanded = property.FindPropertyRelative("isExpanded");

			float ph = EditorGUI.GetPropertyHeight(pos);
			float rh = EditorGUI.GetPropertyHeight(rot);
			float sh = EditorGUI.GetPropertyHeight(scl);

			/// Header

			bool _isExpanded = EditorGUI.Foldout(new Rect(r.xMin, currentline, r.width, TITL_HGHT), isExpanded.boolValue, "");
			if (isExpanded.boolValue != _isExpanded)
			{
				isExpanded.boolValue = _isExpanded;
				property.serializedObject.ApplyModifiedProperties();
			}


			EditorGUI.LabelField(new Rect(r.xMin, currentline, r.width, TITL_HGHT), gc);// property.displayName /*new GUIContent("Transform Crusher " + label)*//*, (GUIStyle)"BoldLabel"*/);

			int totalbits = target.TallyBits();
			int frag0bits = Mathf.Clamp(totalbits, 0, 64);
			int frag1bits = Mathf.Clamp(totalbits - 64, 0, 64);
			int frag2bits = Mathf.Clamp(totalbits - 128, 0, 64);
			int frag3bits = Mathf.Clamp(totalbits - 192, 0, 64);

			string bitstr = frag0bits.ToString();
			if (frag1bits > 0)
				bitstr += "|" + frag1bits;
			if (frag2bits > 0)
				bitstr += "|" + frag2bits;
			if (frag3bits > 0)
				bitstr += "|" + frag3bits;

			bitstr = bitstr + " bits";
			EditorGUI.LabelField(new Rect(paddedleft, currentline, paddedwidth, 16), bitstr, miniLabelRight);

			if (isExpanded.boolValue)
			{
				
				/// TRS Element Boxes
				currentline += TITL_HGHT;
				//float leftConnectorY = currentline;

				DrawSet(r, currentline, ph, pos);
				currentline += ph + SET_PAD;

				DrawSet(r, currentline, rh, rot);
				currentline += rh + SET_PAD;

				DrawSet(r, currentline, sh, scl);
				currentline += sh /*+ SET_PAD*/;

				/// Connecting line between TRS Elements
				//SolidTextures.DrawTexture(new Rect(4, leftConnectorY + 4, 4, currentline - leftConnectorY), SolidTextures.lowcontrast2D);
				//EditorGUI.LabelField(new Rect(0, leftConnectorY + 4, 4, currentline - leftConnectorY - 12), GUIContent.none, (GUIStyle)"MiniSliderVertical");
			}


			EditorGUI.EndProperty();

		}


		private void DrawSet(Rect r, float currentline, float h, SerializedProperty prop)
		{
			//SolidTextures.DrawTexture(new Rect(4, currentline + 4, r.width, h - 12), SolidTextures.lowcontrast2D);
			//EditorGUI.LabelField(new Rect(2, currentline + 4, 4, h - 12), GUIContent.none, (GUIStyle)"MiniSliderVertical");

			EditorGUI.PropertyField(new Rect(r.xMin, currentline, r.width , h), prop);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float ph = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("posCrusher"));
			float rh = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rotCrusher"));
			float sh = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("sclCrusher"));
			SerializedProperty isExpanded = property.FindPropertyRelative("isExpanded");

			float body = SPACING + (isExpanded.boolValue ? (ph + rh + sh + SET_PAD * 2) : 0);
			return TITL_HGHT + body/* + BTTM_MARGIN*/;
		}
	}

}
#endif


