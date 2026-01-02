using Godot;
using System;
using NT106.Scripts.Models;
using System.Threading.Tasks;

public partial class StartEffect : Node2D
{
	[Export] private AnimationPlayer PopUpNoti;

	public void startBanner()
	{
		//startBanner.Play("start_game");
		PopUpNoti.Play("start_game");
	}
}
