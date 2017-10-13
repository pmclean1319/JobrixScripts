using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;


/* The controller script does the heavy lifting of Jobrix. It handles the spawning of new bricks, the saving and loading of data, and most of the UI
 * behaviors as well. 
*/
public class DutiballControllerScript : MonoBehaviour {
    
    public GameObject jobrick; 
    public GameObject dutiballIcon;
    public bool spamToggle = true;
    public Sprite newDutiballColor;
    public Sprite newJobrickIcon;
    public GameObject ui;
    public GameObject uiToggleGroup;
    public InputField myInputField;
    public Dropdown myColorDropDown;
    public Dropdown myIconDropDown;
    public InputField myCounterField;
    public Sprite currentIcon;
    public Scrollbar myViewHeight;
    public Scrollbar myViewWidth;
    public GameObject scoreText;
    public int score = 0;
    public Button go;
    public bool[] dailySetter = new bool[8];
    public Text todayOrWeekly;
    float vary = 0;
    public int TodayValue;
    public Dropdown dueMonth;
    public Dropdown dueDay;
    public Dropdown dueYear;
    string[] jobToStore;
    int[] iconToStore;
    int[] colorToStore;
    int[] counterToStore;
    bool[] dailyToStore = new bool[8];
    public string[] masterJobToStore;
    public int[] masterIconToStore;
    public int[] masterColorToStore;
    public int[] masterCounterToStore;
    public bool[][] masterDailyToStore = new bool[8][];
    public List<GameObject> dutiList;
    public List<GameObject> dutiListSchedule;
    public GameObject myDropDownColorImage;
    public GameObject myDropDownIconImage;
    public Sprite[] dutiBallColorOption;
    public Sprite[] dutiBallIconOption;
    public Camera mainCamera;
    public GameObject dropper;
    public float zoom;
    public float jobrickLane = -11.25f;
    public float jobrickWidth = 7.5f;
    public float scrollSpeed = 0.1F;
    public GameObject WeekList;
    public bool toggleEdit = false;
    public GameObject editBar;
    public bool isClicked = false;
    public float scrollmin, scrollmax;
    public float dropHeight;
    public GameObject backlight;

/* The Start() method, like in all Unity projects, is run on the very first frame during the app's runtime. In this case,
 * the method sets up a few items in the UI to be primed for the user. It also checks for what day it is via the DayValueConverter()
 * and begins the StandardLoad() coroutine.
*/
    void Start()
    {
//        Screen.SetResolution(381, 635, false);
        myCounterField.text = "1";
        IconSelect();
        myIconDropDown.value = 0;
        uiToggleGroup.transform.localScale = new Vector3(0, 0, 0);
        go.gameObject.transform.localScale = new Vector3(0, 0, 0);
        DayValueConverter();
        SetDayBacklight(TodayValue);
        StartCoroutine(StandardLoad());
    }

/* Update() is another standard Unity method, running every frame while the app is in runtime. All Update() handles in this
 * instance is the Scroll() method, which allows for the user to scroll the view up and down a bit on mobile devices. 
*/
    void Update()
    {
        Scroll();
    }

/* The DropBrix() method spawns bricks the user has customized in the top menu. It first uses a simple countermeasure to prevent
 * users spamming bricks by tapping the "Go!" button repeatedly, then it cycles through "lanes" so that bricks are dropped in a
 * predictable way. Then it instantiates a new brick at the drop location, passes over information from the user menu onto the new
 * brick, and also handles the task of making copies of the brick and dropping them in the weekly schedule location for future spawning.
 * Finally, the method calls the Save() coroutine.
 */
    public void DropBrix()
    {
        if (spamToggle == true)
        {
            spamToggle = false;

            // Recalculates drop height to account for blocks
            int numOfBrix = GameObject.FindGameObjectsWithTag("SpawnBrick").Length + GameObject.FindGameObjectsWithTag("TempBrick").Length;
            dropHeight = 100 + ((numOfBrix / 4) * jobrickWidth);
            // Creates a randomized lane to drop the block
            float dropLane = jobrickLane + (jobrickWidth * vary);
            vary++;
            if (vary == 4) { vary = 0; }

            // Creates a new block upon which to stamp the information
            GameObject newbrick = (GameObject)Instantiate(jobrick, new Vector3(dropLane, dropHeight, dropper.transform.position.z), Quaternion.identity);

            // Stamps the information preset onto the block.
            newbrick.transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = myInputField.text;
            newbrick.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = myCounterField.text;
            newbrick.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newJobrickIcon;
            newbrick.GetComponent<DutiballBehavior>().SetJob(myInputField.text);
            newbrick.GetComponent<DutiballBehavior>().SetIcon(myIconDropDown.value);
            newbrick.GetComponent<DutiballBehavior>().SetCounter(int.Parse(myCounterField.text));

            // Stamps the daily schedule onto the block.
            for (int i = 0; i <= 7; i++)
            {
                newbrick.GetComponent<DutiballBehavior>().SetDailyDay(i, dailySetter[i]);
            }

            // Checks the "none" boolean on the block, represented by the 7th element in the daily[] property. If true, the block is marked with a SpawnBall tag. If false,
            // the block is marked with a ScheduleBall tag. It also adds the block to its respective list.
            if (newbrick.GetComponent<DutiballBehavior>().daily[7] == true)
            {
                newbrick.tag = "SpawnBrick";
                dutiList.Add(newbrick);
            }

            // Makes copies of the newly spawned brick and drops them in the proper order in weekly area.
            if (newbrick.GetComponent<DutiballBehavior>().daily[7] == false)
            {
                newbrick.tag = "TempBrick";
                dutiListSchedule.Add(newbrick);
                float dropIncrease = 150f;
                for (int i = 0; i < 7; i++)
                {
                    if (newbrick.GetComponent<DutiballBehavior>().daily[i])
                    {
                        GameObject newSchedball = (GameObject)Instantiate(newbrick, new Vector3(WeekList.transform.GetChild(i).transform.position.x,
                        WeekList.transform.GetChild(i).transform.position.y + dropIncrease, WeekList.transform.GetChild(i).transform.position.z), Quaternion.identity);
                        newSchedball.GetComponent<DutiballBehavior>().daily = new bool[8];
                        newSchedball.GetComponent<DutiballBehavior>().daily[i] = true;
                        newSchedball.tag = "ScheduleBrick";
                        dropIncrease += 10f;
                    }

                }
            }

            // Plays a randomized sound for when the block spawns.
            int sound = UnityEngine.Random.Range(0, 5);
            GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioCollection>().library[sound]);

            // Jobrick storage
            StartCoroutine(Save(newbrick));

            // Destroys the new block if today's date isn't the current date.
            if ((newbrick.GetComponent<DutiballBehavior>().GetDailyDay(TodayValue) == false) && (newbrick.GetComponent<DutiballBehavior>().GetDailyDay(7) == false))
            {
                Destroy(newbrick);
            }

            // Does a wait function in order to prevent spamming blocks.
            StartCoroutine(Spamdisabler());
        }
    }

    //Sets the icon preview image to the select brick color.
    public void IconSelect()
    {
        newJobrickIcon = dutiBallIconOption[myIconDropDown.value];
        myDropDownIconImage.GetComponent<Image>().sprite = dutiBallIconOption[myIconDropDown.value];
    }

    /*Jobrix saves bricks by stripping their icon, job text, counter, and various other properties and converting them into serializable data types
     * which can be loaded later and converted back into bricks once gain.
     */
    public IEnumerator Save(GameObject spawnedBall)
    {
        // Basic counter used for interating through SpawnBalls.
        int counter = 0;

        // Sets up an array for both SpawnBalls.
        GameObject[] allBricks = GameObject.FindGameObjectsWithTag("SpawnBrick");

        // Initializes the information stamp needed for the SpawnBalls for saving.
        jobToStore = new string[allBricks.Length];
        iconToStore = new int[allBricks.Length];
        counterToStore = new int[allBricks.Length];

        // Iterates through each block in the allBalls list, and adds their information to the Save stamp.
        foreach (GameObject brick in allBricks)
        {
            if (brick.GetComponent<DutiballBehavior>().isOriginal == false)
            {
                jobToStore[counter] = (brick.GetComponent<DutiballBehavior>().job);
                iconToStore[counter] = (brick.GetComponent<DutiballBehavior>().icon);
                counterToStore[counter] = (brick.GetComponent<DutiballBehavior>().counter);
                counter++;
            }
        }

        // Initializes a new BinaryFormatter for use in the serializing.
        BinaryFormatter bf = new BinaryFormatter();
        
        // Creates or refreshes the playerInfo.dat file.
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        // Initializes a new PlayerData object
        PlayerData data = new PlayerData();

        // Stamps the new data with the arrays made previously in the Save stamp.
        data.storedDutis = jobToStore;
        data.storedIcons = iconToStore;
        data.storedCounters = counterToStore;



            counter = 0;
            GameObject[] allSchedBricks = GameObject.FindGameObjectsWithTag("ScheduleBrick");

            // Initializes the information stamp needed for the SpawnBalls for saving.
            masterJobToStore = new string[allSchedBricks.Length];
            masterIconToStore = new int[allSchedBricks.Length];
            masterCounterToStore = new int[allSchedBricks.Length];
            masterDailyToStore = new bool[allSchedBricks.Length][];
            for (int i = 0; i < allSchedBricks.Length; i++)
                masterDailyToStore[i] = new bool[8];

            // Iterates through each block in the allBalls list, and adds their information to the Save stamp.
            foreach (GameObject ball in allSchedBricks)
            {
                if (ball.GetComponent<DutiballBehavior>().isOriginal == false)
                {
                    masterJobToStore[counter] = (ball.GetComponent<DutiballBehavior>().job);
                    masterIconToStore[counter] = (ball.GetComponent<DutiballBehavior>().icon);
                    masterCounterToStore[counter] = (ball.GetComponent<DutiballBehavior>().counter);
                    masterDailyToStore[counter] = (ball.GetComponent<DutiballBehavior>().daily);
                    counter++;
                }
            }

            // Stamps the new data with the arrays made previously in the Save stamp.
            data.storedMasterDutis = masterJobToStore;
            data.storedMasterIcons = masterIconToStore;
            data.storedMasterCounters = masterCounterToStore;
            data.storedMasterDaily = masterDailyToStore;

        // Serializes the date into the file, and then closes the file.
        bf.Serialize(file, data);
        file.Close();
        yield return new WaitForSeconds(0);
    }

    /*StandardLoad() handles both the opening of the serialized data and also the actual instantiation and dropping of bricks.
     * It starts with just basic bricks that are not repeated, dropping them in the "Today's Tasks" screen, and then moves on
     * to dropping the saved bricks in the weekly area.
     */ 
    public IEnumerator StandardLoad()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            //Opens up the Filestream and opens up PlayerData
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);

            //Loads the mirrored lists from the serialized PlayerData onto the current data lists.
            jobToStore = data.storedDutis;
            iconToStore = data.storedIcons;
            counterToStore = data.storedCounters;
            dailyToStore = data.storedDaily;


            if (jobToStore != null)
            {
                for (int i = 0; i < jobToStore.Length; i++)
                {
                    //Drops the new brick down a new lane, stamping the loaded data onto before the user is able to see te instantiated brick.
                    float dropLane = jobrickLane + (jobrickWidth * vary);
                    vary++;
                    if (vary == 4) { vary = 0; }
                    GameObject newBrick = (GameObject)Instantiate(jobrick, new Vector3(dropLane, dropper.transform.position.y + (jobrickWidth), dropper.transform.position.z), Quaternion.identity);
                    newBrick.GetComponentInChildren<Text>().text = jobToStore[i];
                    newJobrickIcon = dutiBallIconOption[iconToStore[i]];

                    newBrick.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newJobrickIcon;
                    newBrick.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = jobToStore[i];
                    newBrick.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = counterToStore[i].ToString();

                    newBrick.GetComponent<DutiballBehavior>().SetJob(jobToStore[i]);
                    newBrick.GetComponent<DutiballBehavior>().SetIcon(iconToStore[i]);
                    newBrick.GetComponent<DutiballBehavior>().SetCounter(counterToStore[i]);

                    newBrick.tag = "SpawnBrick";

                    int sound = UnityEngine.Random.Range(0, 5);
                    dutiList.Add(newBrick);
                    GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioCollection>().library[sound]);
                    yield return new WaitForSeconds(1);
                }
            }
                        //This too mirrors the stored information onto a series of temporary lists for unpacking onto the new weekly bricks
                        masterJobToStore = data.storedMasterDutis;
                        masterIconToStore = data.storedMasterIcons;
                        masterColorToStore = data.storedMasterColors;
                        masterCounterToStore = data.storedMasterCounters;
                        masterDailyToStore = data.storedMasterDaily;

            if (masterJobToStore != null)
            {
                float dropIncrease = 150f;
                for (int i = 0; i < masterJobToStore.Length; i++)
                {
                    //Drops the new brick down a new lane, stamping the loaded data onto before the user is able to see te instantiated brick.
                    GameObject newSchedBrick = (GameObject)Instantiate(jobrick, new Vector3(0, 0, 0),Quaternion.identity);

                        newSchedBrick.GetComponentInChildren<Text>().text = masterJobToStore[i];
                        newJobrickIcon = dutiBallIconOption[masterIconToStore[i]];

                        newSchedBrick.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newDutiballColor;
                        newSchedBrick.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = newJobrickIcon;
                        newSchedBrick.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = masterJobToStore[i];
                        newSchedBrick.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = masterCounterToStore[i].ToString();

                        newSchedBrick.GetComponent<DutiballBehavior>().SetJob(masterJobToStore[i]);
                        newSchedBrick.GetComponent<DutiballBehavior>().SetIcon(masterIconToStore[i]);
                        newSchedBrick.GetComponent<DutiballBehavior>().SetCounter(masterCounterToStore[i]);
                        newSchedBrick.GetComponent<DutiballBehavior>().daily = (masterDailyToStore[i]);
                        newSchedBrick.GetComponent<DutiballBehavior>().SetDateFromField(dueMonth.value, dueDay.captionText.text, dueYear.captionText.text);
                        newSchedBrick.tag = "ScheduleBrick";

                    //This block of code iterates through each instantiated block's seven-day boolean list, dropping the block down the appropriate lane if that day in the list is marked as true.
                    for (int o = 0; o < 7; o++)
                    {
                        if (newSchedBrick.GetComponent<DutiballBehavior>().daily[o])
                        {
                            GameObject weeklyBall = (GameObject)Instantiate(newSchedBrick, new Vector3(WeekList.transform.GetChild(o).transform.position.x,
                            WeekList.transform.GetChild(o).transform.position.y + dropIncrease, WeekList.transform.GetChild(o).transform.position.z), Quaternion.identity);
                            dropIncrease += 10f;
                            weeklyBall.GetComponent<DutiballBehavior>().daily = new bool[8];
                            weeklyBall.GetComponent<DutiballBehavior>().daily[o] = true;
                        }
                    }
                    newSchedBrick.tag = "DeadBrickWalking";
                    Destroy(newSchedBrick);
                }
            }

            //This block reads each scheduleBrick and drops them into the "Today's Task" if they are set for today.
            GameObject[] allSchedBricks = GameObject.FindGameObjectsWithTag("ScheduleBrick");
            for (int i = 0; i < allSchedBricks.Length; i++)
            {
                print(allSchedBricks.Length);
                float dropLane = jobrickLane + (jobrickWidth * vary);
                vary++;
                if (vary == 4) { vary = 0; }
                if (allSchedBricks[i].GetComponent<DutiballBehavior>().GetDailyDay(TodayValue) == true)
                {
                    GameObject newball = (GameObject)Instantiate(allSchedBricks[i], new Vector3(dropLane, dropper.transform.position.y + (jobrickWidth), dropper.transform.position.z), Quaternion.identity);
                    newball.tag = "TempBrick";
                    yield return new WaitForSeconds(1);
                }
            }
            file.Close();
            IconSelect();
        }
    }

    /*EditLoad() simply handles the switching views between the Today and Weekly screens, among other small UI details.
     */ 
    public void EditLoad()
    {
        if (toggleEdit == false)
        {
            mainCamera.transform.position = new Vector3(123f, 48, -15);
            mainCamera.orthographicSize = 48;
            todayOrWeekly.text = "Weekly Tasks";
            scrollmin = 40;
            scrollmax = 100;
            toggleEdit = true;
        }
        else
        {
            mainCamera.transform.position = new Vector3(0, 23, -15);
            mainCamera.orthographicSize = 27;
            todayOrWeekly.text = "Today's Tasks";
            scrollmin = 23;
            scrollmax = 125;
            toggleEdit = false;
        }
    }

    /*ToggleEditJobrick() enables and disables the user's brick edit screen, as called by the New Brick button.
     */ 
    public void toggleEditJobrick()
    {
        if (uiToggleGroup.transform.localScale == new Vector3(0, 0, 0))
        {
            uiToggleGroup.transform.localScale = new Vector3(1, 1, 1);
            go.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            uiToggleGroup.transform.localScale = new Vector3(0, 0, 0);
            go.transform.localScale = new Vector3(0, 0, 0);
        }
    }

    /*Scroll() uses a couple of simple if statements to handle touch scroll on mobile devices.
     */ 
    public void Scroll()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) || (Input.GetMouseButton(1)))
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            mainCamera.transform.Translate(0, -touchDeltaPosition.y * scrollSpeed, 0);
        }
        if (mainCamera.transform.position.y < scrollmin)
        {
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, scrollmin, mainCamera.transform.position.z);
        }
        if (mainCamera.transform.position.y > scrollmax)
        {
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, scrollmax, mainCamera.transform.position.z);
        }
    }

    // Creates a string called today that it then uses to convert today's day of the week to int. Used to compare blocks duedates to today.
    public void DayValueConverter()
    {
        string today = DateTime.Now.DayOfWeek.ToString();
        if (today.Equals("Monday")) { TodayValue = 0; }
        if (today.Equals("Tuesday")) { TodayValue = 1; }
        if (today.Equals("Wednesday")) { TodayValue = 2; }
        if (today.Equals("Thursday")) { TodayValue = 3; }
        if (today.Equals("Friday")) { TodayValue = 4; }
        if (today.Equals("Saturday")) { TodayValue = 5; }
        if (today.Equals("Sunday")) { TodayValue = 6; }
    }

    //
    public void DailyStamper(int daily)
    {
        if (dailySetter[daily] == false)
        {
            dailySetter[daily] = true;
        }
        else
        {
            dailySetter[daily] = false;
        }
    }
    public void LoadBypass()
    {
        EditLoad();
    }
    public IEnumerator Spamdisabler()
    {
        yield return new WaitForSeconds(1);
        spamToggle = true;
    }
    public void SetDayBacklight(int day)
        {
            backlight.transform.position = new Vector3(WeekList.transform.GetChild(day).transform.position.x, backlight.transform.position.y, backlight.transform.position.z);
        }
}

[System.Serializable]
class PlayerData
{
    public string[] storedDutis;
    public int[] storedIcons;
    public int[] storedColors;
    public int[] storedCounters;
    public bool[] storedDaily;

    public string[] storedMasterDutis;
    public int[] storedMasterIcons;
    public int[] storedMasterColors;
    public int[] storedMasterCounters;
    public bool[][] storedMasterDaily = new bool[8][];

    public bool hasOpenedOnceToday;
    public DateTime currentTime;
}