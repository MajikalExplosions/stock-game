using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public Text errorMessageField;
    public InputField input;
    public Button playButton;
    public Text moneyField;
    public Text priceField;
    public Text stockField;
    public GraphRenderer camera;

    private int userID;
    private int serverID;
    private string username;
    private string action;
    private bool inQueue;
    private bool inGame;
    private float money;
    private int stock;
    private int lastTurnTime;
    public List<float> stockValues;

    private void Awake()
    {
        Application.targetFrameRate = 45;
    }

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        errorMessageField.text = "";
        input.text = generateRandomUsername();
        resetGame();
        stockValues = new List<float>();
    }

    private void Update()
    {
        if (inGame)
        {
            int t = lastTurnTime;
            lastTurnTime /= 1000;
            int currentTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 - 1050;
            currentTime = currentTime % 60000;
            if (currentTime >= lastTurnTime)
            {
                print("Next Turn. Current: " + currentTime + " | " + "Last Time: " + lastTurnTime);
                pingWrapper();
                t = 1000000000;
            }
            lastTurnTime = t;
        }
    }

    public void resetGame()
    {
        userID = -1;
        serverID = -1;
        username = "null";
        inQueue = false;
        inGame = false;
        action = "ping";
        CancelInvoke();
        playButton.interactable = true;
        input.interactable = true;
    }

    public void startGame()
    {

        if (input.text == "")
        {
            errorMessageField.text = "Please input a name.";
            return;
        }
        else
        {
            username = input.text;
            action = "add-queue";
            errorMessageField.color = new Color(110.0f / 255.0f, 110.0f / 255.0f, 110.0f / 255.0f);
            errorMessageField.text = "Attempting to connect to the server...";
        }
        InvokeRepeating("pingWrapper", 0f, 7.5f);
    }

    void OnApplicationQuit()
    {
        if (inQueue || inGame)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("userid", userID.ToString()));
            formData.Add(new MultipartFormDataSection("action", "quit"));
            UnityWebRequest www = UnityWebRequest.Post("https://majikalexplosions.herokuapp.com/p-amber-badger/ping", formData);
            www.SendWebRequest();
            while (!www.isDone) { }
        }
    }

    public void buyStock()
    {
        action = "buy";
        print("Buying stock.");
        if (money >= stockValues.ToArray()[stockValues.Count - 1])
        {
            money -= stockValues.ToArray()[stockValues.Count - 1];
            stock += 1;
        }

        pingWrapper();
        //Send buy to the server
    }

    public void sellStock()
    {
        action = "sell";
        print("Selling stock.");
        if (stock >= 1)
        {
            stock -= 1;
            money += stockValues.ToArray()[stockValues.Count - 1];
        }
        pingWrapper();
        //Send sell to the server
    }

    private void renderGraph()
    {
        //Check strange green cube thing to see where the graph is
    }

    private void pingWrapper()
    {
        StartCoroutine(pingServer());
    }

    IEnumerator pingServer()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        playButton.interactable = false;
        input.interactable = false;
        formData.Add(new MultipartFormDataSection("userid", userID.ToString()));
        formData.Add(new MultipartFormDataSection("username", username));
        formData.Add(new MultipartFormDataSection("action", action));
        formData.Add(new MultipartFormDataSection("timestamp", (System.DateTime.Now.Millisecond * 1000 + System.DateTime.Now.Second * 1000000).ToString()));

        string server = "https://majikalexplosions.herokuapp.com/p-amber-badger/ping";

        UnityWebRequest www = UnityWebRequest.Post(server, formData);

        yield return www.SendWebRequest();

        string reply = www.downloadHandler.text;

        print("Server Response: " + reply);//DEBUG

        if (www.isNetworkError || www.isHttpError)
        {
            if (SceneManager.GetActiveScene().name == "Game") {
                SceneManager.LoadScene("MainMenu");
            }
            resetGame();
            errorMessageField.color = new Color(225.0f / 255.0f, 110.0f / 255.0f, 110.0f / 255.0f);
            errorMessageField.text = "An error occurred while connecting to the server.";
            yield break;
        }
        else
        {
            errorMessageField.color = new Color(110.0f / 255.0f, 225.0f / 255.0f, 110.0f / 255.0f);
        }

        if (userID == -1)
        {
            userID = int.Parse(reply);
            inQueue = true;
            errorMessageField.text = "Connected. Waiting for game to start...";

            CancelInvoke();
            InvokeRepeating("pingWrapper", 0.5f, 1f);
            action = "queue-status";
        }
        else if (inQueue)
        {
            if (reply.ToUpper().StartsWith("G"))
            {
                //Game started yay
                errorMessageField.text = "Game is starting soon...";
                inQueue = false;
                inGame = true;
                serverID = int.Parse(reply.Remove(0, 1));
                SceneManager.LoadScene("Game");
                action = "game-status";
                camera = GameObject.Find("Main Camera").GetComponent<GraphRenderer>();
            }
            else if (reply.ToUpper().StartsWith("P"))
            {
                errorMessageField.text = "Connected. Waiting for game to start..." + reply.Remove(0, 1) + " Players in Queue";
            }
        }
        else if (inGame && SceneManager.GetActiveScene().name == "Game")
        {
            if (reply == "game-over") {
                SceneManager.LoadScene("MainMenu");
                resetGame();
            }
            lastTurnTime = int.Parse(reply.Substring(0, reply.IndexOf("|")));
            reply = reply.Substring(reply.IndexOf("|") + 1);
            stockValues.Add(float.Parse(reply.Substring(0, reply.IndexOf("|"))));
            reply = reply.Substring(reply.IndexOf("|") + 1);
            money = float.Parse(reply.Substring(0, reply.IndexOf("|")));
            reply = reply.Substring(reply.IndexOf("|") + 1);
            stock = int.Parse(reply);

            if (stockField == null || moneyField == null)
            {
                moneyField = GameObject.Find("_moneyfield").GetComponent<Text>();
                stockField = GameObject.Find("_stockfield").GetComponent<Text>();
                priceField = GameObject.Find("_pricefield").GetComponent<Text>();
            }
            moneyField.text = "Money: $" + money.ToString("N2");
            priceField.text = "Stock Price: $" + stockValues.ToArray()[stockValues.Count - 1].ToString("N2");
            stockField.text = "Stock: " + stock.ToString();

            //print("Current Stock Price: " + stockValues.ToArray()[stockValues.Count - 1]);

            action = "game-status";
        }
    }

    private string generateRandomUsername()
    {
        string randomName = "123456789012333456789012345", s = "";
        while (randomName.Length > 24)
        {
            StreamReader adj = new StreamReader("Assets/Resources/adjectives.txt");
            StreamReader names = new StreamReader("Assets/Resources/usernames.txt");

            for (int i = 0; i < Random.Range(0, 247); i++)
            {
                adj.ReadLine();
            }
            s = adj.ReadLine();
            s = s.Substring(0, 1).ToUpper() + s.Substring(1);
            randomName = s;
            adj.Close();
            for (int i = 0; i < Random.Range(0, 186); i++)
            {
                names.ReadLine();
            }
            s = names.ReadLine();
            s = s.Substring(0, 1).ToUpper() + s.Substring(1);
            randomName += s;
            names.Close();
            randomName += Random.Range(0, 10000);
        }
        return randomName;
    }
}