using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

public class GameController : MonoBehaviour {

    public GameObject bars;
    public GameObject roof;
    public GameObject dcr_event;
    public GameObject inputObject;
    public float clickRate;

    public int width = 15;
    public int hight = 7;

    [SerializeField]
    private InputField inputField;
    public Text gameoverText;

    // so we only check for accepting state/deadends each time an event has been executed instead of each update call.
    public static bool retest = false;

    // list of events in this dcr
    private Dictionary<string,EventManager> events;

    // the world map, used as [i,j] give positions x = (i-7)*2 z = (j-3)*2 when instantiating new events. 
    private bool[,] map;

    private List<GameObject> eventObjects;

    public static bool Retest
    {
        get
        {
            return retest;
        }

        set
        {
            retest = value;
        }
    }


    void Start() {
        eventObjects = new List<GameObject>();
        events = new Dictionary<string, EventManager>();
        map = new bool[width, hight];
        MapEmpty();

    }

    private void MapEmpty()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < hight; j++)
            {
                map[i, j] = false;
            }
        }
    }
        /*
         * Get a menu to choose the files needed. and pass this along to the reader instead of having a single one included. 
         * Currently you need to type in file wanted without .xml at end.
         **/
    public void GetInput(string input)
    {
        input = "Maps/" + input;
        inputField.text = "";
        XMLReader(input);

        foreach(KeyValuePair<string,EventManager> e in events)
        {
            e.Value.SetUp();
        }
    }

    /*
     * Check for accepting state (and deadend reached once we get that propper implemented) 
     */
    private void Update()
    {
        if (retest)
        {
            retest = false;
            if (AcceptingState())
            {
                // set to victory screen and have a reset/menu option. 
            }
        }
    }

    /*
     * goes through all events and check anny are included and pending, 
     * Return false if any EventManager returns inlcuded && pending
     * Return true all other cases
     */
    private bool AcceptingState()
    {
        foreach (KeyValuePair<string, EventManager> e in events)
        {
            bool executed;
            bool pending;
            bool included;
            included = e.Value.GetState(out executed, out pending);
                if (pending && included) return false;
        }
        Debug.Log("Accepting state found");
        RestartGame();
        return true;
    }

    private void RestartGame()
    {
        foreach(GameObject e in eventObjects)
        {
            Destroy(e);
        }
        eventObjects.Clear();
        events.Clear();
        gameoverText.text = "Game Over you won!";
        inputObject.SetActive(true);
    }

    // returns a new position at random from the allowed positions. 
    int GetPosition(out int z)
    {
        bool test = false;
        while (!test)
        {
            int x = Random.Range(0, width);
            z = Random.Range(0, hight);
            if (!map[x, z]) return x;
        }
        z = -1;
        return -1;
    }


    /*
     * Current reader of maps from xml files, might be changed later. 
     */
    void XMLReader(string filename)
    {
        TextAsset file = Resources.Load<TextAsset>(filename);
        if(file == null)
        {
            Debug.Log("no file found");
            return;
        }

        inputObject.SetActive(false);

        XmlReader xreader = new XmlTextReader(new StringReader(file.text));
        
        if (xreader.ReadToDescendant("events"))
        {
            Debug.Log("Events entered");
            XmlReader eventsReader = xreader.ReadSubtree();
            if (eventsReader.ReadToDescendant("event"))
            {
                do
                {
                    string id = eventsReader.GetAttribute("id");
                    int z;
                    int x = GetPosition(out z);
                    map[x, z] = true;
                    GameObject toInstantiate = Instantiate(dcr_event, new Vector3((x - (width-1)/2) * 2, 0f, (z - (hight-1)/2) * 2), Quaternion.identity);
                    eventObjects.Add(toInstantiate);
                    EventManager added = toInstantiate.GetComponent<EventManager>();
                    added.ID = id;
                    added.Event = toInstantiate;
                    added.Initialize();
                    events[id] = added;
                    Debug.Log(id + " added to the game");
                } while (eventsReader.ReadToNextSibling("event"));
            }
            eventsReader.Close();
        }
        if (xreader.ReadToNextSibling("constraints"))
        {
            Debug.Log("constraints entered");
            XmlReader creader = xreader.ReadSubtree();
            if (creader.ReadToDescendant("conditions"))
            {
                Debug.Log("conditions entered");
                if (creader.ReadToDescendant("condition"))
                {
                    do
                    {
                        string source = creader.GetAttribute("sourceId");
                        string target = creader.GetAttribute("targetId");
                        events[target].AddCondition(events[source]);
                    } while (creader.ReadToNextSibling("condition"));
                }
            }
            if (creader.ReadToNextSibling("responses"))
            {
                if (creader.ReadToDescendant("response"))
                {
                    do
                    {
                        string source = creader.GetAttribute("sourceId");
                        string target = creader.GetAttribute("targetId");
                        events[source].AddResponse(events[target]);
                    } while (creader.ReadToNextSibling("response"));
                }
            }
            if (creader.ReadToNextSibling("excludes"))
            {
                if (creader.ReadToDescendant("exclude"))
                {
                    do
                    {
                        string source = creader.GetAttribute("sourceId");
                        string target = creader.GetAttribute("targetId");
                        events[source].AddExclusion(events[target]);
                    } while (creader.ReadToNextSibling("exclude"));
                }
            }
            if (creader.ReadToNextSibling("includes"))
            {
                if (creader.ReadToDescendant("include"))
                {
                    do
                    {
                        string source = creader.GetAttribute("sourceId");
                        string target = creader.GetAttribute("targetId");
                        events[source].AddInclusion(events[target]); 
                    } while (creader.ReadToNextSibling("include"));
                }
            }
            if (creader.ReadToNextSibling("milestones"))
            {
                if (creader.ReadToDescendant("milestone"))
                {
                    do
                    {
                        string source = creader.GetAttribute("sourceId");
                        string target = creader.GetAttribute("targetId");
                        events[target].AddMilestone(events[source]);
                    } while (creader.ReadToNextSibling("milestone"));
                }
            }
            creader.Close();
        }
        else { Debug.Log("no constraints found"); }
        if (xreader.ReadToNextSibling("marking"))
        {
            if (xreader.ReadToDescendant("executed"))
            {
                if (xreader.ReadToDescendant("event"))
                {
                    do
                    {
                        string id = xreader.GetAttribute("id");
                        events[id].SetExecuted();
                    } while (xreader.ReadToNextSibling("event"));
                }
            }
            if (xreader.ReadToNextSibling("included"))
            {
                if (xreader.ReadToDescendant("event"))
                {
                    do
                    {
                        string id = xreader.GetAttribute("id");
                        events[id].Include();
                    } while (xreader.ReadToNextSibling("event"));
                }
            }
            if (xreader.ReadToNextSibling("pendingResponses"))
            {
                if (xreader.ReadToDescendant("event"))
                {
                    do
                    {
                        string id = xreader.GetAttribute("id");
                        events[id].MakePending();
                    } while (xreader.ReadToNextSibling("event"));
                }
            }
        }
    }
}
