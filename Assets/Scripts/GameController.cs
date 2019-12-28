using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    private const string STORAGE_COINS = "COINS";
    private const string STORAGE_LEVEL = "LEVEL";

    public static event Action GameStarted;
    public static event Action<int> CoinsChanged;
    public static int Coins => instance.coins;

    private static GameController instance;
    
    [SerializeField] private SledsController sledsFab;
    [SerializeField] private LevelController[] levelFabs;
    [SerializeField] private LineRenderer[] foePresets;

    private SledsController playerSleds;
    private SledsController foeSleds;
    private bool isPlaying = false;
    private LevelController playerLevel;
    private LevelController foeLevel;
    private bool inputLocked = false;
    private bool foePlaying;
    private string foeNick;

    private int playerLayer;
    private int foeLayer;
    private int collidesWithPlayerLayer;
    private int collidesWithFoeLayer;
    private int levelLayer;
    private int decorLayer;

    private int coins
    {
        get => PlayerPrefs.GetInt(STORAGE_COINS, 100);
        set => PlayerPrefs.SetInt(STORAGE_COINS, value);
    }
    
    private int levelIndex
    {
        get => PlayerPrefs.GetInt(STORAGE_LEVEL, 0);
        set => PlayerPrefs.SetInt(STORAGE_LEVEL, value);
    }

    public static void ApplyDrawing(Vector2[] posArray, float width, float height, bool toRight)
    {
        if (instance.inputLocked) return;
        if (!instance.isPlaying)
        {
            GameStarted?.Invoke();
            instance.isPlaying = true;
            instance.playerSleds.StartRace(instance.playerLayer);
            instance.foeSleds.StartRace(instance.foeLayer, instance.foeNick);
            instance.foePlaying = true;
            instance.StartCoroutine(instance.GenerateFoeDrawings());
        }
        instance.playerSleds.SetDrawnData(posArray, width, height, toRight);
    }
    
    public static void Restart()
    {
        if (!instance.isPlaying || instance.inputLocked)
            return;

        Vector3 checkpoint = instance.playerSleds.LastCheckpoint;
        Destroy(instance.playerSleds.gameObject);
        instance.isPlaying = false;
        instance.playerSleds = Instantiate(instance.sledsFab, checkpoint, Quaternion.identity);
        instance.playerSleds.SetCheckpoint(checkpoint);
        instance.inputLocked = true;
        UIController.FadeIn(() =>
        {
            CamController.SetPosition(instance.playerSleds.transform.position);
            instance.inputLocked = false;
            UIController.ResetFtue();
            UIController.FadeOut();
        });
    }

    public static void AddCoin()
    {
        instance.coins++;
        CoinsChanged?.Invoke(instance.coins);
    }

    public static void Finished(bool isPlayer)
    {
        if (isPlayer)
            instance.StartCoroutine(instance.FinishLevelInternal());
        else
            instance.foePlaying = false;
    }

    public static void LoadLevel()
    {
        instance.LoadLevelInternal();
    }

    private IEnumerator GenerateFoeDrawings()
    {
        while (foePlaying)
        {
            LineRenderer foePreset = foePresets[Random.Range(0, foePresets.Length)];
            Vector3[] foePoints = new Vector3[foePreset.positionCount];

            bool dirRight = instance.foeSleds.DirectedRight;
            if (!dirRight)
                Array.Reverse(foePoints);
            foePreset.GetPositions(foePoints);

            for (int i = 0; i < foePoints.Length; i++)
            {
                foePoints[i] += Random.insideUnitSphere * 50f;
                foePoints[i] *= Vector2.one * (4f / 1024);
                foePoints[i] *= dirRight ? Vector2.one : new Vector2(-1, 1);
            }

            Rect borders = new Rect(foePoints[0], Vector2.zero);
            Array.ForEach(foePoints, p =>
            {
                borders.xMin = Mathf.Min(borders.xMin, p.x);
                borders.xMax = Mathf.Max(borders.xMax, p.x);
                borders.yMin = Mathf.Min(borders.yMin, p.y);
                borders.yMax = Mathf.Max(borders.yMax, p.y);
            });
            Vector2[] posArr = new Vector2[foePoints.Length];
            for (int i = 0; i < foePoints.Length; i++)
                posArr[i] = (Vector2) foePoints[i] - borders.center;

            instance.foeSleds.SetDrawnData(posArr, borders.width, borders.height, dirRight);

            yield return new WaitForSeconds(Random.Range(2f, 7f));
        }
    }

    private IEnumerator FinishLevelInternal()
    {
        inputLocked = true;
        yield return new WaitForSeconds(3f);
        UIController.FadeIn(() =>
        {
            if (levelIndex >= levelFabs.Length - 1)
                levelIndex = 0;
            else
                levelIndex++;

            GameStarted = null;
            CoinsChanged = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }
    
    private void LoadLevelInternal()
    {
        playerLevel = Instantiate(instance.levelFabs[levelIndex]);
        foeLevel = Instantiate(instance.levelFabs[levelIndex]);
        foeLevel.transform.position = Vector3.forward * 5;
        Array.ForEach(foeLevel.GetComponentsInChildren<Collider2D>(), c =>
        {
            if (c.gameObject.layer == playerLayer)
                c.gameObject.layer = foeLayer;
            if (c.gameObject.layer == collidesWithPlayerLayer)
                c.gameObject.layer = collidesWithFoeLayer;
            if (c.gameObject.layer == levelLayer)
                Destroy(c);
        });
        Array.ForEach(foeLevel.GetComponentsInChildren<MeshRenderer>(), mr =>
        {
            if (mr.gameObject.layer == decorLayer)
                Destroy(mr.gameObject);
        });

        Vector3 playerSpawnPos = playerLevel.SpawnPoint.position;
        playerSleds = Instantiate(sledsFab, playerSpawnPos, Quaternion.identity);
        playerSleds.SetCheckpoint(playerSpawnPos);
        
        Vector3 foeSpawnPos = foeLevel.SpawnPoint.position;
        foeSleds = Instantiate(sledsFab, foeSpawnPos, Quaternion.identity);
        foeSleds.SetCheckpoint(foeSpawnPos);

        if (Random.value > .7f)
            foeNick = "Player" + Random.Range(1000, 9999);
        else
        {
            string[] nickdb = Resources.Load<TextAsset>("first-names").text.Split('\n');
            foeNick = nickdb[Random.Range(0, nickdb.Length)];
        }
    }

    private void Awake()
    {
        instance = this;
        playerLayer = LayerMask.NameToLayer("Player");
        foeLayer = LayerMask.NameToLayer("Foe");
        collidesWithPlayerLayer = LayerMask.NameToLayer("CollidesWithPlayer");
        collidesWithFoeLayer = LayerMask.NameToLayer("CollidesWithFoe");
        levelLayer = LayerMask.NameToLayer("Level");
        decorLayer = LayerMask.NameToLayer("Decoration");
        Application.targetFrameRate = 60;

        if (!Application.isEditor)
        {
            AppLovin.InitializeSdk();
            AppLovin.PreloadInterstitial();
            AppLovin.LoadRewardedInterstitial();
        }
    }

    private void Update()
    {
        if (!playerSleds) return;
        CamController.SetPosition(playerSleds.transform.position);
    }
}
