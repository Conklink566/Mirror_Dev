﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.NST
{
	/// <summary>
	///  Super simplistic Teleport test component. Hitting T will teleport this NST networked object to the center of the world bounds.
	/// </summary>
	public class NSTTeleportOnT : MonoBehaviour
	{

		NetworkSyncTransform nst;

		private void Awake()
		{
			nst = GetComponent<NetworkSyncTransform>();
		}
		// Update is called once per frame
		void Update()
		{
			if (!nst.na.IsMine)
				return;

			if (Input.GetKeyDown(KeyCode.T))
			{
				Debug.Log("Teleporting to " + WorldCompressionSettings.globalPosCrusher.bounds.center);
				nst.Teleport(WorldCompressionSettings.globalPosCrusher.Bounds.center, Quaternion.identity);
			}
		}
	}
}




