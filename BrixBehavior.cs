using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DutiballBehavior : MonoBehaviour, IPointerClickHandler {
    public float speedMax;
    public string job;
    public int icon;
    public int color;
    public bool isOriginal=false;
    public GameObject controller;
    public GameObject explosion;
    public GameObject coin;
    public int counter = 1;
    public bool[] daily = new bool[8];
    public int[] dueDate = new int[2];
    public DateTime dueDateStruct;
    public int flickerBeacon = 0;

	// Use this for initialization
	void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        
    }

    void Start()
    {
        transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = counter.ToString();
    }

    void Update()
    {
        FlashDueDate();
    }
	
    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (counter == 1)
        {
            gameObject.tag = "DeadBrickWalking";
            Instantiate(explosion, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            
            counter--;
            transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = counter.ToString();
            GameObject jumpCoin = (GameObject)Instantiate(coin, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z-1), Quaternion.identity);
        }
        StartCoroutine(controller.GetComponent<DutiballControllerScript>().Save(this.gameObject));
    }

    public void SetJob(string item) { job = item; }
    public string GetDuti() { return job; }
    
    public void SetIcon(int item) { icon = item; }    
    public int GetIcon() { return icon; }

    public void SetColor(int item) { color = item; }
    public int SetColor() { return color; }

    public void SetDailyDay(int day, bool set) { daily[day] = set; }
    public bool GetDailyDay(int day) { return daily[day]; }

    public void SetCounter(int item) { counter = item; }

    public void SetDateFromField(int month, string day, string year)
    {
            dueDate[0] = month;
        if (day != "")
        {
            dueDate[1] = Convert.ToInt32(day);
            dueDate[2] = Convert.ToInt32(year);
        }
        string date = month.ToString() + "/" + day + "/" + year;
        print(date);
        transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().text = date;
        if (month != 0)
        {
            transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().enabled = true;
        }
    }

    public void SetDateFromLoad()
    {
        string date = dueDate[0].ToString() + "/" + dueDate[1] + "/" + dueDate[2];
        print(date);
        transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().text = date;
        if (dueDate[0] != 0)
        {
            transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().enabled = true;
        }
    }

    void FlashDueDate()
    {
        if (flickerBeacon == 1)
        {
            transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().color = new Color(1, 0, 0, 1);
            flickerBeacon = 2;
        }
        else if (flickerBeacon == 2)
        {
            transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().color = new Color(1, 0.92f, 0.016f, 1);
            flickerBeacon = 1;
        }
    }
}
