﻿using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200001A RID: 26
public class GameController : NetworkBehaviour
{
	
	// Token: 0x1700000A RID: 10
	// (get) Token: 0x0600008B RID: 139 RVA: 0x00005210 File Offset: 0x00003410
	// (set) Token: 0x0600008C RID: 140 RVA: 0x00005218 File Offset: 0x00003418
	public Transform startPos { get; set; }

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x0600008D RID: 141 RVA: 0x00005221 File Offset: 0x00003421
	// (set) Token: 0x0600008E RID: 142 RVA: 0x00005229 File Offset: 0x00003429
	public GameObject currentCar { get; set; }

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x0600008F RID: 143 RVA: 0x00005232 File Offset: 0x00003432
	// (set) Token: 0x06000090 RID: 144 RVA: 0x0000523A File Offset: 0x0000343A
	public bool playing { get; set; }

	public bool inputFieldSelected = false;
	// Token: 0x06000091 RID: 145 RVA: 0x00005244 File Offset: 0x00003444
	private void Awake()
	{
		GameController.Instance = this;
		Time.timeScale = 1f;
		this.startPos = this.checkPoints.GetChild(0);
		if (GameState.Instance.gamemode != Gamemode.Multiplayer) { 
			//Setup non-multiplayer car
			currentCar = UnityEngine.Object.Instantiate<GameObject>(PrefabManager.Instance.cars[GameState.Instance.car], startPos.position, startPos.rotation);
			currentCar.GetComponent<CarSkin>().SetSkin(GameState.Instance.skin);
			SetCurrentCar(currentCar);
            Invoke("StartRace", startTime);
        }
    }
	// Token: 0x06000092 RID: 146 RVA: 0x000052D4 File Offset: 0x000034D4
	public void SetCurrentCar(GameObject car)
	{
		this.currentCar = car;
		CameraController.Instance.AssignTarget(this.currentCar.transform);
		ShakeController.Instance.car = this.currentCar.GetComponent<Car>();
		ReplayController.Instance.car = this.currentCar.GetComponent<Car>();
		this.currentCar.AddComponent<CheckpointUser>();
	}
	
	public void doLights()
	{
		UIManager.Instance.DisableMultiplayerBanner();
		GameObject.Find("startLight").GetComponent<StartLight>().StartLights();
		GameObject.Find("startLight (1)").GetComponent<StartLight>().StartLights();

        Invoke("StartRace", startTime);
    }
	// Token: 0x06000093 RID: 147 RVA: 0x0000532C File Offset: 0x0000352C
	public void StartRace()
	{
		
		this.playing = true;
		Timer.Instance.StartTimer();
	}

	// Token: 0x06000094 RID: 148 RVA: 0x0000533F File Offset: 0x0000353F
	private void Update()
	{
		this.PlayerInput();
	}

	public void SetJoinCode(string code) 
	{
		this.joinCode = code;
		this.joinCodeTextField.text = code;

	}

	// Token: 0x06000095 RID: 149 RVA: 0x00005348 File Offset: 0x00003548
	private void PlayerInput()
	{
		if (base.IsInvoking("ShowFinishScreen"))
		{
			if (Input.GetButtonDown("Cancel"))
			{
				base.CancelInvoke("ShowFinishScreen");
				this.ShowFinishScreen();
			}
			return;
		}
		if (Input.GetButtonDown("Cancel") && !Pause.Instance.paused)
		{
			Pause.Instance.TogglePause();
		}
	}

	// Token: 0x06000096 RID: 150 RVA: 0x000053A4 File Offset: 0x000035A4
	public void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	// Token: 0x06000097 RID: 151 RVA: 0x000053C4 File Offset: 0x000035C4
	public void Recover()
	{
		CheckpointUser component = this.currentCar.GetComponent<CheckpointUser>();
		if (!component)
		{
			return;
		}
		MonoBehaviour.print("cur check: " + component.GetCurrentCheckpoint(this.finalCheckpoint == 0));
		Transform child = this.checkPoints.GetChild(component.GetCurrentCheckpoint(this.finalCheckpoint == 0));
		this.currentCar.GetComponent<Rigidbody>().velocity = Vector3.zero;
		this.currentCar.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		this.currentCar.transform.rotation = child.rotation;
		this.currentCar.transform.position = child.position;
	}
	public void LoseRace()
	{
		if (this.victory) { return; }
		this.victory = false;
		this.playing = false;
		Time.timeScale = 0.3f;
		base.Invoke("ShowFinishScreen", 1f);
		if (this.endCamera) 
		{
			this.endCamera.target = this.currentCar.transform;
			this.endCamera.gameObject.SetActive(true);
			CameraController.Instance.gameObject.SetActive(false);
		}

	}
	[ServerRpc(RequireOwnership =false)]
	public void IFinishedServerRpc(ServerRpcParams rpcParams = default)
	{
		ulong triggerlingClientId = rpcParams.Receive.SenderClientId;
		SomeoneFinishedClientRpc(triggerlingClientId, new ClientRpcParams());
	}
	[ClientRpc]
	public void SomeoneFinishedClientRpc(ulong triggeringClientId,ClientRpcParams rpcParams = default)
	{
		Debug.Log("Someone else finished");
		if (NetworkManager.Singleton.LocalClientId == triggeringClientId)
		{
			return;
		}
		LoseRace();
	}
	// Token: 0x06000098 RID: 152 RVA: 0x0000547C File Offset: 0x0000367C
	public void FinishRace(bool win, Transform car)
	{
		if (!this.playing)
		{
			return;
		}
		
		this.victory = win;
		this.playing = false;
		Time.timeScale = 0.3f;
		base.Invoke("ShowFinishScreen", 1f);
		if (this.endCamera)
		{
			this.endCamera.target = car;
			this.endCamera.gameObject.SetActive(true);
			CameraController.Instance.gameObject.SetActive(false);
		}
		//broadcast the event to all other clients 
		if (GameState.Instance.gamemode == Gamemode.Multiplayer)
		{
			IFinishedServerRpc();
		}
	}

	// Token: 0x06000099 RID: 153 RVA: 0x000054F4 File Offset: 0x000036F4
	public void ShowFinishScreen()
	{
		FinishController.Instance.Open(this.victory);
	}

	// Token: 0x040000C3 RID: 195
	public Transform path;
	public int playerCount = 0; 

	// Token: 0x040000C4 RID: 196
	public Transform checkPoints;

	// Token: 0x040000C5 RID: 197
	public LookAtTarget endCamera;

	// Token: 0x040000C6 RID: 198
	public int finalCheckpoint;

	// Token: 0x040000C8 RID: 200
	public float startTime = 1.5f;

	// Token: 0x040000C9 RID: 201
	public static GameController Instance;

	private string joinCode;
	[SerializeField] private TextMeshProUGUI joinCodeTextField;

	// Token: 0x040000CA RID: 202
	private bool victory;
}
