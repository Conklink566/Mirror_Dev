using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class JoinLeave : MonoBehaviour
{

	public KeyCode join = KeyCode.P;
	public KeyCode leave = KeyCode.O;


	void Update()
	{

#pragma warning disable CS0618 // UNET is obsolete

        if (Input.GetKeyDown(join))
        {
            //ClientScene.AddPlayer(0); //UNet Code
            ClientScene.AddPlayer();
        }

        if (Input.GetKeyDown(leave))
        {
            //ClientScene.RemovePlayer(0); //UNet Code
            ClientScene.RemovePlayer();
        }

#pragma warning restore CS0618 // UNET is obsolete

	}
}


