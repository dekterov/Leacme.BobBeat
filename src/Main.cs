// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Main : Spatial {

	public AudioStreamPlayer Audio { get; } = new AudioStreamPlayer();
	private AudioEffectPitchShift pitchEffect;
	private PackedScene drumScene = GD.Load<PackedScene>("res://scenes/Drum.tscn");

	private void InitSound() {
		if (!Lib.Node.SoundEnabled) {
			AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
		}
	}

	public override void _Notification(int what) {
		if (what is MainLoop.NotificationWmGoBackRequest) {
			GetTree().ChangeScene("res://scenes/Menu.tscn");
		}
	}

	public override void _Ready() {
		GetNode<WorldEnvironment>("sky").Environment.BackgroundColor = new Color(Lib.Node.BackgroundColorHtmlCode);
		InitSound();
		AddChild(Audio);

		pitchEffect = (AudioEffectPitchShift)AudioServer.GetBusEffect(AudioServer.GetBusIndex("Master"), 0);

		for (int x = 0; x < 9; x++) {
			for (int z = 0; z < 5; z++) {
				var drum = (Spatial)drumScene.Instance();
				drum.Translation = new Vector3(x, 0, z);
				AddChild(drum);

				var drumPhysics = drum.GetNode<StaticBody>("DrumPhysics");
				var drumZ = drum.Transform.origin.z;
				var drumX = drum.Transform.origin.x;

				AudioStream sound = null;

				switch (drumZ) {
					case 0:
						sound = GD.Load<AudioStream>("res://assets/drum.wav");
						break;
					case 1:
						sound = GD.Load<AudioStream>("res://assets/bass.wav");
						break;
					case 2:
						sound = GD.Load<AudioStream>("res://assets/pop.wav");
						break;
					case 3:
						sound = GD.Load<AudioStream>("res://assets/synth.wav");
						break;
					case 4:
						sound = GD.Load<AudioStream>("res://assets/cymbal.wav");
						break;
				}

				drumPhysics.Connect("input_event", this, nameof(OnDrumButton), new Godot.Collections.Array { drum, sound });
			}
		}
	}

	private void OnDrumButton(Node camera, InputEvent @event, Vector3 click_pos, Vector3 click_normal, int shape_idx, Spatial drum, AudioStream sound) {
		if (@event is InputEventScreenTouch te && te.Pressed) {

			var drumMesh = drum.GetNode<MeshInstance>("DrumPhysics/DrumCollision/Drum");

			drumMesh.MaterialOverride = new SpatialMaterial() {
				EmissionEnabled = true,
				Emission = Color.FromHsv(GD.Randf(), 1, 1),
				EmissionEnergy = 0.3f
			};

			new System.Timers.Timer() { AutoReset = false, Enabled = true, Interval = 2000 }.Elapsed += (z, zz) => { drumMesh.MaterialOverride = null; };

			var posZ = drum.Transform.origin.z;
			var posX = drum.Transform.origin.x;
			if (posX.Equals(0)) {
				pitchEffect.PitchScale = 1;
			} else if (posX.Equals(1)) {
				pitchEffect.PitchScale = 2;
			} else {
				pitchEffect.PitchScale = posX * 2f;
			}
			sound.Play(Audio);

		}
	}

	public override void _Process(float delta) {

	}

}
