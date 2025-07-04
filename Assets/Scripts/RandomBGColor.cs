﻿using System;
using UnityEngine;

// Token: 0x02000035 RID: 53
public class RandomBGColor : MonoBehaviour
{
	// Token: 0x0600010F RID: 271 RVA: 0x00006A08 File Offset: 0x00004C08
	private void Awake()
	{
		this.camera = base.GetComponentInChildren<Camera>();
	}

	// Token: 0x06000110 RID: 272 RVA: 0x00006A18 File Offset: 0x00004C18
	private void RandomColor()
	{
		Color backgroundColor = this.colors[UnityEngine.Random.Range(0, this.colors.Length)];
		this.camera.backgroundColor = backgroundColor;
	}

	// Token: 0x06000111 RID: 273 RVA: 0x00006A4B File Offset: 0x00004C4B
	private void OnEnable()
	{
		this.RandomColor();
	}

	// Token: 0x0400012C RID: 300
	private Camera camera;

	// Token: 0x0400012D RID: 301
	private Color[] colors = new Color[]
	{
		new Color(1f, 0.65f, 0.4f),
		new Color(1f, 0.4f, 0.41f),
		new Color(1f, 0.4f, 0.66f),
		new Color(0.95f, 0.48f, 1f),
		new Color(0.45f, 0.45f, 1f),
		new Color(0.316f, 0.7123f, 1f),
		new Color(0.35f, 1f, 0.48f)
	};
}
