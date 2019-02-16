//Copyright 2018, Davin Carten, All rights reserved
using emotitron.Debugging;

using UnityEngine;
using emotitron.InputSystem;
using emotitron.Utilities.Pooling;
using emotitron.Utilities.GenericCast;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace emotitron.NST
{
	[HelpURL("https://docs.google.com/document/d/12RvQsfKpYFmhPDaYMp_skxxghGOPXK55qKRlKiifZVU/edit#bookmark=id.5nnhlr17n03i")]

	public class NSTCastDefinition : NSTRewindComponent, INstCast, INstCastResults
	{
		public CastDefinition castDefinition = new CastDefinition()
		{
			name = "Unnamed",
			castType = CastType.Raycast,
			sourceObject = null,
			authorityToAll = Replicate.HitsWithHitGroups,
			castHitMask = 63,
			distance = 1000f,
			radius = 5f,
			offset1 = new Vector3(0, 0, 0),
			offset2 = new Vector3(1, 1, 1),
			orientation = new Vector3(0,0,0)
		};
		
		[Space]
		[Tooltip("This prefab will be added to a pool, and will be spawned whenever a cast or result event is triggered.")]
		[SerializeField]
		public GameObject graphicPrefab;

		[Tooltip("Creates a placeholder graphic if a GraphicPrefab is not assigned.")]
		public bool usePlaceholder = true;

		[Tooltip("Automatically hide and return the cast graphic prefab to the pool after X seconds. 0 to not automatically return it to the pool (you must disable it yourself)")]
		[Range(0,10)]
		public float prefabLifespan = 1f;
		
		[Tooltip("Which timing segment to apply this cast result on. Ideal will usually be OnEndInterpolate, as that is where clients will have finished interpolating to the position that the cast event happened at. Rcv and Start can be used if more immediate results are needed.")]
		[SerializeField] public ApplyTiming applyTiming = ApplyTiming.OnEndInterpolate;

		[Tooltip("If a component with IVitals exists on the root of hit object, this much damage will be applied it. 0 disables searching for IVitals.")]
		public int baseDamage = 20;

		[HideInInspector]
		public InputSelectors triggers = new InputSelectors(KeyCode.Space);

		[HideInInspector]
		[SerializeField]
		public UnityEventFrameCastDef onCastCallbacks;

		[HideInInspector]
		[SerializeField]
		public UnityEventCastResults onResultCallbacks;

		public override void OnNstPostAwake()
		{
			base.OnNstPostAwake();
			castDefinition.nst = nst;

			if (usePlaceholder && !graphicPrefab)
				graphicPrefab = castDefinition.CreateCastMeshGraphic();

			if (graphicPrefab)
				Pool.AddPrefabToPool(graphicPrefab);
		}

		public void Update()
		{
			/// TODO: nst. not needed most likely
			if (nst.na.IsMine == false)
			{
				enabled = false;
				return;
			}

			for (int i = 0; i < triggers.selectors.Count; i++)
				if (triggers.selectors[i].Test())
				{
					nstRewindEngine.NetworkCast(castDefinition.id);
					return;
				}
		}

		/// <summary>
		/// Initiate the cast process using the castDefinition defined for this NSTCastDefinition component.
		/// </summary>
		public virtual void NetworkCast()
		{
			nstRewindEngine.NetworkCast(castDefinition.id);
		}

		//TODO make interface callbacks a list in nstRewind... and permanently remove this interfaces from the list if unused.

		public void OnCast(Frame frame, CastDefinition castdef)
		{
			if (castdef.id != castDefinition.id)
				return;

			XDebug.DrawRay(castdef.sourceObject.transform.position, castdef.sourceObject.transform.forward * 100, Color.red, 1f, castdef.showDebugRay);
			XDebug.Log(("<b>OnCast </b>Graphic Fired On Owner: " + castdef + "\n<i>Disable this report by unchecking 'Report To Log' on NSTCastDefinition.</i>"), true, castdef.reportToLog);

			onCastCallbacks.Invoke(frame, castdef);
			
			CastGraphic(castdef, castdef.ghostGO.transform.position, castdef.ghostGO.transform.rotation);
		}

		/// <summary>
		/// Callback from RewindEngine with results of an incoming NetworkCast.
		/// </summary>
		public void OnCastResults(CastResults results, ApplyTiming callbackTiming)
		{
			// Ignore this cast result if the timing isn't the one we are interested in (arrive, startinterp, endinterp)
			// Unless this the owner (the server owns this caster), in which case we want to let it through - 
			// as it only fires on Rcv after doing the test and happens in real time an not on the frame buffer.

			if (results.CastDef.id != castDefinition.id)
				return;

			if (!na.IsMine && // is mine skip, will have done its action OnCast - TODO: can make this an inspector option later
				callbackTiming != applyTiming && // and is not the correct apply timing
				results.frame.frameid != nst.frameCount)  // and is not offtick frame (offtick only fires on RCV and should be processed as it never interpolates
				return;

			// Trigger any functions in the inspector list.
			onResultCallbacks.Invoke(results, callbackTiming);

			if (!nst.na.IsMine)
				CastGraphic(results.CastDef);

			if (baseDamage != 0)
				ApplyResultsToIVitals(results);

			XDebug.Log("<b>Cast Results:</b> " + results  + "\n<i>Disable this report by unchecking 'Report To Log' on NSTCastDefinition.</i>", castDefinition.reportToLog, true);
		}

		/// <summary>
		/// See if the hit objects have any IVitals interfaces, if so apply damage to them if applicable for this client/auth model
		/// </summary>
		/// <param name="results"></param>
		private void ApplyResultsToIVitals(CastResults results)
		{
			List<int> hgmasks = results.hitGroupMasks;
			int hgmasksCount = hgmasks.Count;

			// If we are using client authority, damage is applied when the owner receives this. SUPER suseptible to cheating of course.
			if (NetLibrarySettings.single.defaultAuthority == DefaultAuthority.OwnerAuthority /*results.castDef.AuthorityModel  == AuthorityModel.OwnerAuthority */ 
				&& castDefinition.authorityToAll > Replicate.CastOnly)
			{
				List<NetworkSyncTransform> nsts = results.nsts;

				int count = results.nsts.Count;
				for (int i = 0; i < count; ++i)
				{
					HealthSystem.IVitals health = nsts[i].GetComponent<HealthSystem.IVitals>();

					if (health != null && health.NA.IAmActingAuthority)
						health.ApplyDamage(baseDamage, (hgmasksCount == 0) ? 1 : hgmasks[i]);
				}
			}
			// If this is the server and a health interface exists - report the hit to the health interface
			else if (results.CastDef.AuthorityModel == AuthorityModel.ServerAuthority) // na.IAmActingAuthority(results.castDef.authorityModel)) // MasterNetAdapter.ServerIsActive)
			{
				List<NetworkSyncTransform> nsts = results.nsts;

				int count = results.nsts.Count;
				for (int i = 0; i < count; ++i)
				{
					HealthSystem.IVitals health = nsts[i].GetComponent<HealthSystem.IVitals>();

					if (health != null)
						health.ApplyDamage(baseDamage, (hgmasksCount == 0) ? 1 : hgmasks[i]);
				}
			}
			//else
			//{
			//	XDebug.LogWarning("Does not have authority to deal damage. " + results.CastDef.AuthorityModel + "  replicate:" + castDefinition.ownerToAuthority + "  authmod: " + castDefinition.AuthorityModel);
			//}
		}

		private void CastGraphic(CastDefinition castDef, Vector3 lossyPosition, Quaternion lossyRotation)
		{
			if (graphicPrefab)
				//Pool.Spawn(graphicPrefab, castDef.sourceObject.transform, prefabLifespan);
				Pool.Spawn(graphicPrefab, lossyPosition, lossyRotation, prefabLifespan);
		}

		private void CastGraphic(CastDefinition castDef)
		{
			if (graphicPrefab)
				Pool.Spawn(graphicPrefab, castDef.sourceObject.transform, prefabLifespan);
		}



		///// <summary>
		///// If no projectile prefab was entered in the inspector, Instantiate a sphere as a placeholder.
		///// </summary>
		//private static GameObject CreateCastMeshGraphic(CastDefinition cd)
		//{

		//	CastType castType = cd.castType;
		//	GameObject empty = new GameObject("Placeholder Cast Graphic");
		//	GameObject beam = null;

		//	// Create ray graphic
		//	if (castType == CastType.Raycast || castType == CastType.BoxCast || castType == CastType.CapsuleCast)
		//	{
		//		beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

		//		beam.transform.parent = empty.transform;

		//		float dist = Mathf.Max(cd.distance, 1000f);
		//		beam.transform.localEulerAngles = new Vector3(90, 0, 0);
		//		beam.transform.localScale = new Vector3(.1f, dist, .1f);
		//		beam.transform.localPosition = new Vector3(0, 0, dist);

		//		Destroy(beam.GetComponent<Collider>());
		//	}

		//	// Create overlap graphic
		//	else if (castType == CastType.OverlapBox || castType == CastType.OverlapSphere)
		//	{
		//		beam = GameObject.CreatePrimitive(
		//			castType == CastType.OverlapBox ? PrimitiveType.Cube : PrimitiveType.Sphere);

		//		beam.transform.parent = empty.transform;

		//		float d = Mathf.Max(cd.radius * 2, 40f);
		//		beam.transform.localEulerAngles = castType == CastType.OverlapBox ? cd.orientation : new Vector3(0, 0, 0);

		//		beam.transform.localScale =
		//			castType == CastType.OverlapBox		? cd.offset1 * 2 :
		//			castType == CastType.OverlapSphere	? new Vector3(d, d, d) :
		//			new Vector3(1, 1, 1); // not used.

		//		beam.transform.localPosition = cd.useOffset ? cd.offset1 : new Vector3(0, 0, 0);

		//		Destroy(beam.GetComponent<Collider>());
		//	}

		//	// Create overlap graphic
		//	else if (castType == CastType.OverlapCapsule)
		//	{
		//		CreateChildSphere(empty, cd.offset1, cd.radius);
		//		CreateChildSphere(empty, cd.offset2, cd.radius);
		//		CreateCylinderBetweenTwoPoints(empty, cd.offset1, cd.offset2, cd.radius);
		//	}

		//	empty.SetActive(false);
		//	return empty;
		//}

		//private static void CreateCylinderBetweenTwoPoints(GameObject par, Vector3 start , Vector3 end, float radius)
		//{
		//	GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		//	cyl.transform.parent = par.transform;
		//	Destroy(cyl.GetComponent<Collider>());

		//	float length = (end - start).magnitude;
		//	Vector3 pos = start + (end - start) / 2f;
		//	Vector3 dir = (end - start).normalized;

		//	cyl.transform.localScale = new Vector3(radius * 2 , length / 2, radius * 2 );
		//	cyl.transform.localPosition = pos;
		//	cyl.transform.up = dir;

		//}

		//private static void CreateChildSphere (GameObject par, Vector3 pos, float radius)
		//{
		//	GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		//	Destroy(ball.GetComponent<Collider>());

		//	ball.transform.parent = par.transform;
		//	ball.transform.localPosition = pos;
		//	float d = radius * 2;
		//	ball.transform.localScale = new Vector3(d, d, d);
		//}

	}

	/* ---------------------------------  */
	/* ----------  EDITOR CODE ---------  */
	/* ---------------------------------  */

#if UNITY_EDITOR

	[CustomEditor(typeof(NSTCastDefinition))]
	[CanEditMultipleObjects]
	public class NSTCastDefinitionEditor : NSTRewindComponentEditor
	{
		NSTCastDefinition nstcd;
		CastDefinition cd;

		SerializedProperty onResultCallbacks;
		SerializedProperty onCastCallbacks;

		ReorderableList inputList;

		public override void OnEnable()
		{
			base.OnEnable();

			nstcd = (NSTCastDefinition)target;
			cd = nstcd.castDefinition;

			if (RewindSettings.Single.autoAddRewindEngine)
				nstcd.nstRewindEngine = NSTRootSingleton<NSTRewindEngine>.EnsureExistsOnRoot(nstcd.transform);

			onResultCallbacks = serializedObject.FindProperty("onResultCallbacks");
			onCastCallbacks = serializedObject.FindProperty("onCastCallbacks");
		}

		bool showTriggers = true;
		bool showCastCallbacks = true;
		bool showResultCallbacks = true;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();

			showTriggers = EditorGUILayout.Foldout(showTriggers, "Triggers");

			if (showTriggers)
			{
				if (GUI.Button(EditorGUILayout.GetControlRect(), "Copy Trigger Code", (GUIStyle)"toolbarbutton"))
					EditorGUIUtility.systemCopyBuffer = CodeSnippetTrigger();

				EditorGUILayout.PropertyField(serializedObject.FindProperty("triggers"));
				EditorGUILayout.Space();
			}

			showCastCallbacks = EditorGUILayout.Foldout(showCastCallbacks, "OnCast Callbacks");

			if (showCastCallbacks)
			{
				EventBlock("OnCast", onCastCallbacks, null, CodeSnippetOnCastCallback);
			}

			EditorGUILayout.Space();

			showResultCallbacks = EditorGUILayout.Foldout(showResultCallbacks, "OnResult Callbacks");

			if (showResultCallbacks)
			{
				string tooltip = "When the results will be applied. Hits can be applied immediately as they arrive on the server/other clients, " +
				"or as they come off the buffer in sync with the sourceObject. EndInterpolate should be used if you want cast graphics to be visually in sync with the source object.";

				EventBlock("OnResults", onResultCallbacks, null, CodeSnippetResults, tooltip);
			}
			EditorGUILayout.Space();

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

			RewindSettings.Single.DrawGui(target, true, false);
		}

		private void EventBlock(string title, SerializedProperty callbacks, SerializedProperty timing, SnippetDelegate snippetgen, string timingtooltip = "")
		{
			// Label and code snippet
			Rect r = EditorGUILayout.GetControlRect();

			if (GUI.Button(r, "Copy "+ title +" Code Snippet", (GUIStyle)"toolbarbutton"))
			{
				EditorGUIUtility.systemCopyBuffer = snippetgen();
			}
			// Event selector
			EditorGUILayout.PropertyField(callbacks);
			EditorGUILayout.Space();
		}

		// The really ugly code snippets....

		private delegate string SnippetDelegate();

		private string CodeSnippetTrigger()
		{
			string intName = "_" + cd.name.Replace(" ", "") + "Id";

			return
				"//using emotitron.NST; \r\n" +
				"\r\n" + 
				"NSTRewindEngine nstRewindEngine;  \r\n" +
				"int " + intName + "; \r\n" +
				"\r\n" + 
				"// Cache the RewindEngine component and CastDefinition. \r\n" +
				"void Start() \r\n" +
				"{ \r\n" +
				"	nstRewindEngine = GetComponent<NSTRewindEngine>();   \r\n" +
				"	" + intName + " = nstRewindEngine.CastDefIdLookup(\"" + cd.name + "\"); \r\n" +
				"} \r\n" +
				"\r\n" +
				"// Rename this method, or use the NetworkCast call inside wherever you want to fire this cast from. \r\n" +
				"public void MY_METHOD_TO_FIRE_CAST() \r\n" +
				"{ \r\n" +
				"	nstRewindEngine.NetworkCast(" + intName + "); \r\n" +
				"} \r\n";
		}

		private string CodeSnippetOnCastCallback()
		{
			string idVarName = "_" + StringTools.RemoveAllNonAlphaNumeric(cd.name);

			return
				"//using emotitron.NST; \r\n" +
				"\r\n" +
				"// This method can be called by adding the interface INSTCast to your monobehavior like so: \r\n" +
				"// public class MyClass : Monobehaviour, INSTCast {} \r\n" +
				"\r\n" +
				"\r\n" +
				"// or by adding it to the OnCast callback list on the NSTCastDefinition component you just copied this from. \r\n" +
				"\r\n" +
				"// This is called on the initiating owner (usually the player) only. Any client side feedback to the user should happen here,\r\n" +
				"// such as weapon fire graphics and audio. This is called immediately, before the serer is notified and has confirmed\r\n" +
				"// so any actions here are cosmetic in nature. Use the OnResults callback instead to handle confirmed results.\r\n" +
				"int " + idVarName + "CastId = -1;\r\n" +
				"public void OnCast(Frame frame, CastDefinition castDef)\r\n" +
				"{\r\n" +
				"	// Cache the name lookup to avoid this mess every time you cast.\r\n" +
				"	if (" + idVarName + "CastId == -1)\r\n" +
				"		" + idVarName + "CastId = castDef.nst.GetComponent<NSTRewindEngine>().CastDefLookup(\"" + cd.name + "\").id;\r\n" +
				"\r\n" +
				"	// only check the castDef we are interested in (only used if we are using the interface callback)  \r\n" +
				"	// You can make your own cast id checks if you want this method to handle more than one cast definition.  \r\n" +
				"	if (castDef.id != " + idVarName + "CastId)\r\n" +
				"		return;\r\n" +
				"\r\n" + 
				"	Vector3 srcPos = castDef.sourceObject.transform.position;\r\n" +
				"	Quaternion srcRot = castDef.sourceObject.transform.rotation;\r\n" +
				"	\r\n" +
				"	\r\n" +
				"	// Put any immediate owner specific feedback actions here. \r\n" +
				"	Debug.DrawRay(srcPos, srcRot * new Vector3(0, 0, 100), Color.green, 1f);\r\n" +
				"}\r\n";
		}

		private delegate void FrameDelegate(CastResults results);

		private string CodeSnippetResults()
		{
			// Get the name of the appropriate interface based on the timing dropdown selection (complicated here so that name changes don't break string literals)
			string interfaceName = "INstCastResults";
			
			string methodName = "OnCastResults";
			
			string idVarName = "_" + StringTools.RemoveAllNonAlphaNumeric(cd.name);
			string applyTimingStr = Enum.GetName(typeof(ApplyTiming), nstcd.applyTiming);

			return
				"//using emotitron.NST;\r\n" +
				"//using System.Collections.Generic;\r\n" +
				"\r\n" +
				"/// This method can be called by adding the interface " + interfaceName + " to your monobehavior like so: \r\n" +
				"// public class MyClass : Monobehaviour, " + interfaceName + " \r\n" +
				"/// or by calling this method by adding it to the OnResults callback list on the NSTCastDefinition component you copied this code from. \r\n" +
				"\r\n" +
				"int " + idVarName + "CastId = -1;\r\n" +
				"public void " + methodName + "(CastResults castresults, ApplyTiming applyTiming)\r\n" +
				"{\r\n" +
				"	/// Change this value if you want to respond to cast results at some timing other than "+ applyTimingStr + ".\r\n" +
				"	if (applyTiming != ApplyTiming."+ applyTimingStr + ")    \r\n" +
				"		return;   \r\n" +
				"\r\n" +
				"	/// Cache the name lookup to avoid this mess every time you cast.\r\n" +
				"	if (" + idVarName + "CastId == -1)\r\n" +
				"		" + idVarName + "CastId = castresults.CastDef.nst.GetComponent<NSTRewindEngine>().CastDefLookup(\"" + cd.name + "\").id;\r\n" +
				"\r\n" +
				"	/// only check the castDef we are interested in (only used if we are using the interface callback)  \r\n" +
				"	/// You can make your own cast id checks if you want this method to handle more than one cast definition.  \r\n" +
				"	if (castresults.CastDef.id != "+ idVarName + "CastId)\r\n" +
				"		return;\r\n" +
				"\r\n" +
				"	/// The number of rewind layer tags to iterate through. Unconfirmed hits don't include any hitbox group info and is a raw list  \r\n" +
				"	/// of NSTs that the owner claimed to have hit. Unconfirmed hits will be stored in hits[0], confirmed hits will be sorted by hit[hitboxLayer]   \r\n" +
				"	/// The Hitbox Layers are defined in NSTRewindSettings and colliders are assigned to Hitbox Layers using the NSTHitGroup component.\r\n" +
				"	int layercount = (castresults.isConfirmed) ? HitGroupSettings.single.hitGroupTags.Count : 1;\r\n" +
				"	List<NetworkSyncTransform> hits = castresults.nsts;\r\n" +
				"	List<int> hitGroupMasks = castresults.hitGroupMasks;\r\n" +
				"\r\n" +
				"	/// Some bool values you can use to determine how to respond to this event differently for owner/server/others.\r\n" +
				"	bool isMaster = MasterNetAdapter.ServerIsActive;\r\n" +
				"	bool isClient = MasterNetAdapter.ClientIsActive;\r\n" +
				"	bool isMine = na.IsMine;\r\n" +
				"	bool isActingAuthority = na.IAmActingAuthority;\r\n" +
				"\r\n" +
				"	/// Iterate through all of the NSTs indicated as having been hit in CastResults.\r\n" +
				"	for (int i = 0; i < hits.Count; i++)\r\n" +
				"	{\r\n" +
				"		Debug.Log(\" Hit: \" + hits[i].name + \" on Hit Groups \" + hitGroupMasks);\r\n" +
				"		/// Here is a sample tree for responding to this event.\r\n" +
				"		if (isActingAuthority)\r\n" +
				"		{\r\n" +
				"			/// Place authority specific handlers here, such as calculating damage from hits\r\n" +
				"		}\r\n" +
				"		if (isClient)\r\n" +
				"		{\r\n" +
				"			/// Place specific code for other clients here\r\n" +
				"			if (isMine)\r\n" +
				"			{\r\n" +
				"				/// Place owner only code here, such as corrections if OnCast did something on the owner you want to undo\r\n" +
				"			}\r\n" +
				"			else\r\n" +
				"			{\r\n" +
				"				/// Place code that should run on clients other than the owner.\r\n" +
				"			}\r\n" +
				"		}\r\n" +
				"	}\r\n" +
				"}\r\n";





		}
	}

#endif

}



