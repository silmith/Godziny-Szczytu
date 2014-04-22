using UnityEngine;
using System.Collections.Generic;

/* Reprezentuje mape w postaci grafu (polaczonych ze soba punktow z okresleniem dlugosci pomiedzy nimi. */
public class Map : MonoBehaviour 
{
	/* reprezentuje skrzyzowanie */
    private class Road
	{
		private Vector2 start;
		private Vector2 end;
		private float length = 0;
		private GameObject model;

		/* punkt poczatkowy drogi */
		public Vector2 Start
		{
			set
			{
				start = new Vector2(value.x, value.y);
				CalculateLength();
			}
			get
			{
				return start;
			}
		}

		/* punkt koncowy drogi */
		public Vector2 End
		{
			set
			{
				end = new Vector2(value.x, value.y);
				CalculateLength();
			}
			get
			{
				return end;
			}
		}

		/* dlugosc drogi */
		public float Length
		{
			get
			{
				return length;
			}
		}

		/* fizyczny/graficzny model drogi */
		public GameObject Model
		{
			get
			{
				return model;
			}
		}

		/* oblicza dlugosc drogi. Jezeli start lub end sa null, wtedy dlugosc wynosi 0 */
		private void CalculateLength()
		{
			if(start == null || end == null)
				length = 0;
			else
				length = Mathf.Sqrt(Mathf.Pow(start.x - end.x, 2) + Mathf.Pow(start.y - end.y, 2));
		}

		/* konstruktor.
		 * model - fizyczny/graficzny model drogi */
		public Road(GameObject model)
		{
			this.model = model;
		}
	}

	/* ------------------------------------- */

    /* reprezentuje skrzyzowanie */
    private class Crossroads
    {
        private GameObject model;
		private Vector2 position;
		/* polaczenia z innymi skrzyzowaniami. kluczem jest pozycja (x, y) */
		private Dictionary<Vector2, Crossroads> connectedCrossroads;

        /* fizyczny/graficzny model */
        public GameObject Model
        {
            get
            {
                return model;
            }
        }

        /* pozycja skrzyzowania */
        public Vector2 Position
        {
            get
            {
				return position;
            }
        }

		/* polaczenia z innymi skrzyzowaniami */
		public Dictionary<Vector2, Crossroads> ConnectedCrossroads
		{
			get
			{
				return connectedCrossroads;
			}
		}

		/* konstruktor
		 * model - model fizyczny/graficzny skrzyzowania */
		public Crossroads(GameObject model)
		{
			this.model = model;
			position = new Vector2(model.transform.position.x, model.transform.position.z);
			connectedCrossroads = new Dictionary<Vector2, Crossroads>();
		}

		/* konstruktor
		 * model - model fizyczny/graficzny skrzyzowania 
		 * pos - pozycja skrzyzowaania */
		public Crossroads(GameObject model, Vector2 pos)
		{
			this.model = model;
			this.model.transform.Translate(new Vector3(pos.x, 0, pos.y));
			position = pos;
			connectedCrossroads = new Dictionary<Vector2, Crossroads>();
		}
       
    }

    /* ------------------------------------- */

    /* prefab skrzyzowania */
    public GameObject crossroadsPrefab;
    /* prefab drogi */
    public GameObject roadPrefab;

	/* zbior skrzyzowan. kluczem jest pozycja (x, y) */
	private Dictionary<Vector2, Crossroads> crossroads;
	/* zbior drog, ktore lacza skrzyzowania. kluczem jest pozycja srodka drogi (x, y) */
	private Dictionary<Vector2, Road> roads; 

	/* ekwiwalent konstruktora */
	public void Start () 
	{
		crossroads = new Dictionary<Vector2, Crossroads>();
		roads = new Dictionary<Vector2, Road>();
	}

	/* tworzy i dodaje do mapy skrzyzowanie.
	 * pos - pozycja skrzyzowania
	 * zwraca: true, jesli skrzyzowanie dodane; false - w przeciwnym wypadku */
	public bool AddCrossroads(Vector2 pos)
	{
		bool returnVal = false;
		Crossroads cross; //nowoutworzone skrzyzowanie

		//jesli skrzyzowanie na danej pozycji juz istnieje, to nie dodawaj nowego
		if(!crossroads.ContainsKey(pos))
		{
			cross = new Crossroads((GameObject)Instantiate(crossroadsPrefab), pos);
			crossroads.Add(pos, cross);

			returnVal = true;
		}

		return returnVal;
	}

	/* dodaje droge laczaca dwa skrzyzowania
	 * cross1Pos, cross2Pos - skrzyzowania, ktore maja zostac polaczone
	 * zwraca: true, gdy droga zostanie dodana; false - w przeciwnym wypadku */
	public bool AddRoad(Vector2 cross1Pos, Vector2 cross2Pos)
	{
		bool returnVal = false;
		Road road; //nowoutworzona droga
		Vector2 roadPos; //punkt srodkowy drogi
		float roadAngle = 0; //kat obrotu drogi w radianach
		Crossroads cross1 = crossroads[cross1Pos];
		Crossroads cross2 = crossroads[cross2Pos]; 

		//jesli polaczenie istnieje, to nic nie rob. nei lacz tex skryzowania z samym soba
		if(cross1 != null && cross2 != null && cross1 != cross2 && !cross1.ConnectedCrossroads.ContainsKey(cross2Pos) && cross1Pos != cross2Pos)
		{
			road = new Road((GameObject)Instantiate(roadPrefab));

			//nadaj punkty koncowe
			road.Start = cross1Pos;
			road.End = cross2Pos;

			//nadaj odpowiednie polozenie drogi
			roadPos = new Vector2((cross1Pos.x + cross2Pos.x) / 2.0f, (cross1Pos.y + cross2Pos.y) / 2.0f);
			road.Model.transform.Translate(new Vector3(roadPos.x, 0.0f, roadPos.y));

			//nadaj odpowiednia dlugosc
			road.Model.transform.localScale = new Vector3(0.1f, 1.0f, road.Length * road.Model.transform.localScale.z);

			//obroc pod odpowiednim katem
			roadAngle = (cross1Pos.x - cross2Pos.x)/ (cross1Pos.y - cross2Pos.y);
			roadAngle = Mathf.Atan(roadAngle);
			road.Model.transform.Rotate(new Vector3(0, roadAngle * Mathf.Rad2Deg, 0));

			//zanim dodasz droge, sprawdz, czy nie przecina sie z inna
			foreach(var r in roads)
			{
				if(r.Value.Model.renderer.bounds.Intersects(road.Model.renderer.bounds))
					Debug.Log("SRAAAANIE");
			}

			//dodaj droge do kolekcji
			roads.Add(roadPos, road);

            //polacz logicznie ze soba skrzyzowania
            cross1.ConnectedCrossroads.Add(cross2Pos, cross2);
            cross2.ConnectedCrossroads.Add(cross1Pos, cross1);

			returnVal = true;
		}

		return returnVal;
	}

	/* usuwa skrzyzowanie o podanej pozycji. niszczy tez wszelkie drogi polaczone z tym skrzyzowaniem oraz polaczenia logiczne
	 * zwraca: true, jesli skrzyzowanie zostanie usuniete; false - w przeciwnym wypadku */
	public bool RemoveCrossroads(Vector2 pos)
	{
		bool returnVal = false;
		Crossroads cross; //skrzyzowanie do usuniecia

		if(crossroads.ContainsKey(pos))
		{
			cross = crossroads[pos];

			//usun polaczenia (logiczne oraz drogi)
			foreach(var c in cross.ConnectedCrossroads)
				RemoveRoad(pos, c.Value.Position);

			//usun samo skrzyzowanie
			GameObject.Destroy(cross.Model);
			returnVal = crossroads.Remove(pos);
		}

		return returnVal;
	}

	/* usuwa droge laczaca skrzyzowania o podanych pozycjach. usuwa tez polaczenie logiczne miedzy skrzyzowaniami*/
	public bool RemoveRoad(Vector2 cross1Pos, Vector2 cross2Pos)
	{
		bool returnVal = false;
		Crossroads cross1 = crossroads[cross1Pos];
		Crossroads cross2 = crossroads[cross2Pos];
		Vector2 roadPos;
		Road road; //droga do usuniecia

		if(cross1 != null && cross2 != null)
		{
			roadPos = (cross1Pos + cross2Pos) / 2.0f;
			road = roads[roadPos];

			if(road != null)
			{
				GameObject.Destroy(road.Model);
				returnVal = roads.Remove(roadPos);
				
				//usun polaczeni logiczne miedzy skrzyzowaniami
				cross1.ConnectedCrossroads.Remove(cross2Pos);
				cross2.ConnectedCrossroads.Remove(cross1Pos);
			}
		}

		return returnVal;
	}
}
