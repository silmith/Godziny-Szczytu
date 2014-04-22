using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku dodajacego droge */
public class RemoveRoadButton : Button
{
	public override void OnClick()
	{
		Listener.LastEvent = Controller.Event.RemoveRoadButtonClicked;
	}
}
