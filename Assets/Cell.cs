using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
	public Nullable<byte> Value = null;

	[Header("KTaNE Boilerplate")]
	public KMSelectable Me;
	public KMAudio Audio;

	[Header("Label in corner")]
	public TextMesh text;
	public MeshRenderer textRenderer;

	[Header("Main number")]
	public TextMesh number;

	public void Awake() {
		Me.OnInteract += OnClick;
	}

	public void SetText(string text) {
		this.text.text = text;
	}

	public void Clear() {
		this.textRenderer.enabled = true;
		this.Value = null;
		RedrawButton();
	}

	bool OnClick() {
		Audio.PlayGameSoundAtTransform(
			KMSoundOverride.SoundEffect.ButtonPress,
			this.transform
		);
		return false;
	}

	void RedrawButton() {
		this.number.text = this.Value == null ? "" : this.Value.ToString();
	}
}
