using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UGGRandomizer
{
    public class RandomMain : MelonMod
    {
        private string seedText = "0";
        private int seed;
        private bool showSeedInput = true;
        private bool seedEntered = false;
        private bool showWarning = false;
        private string warningMessage = "";
        private bool showSeed = false;
        private bool showHowTo = false;

        private Text uiText;
        private Color[] rainbowColors = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.magenta
        };
        private float timeCounter;
        private GameObject textObject;
        private GameObject canvasObject;
        private bool uiInitialized = false;

        public override void OnGUI()
        {         
            if (showSeedInput)
            {
                GUILayout.BeginArea(new Rect(10, 10, 200, 150));
                GUILayout.Label("Enter Seed:");
                seedText = GUILayout.TextField(seedText, 9);

                if (GUILayout.Button("Submit Seed"))
                {
                    OnSeedInputChanged();
                    MelonLogger.Msg("Seed Submitted: :)");
                }

                //if (GUILayout.Button("Continue"))
                //{
                    //showSeedInput = false;
                    //MelonLogger.Msg("Seed Input Closed. Continuing without submitting seed.");
                //}

                if (showWarning)
                {
                    GUIStyle warningStyle = new GUIStyle();
                    warningStyle.normal.textColor = Color.red;
                    GUILayout.Label(warningMessage, warningStyle);
                }

                GUILayout.EndArea();
            }

            if (showSeed)
            {
                GUILayout.BeginArea(new Rect(10, 220, 300, 100)); 
                GUILayout.Label("Generated Seed: " + seed);
                GUILayout.EndArea(); 
            }

            if (showHowTo)
            {
                GUILayout.BeginArea(new Rect(15, 275, 350, 650));
                GUILayout.Label("------------------------------------------------------");
                GUILayout.Label("Welcome to UGG Item Randomizer!");
                GUILayout.Label("------------------------------------------------------");
                GUILayout.Label("How To:");
                GUILayout.Label("Start the game normally, choosing either the yellow, green, or red book. (Make sure you erase the save if starting from the beginning)");
                GUILayout.Label("Load into the game, and before you honk, you will notice the input seed box has appeared.");
                GUILayout.Label("Input any desired number for a seed. If 0 is inputted, a random seed will be generated.");
                GUILayout.Label("(Only numbers can be inputted. Letters will not work)");
                GUILayout.Label("Push the submit seed button after you have chosen your number. You can now honk and play!");
                GUILayout.Label("Enjoy!");
                GUILayout.Label("If you quit or reset then the items will go back to their original places.");
                GUILayout.Label("You will have to input the seed again to give you the same item pattern.");
                GUILayout.Label("To show/hide the generated seed number, just hit 3.");
                GUILayout.Label("If you want to play around with the randomness or seeds, then push 2 to show hide the Seed Input Menu.");
                GUILayout.Label("Warning! Please only submit seed in the main game and not in the menus or it will break.");
                GUILayout.Label("------------------------------------------------------");
                GUILayout.Label("Made by xZeKo");
                GUILayout.EndArea();
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                showSeedInput = true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                showSeed = !showSeed;
            }

            if (uiText != null)
            {
                timeCounter += Time.deltaTime;
                float lerpTime = Mathf.PingPong(timeCounter, 1f);
                int colorIndex1 = Mathf.FloorToInt(lerpTime * (rainbowColors.Length - 1));
                int colorIndex2 = Mathf.CeilToInt(lerpTime * (rainbowColors.Length - 1));
                uiText.color = Color.Lerp(rainbowColors[colorIndex1], rainbowColors[colorIndex2], lerpTime);
            }
        }

        public void OnSeedInputChanged()
        {
            if (int.TryParse(seedText, out seed))
            {
                if (seed == 0)
                {
                    seed = new System.Random().Next(1, int.MaxValue);
                    MelonLogger.Msg("Generated random seed: " + seed);
                }

                seedEntered = true;
                showSeedInput = false;
                showWarning = false;
                ShuffleItems(seed);
                MelonLogger.Msg("Shuffled with seed: " + seed);
            }
            else
            {
                warningMessage = "Invalid seed input.\nPlease enter a number.";
                showWarning = true;
                MelonLogger.Msg("Invalid seed input.");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == 1)
            {
                showHowTo = true;
                showSeedInput = false;

                if (!uiInitialized)
                {
                    InitializeUI();
                    MelonLogger.Msg("UI Initialized on entering scene: " + sceneName);
                }
            }
            else if (buildIndex == 2)
            {
                showHowTo = false;
                showSeedInput = true;

                if (textObject != null)
                {
                    GameObject.Destroy(textObject);
                    textObject = null;
                    MelonLogger.Msg("UI Canvas Destroyed for scene: " + sceneName);
                }

                uiInitialized = false;
            }

            MelonLogger.Msg("OnSceneWasInitialized: " + buildIndex.ToString() + " | " + sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (buildIndex == 2)
            {
                MelonLogger.Msg("Scene Unloaded: " + sceneName);

                showHowTo = true;
                showSeedInput = false;

                InitializeUI();
                uiInitialized = true;
                MelonLogger.Msg("UI Initialized after unloading buildIndex == 2.");
            }

            if (buildIndex == 1)
            {
                if (textObject != null)
                {
                    GameObject.Destroy(textObject);
                    textObject = null;
                    uiInitialized = false;
                    MelonLogger.Msg("UI Canvas Destroyed for scene: " + sceneName);
                }
                showHowTo = false;
            }
        }


        private void InitializeUI()
        {
            MelonLogger.Msg("Initializing UI...");

            canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 1000;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.SetActive(true);

            textObject = new GameObject("RainbowText");
            uiText = textObject.AddComponent<Text>();
            uiText.text = "Item\nRandomizer";
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            uiText.fontSize = 75;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;

            textObject.SetActive(true);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -30);

            rectTransform.sizeDelta = new Vector2(500, 200);

            rectTransform.localScale = Vector3.one;

            uiText.color = Color.white;

            uiInitialized = true;
            MelonLogger.Msg("UI Initialized");
        }
        public static void ShuffleItems(int seed)
        {
            MelonLogger.Msg("Shuffling items deterministically with seed: " + seed);

            System.Random rand = new System.Random(seed);

            for (int i = Constants.allitems.Length - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);

                string temp = Constants.allitems[i];
                Constants.allitems[i] = Constants.allitems[j];
                Constants.allitems[j] = temp;

                GameObject spawnItem = GameObject.Find(Constants.allitems[i]);
                GameObject spawnItem1 = GameObject.Find(Constants.allitems[j]);

                if (spawnItem != null && spawnItem1 != null)
                {
                    Vector3 tempPosition = spawnItem.transform.position;
                    spawnItem.transform.position = spawnItem1.transform.position;
                    spawnItem1.transform.position = tempPosition;

                    MelonLogger.Msg($"Swapped positions: {Constants.allitems[i]} <-> {Constants.allitems[j]}");
                }
                else
                {
                    MelonLogger.Error($"GameObject not found for items: {Constants.allitems[i]} or {Constants.allitems[j]}");
                }
            }
        }
    }
}
