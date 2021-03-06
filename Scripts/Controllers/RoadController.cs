﻿using UnityEngine;
using System.Collections;

/* obsluguje akcje zwiazane z modyfikacja sieci drog przez uzytkownika */
public class RoadController : Controller 
{
	/* prefaby obiektow oraz pola przechowujace referencje ich obiektow */
	/* podmenu do zarzadzania drogami */
	public GameObject roadMenuPrefab;
	private Menu roadMenu = null;
	/* grid wyznaczajacy miejsca, na ktorych mozna umieszczac drogi */
	public GameObject gridPrefab;
	private GameObject grid = null;

	/* pozycja skrzyzowania zaznaczonego/utworzonego przez uzytkownika. 
	 * Potrzebne do tworzenia drog */
	private Vector2 crossPos;
	private bool crossSelected;
	/* mapa */
	private Map map;

	/* w zaleznosci od eventu przydziela odpowiednia akcje wykonywana cyklicznie */
	public override Event LastEvent
	{
		set
		{
			switch(value)
			{
			case Event.AddRoadButtonClicked:
				base.LastEvent = value;
				activeAction += AddRoadButtonAction;
				break;
			case Event.RemoveRoadButtonClicked:
				base.LastEvent = value;
				activeAction = RemoveRoadButtonAction;
				break;
			default:
				Debug.LogWarning("Przyjeto nieobslugiwane zdarzenie");
				break;
			}
		}
	}
	
	/* -------------------- WLASCIWE FUNKCJE -------------------- */

	/* wygeneruj grid oraz podmenu */
	public void Start()
	{
		GameObject menu = (GameObject)Instantiate(roadMenuPrefab);
		roadMenu = menu.GetComponent<Menu>();
		roadMenu.listener = this;

		crossSelected = false;

		grid = (GameObject)Instantiate(gridPrefab);

		map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");
	}
	
	/* wykonuj operacje w zaleznosci od ostatniego zgloszonego eventu */
	public override void Update()
	{
		if(activeAction != null)
			activeAction();
	}

	/* niszczy grid i menu */
	public void OnDestroy()
	{
		Destroy(grid);
		Destroy(roadMenu);
	}
		
	/* w miejcu klikniecia tworzy skrzyzowanie i/lub droge */
	private void AddRoadButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 cross2Pos;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);

			cross2Pos = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));
			//jezeli wczesniej zadne skrzyzowanie nie bylo utworzone/zaznaczone, to aktualne nim bedzie
			if(!crossSelected)
			{
				crossPos = cross2Pos;
				map.AddCrossroads(crossPos);
				crossSelected = true;
			}
				
			//w przeciwynm wypadku utworz droge miedzy skrzyzowaniami
			else
			{
				map.AddCrossroads(cross2Pos);
				map.AddRoad(crossPos, cross2Pos);

				//odznacz skryzowanie
				crossSelected = false;
			}
		}
	}

	/* usuwa droge z miejsca, w ktorym kliknieto */
	private void RemoveRoadButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();

		//usun ewentualne zanzaczenie
		crossSelected = false;
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);

			//zaimplementowac
		}
	}
	
}
