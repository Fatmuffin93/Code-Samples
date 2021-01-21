using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

//TODO: Need to make this much faster it is too slow right now
//Have no other idea on how to do this but this will be called
//after every swap and every solve
//Could also make it so the it happens only on the hint delay so faster players will rarely see it
//Could limit the num of words found but will try my best to avoid
//For now i am only making it find 10 words
public class HintManager : MonoBehaviour
{
    public Tile[] hint;
    public float hintDelay;
    private float hintDelaySeconds;
    public float endDelay;
    private float endDelaySeconds;
    List<Tile[]> hints;
    public bool debugFindHints;
    List<ParticleSystem> currentMarkers;
    bool finished = false;
    public Text hintText;
    Object HintParticle;
    StringBuilder stringBuilder;
    // Start is called before the first frame update
    void Start()
    {
        hintDelaySeconds = hintDelay;
        endDelaySeconds = endDelay;
        hints = new List<Tile[]>();
        currentMarkers = new List<ParticleSystem>();
        HintParticle = Resources.Load("Hint Particle");
    }

    public void InitializeStringBuilder(int count)
    {
        stringBuilder = new StringBuilder(count);
    }

    bool hintNotFound = false;
    // Update is called once per frame
    void Update()
    {
        if (!Grid.ptr.init || Grid.ptr.mouseDown || (PlayerPrefs.HasKey("hints") && PlayerPrefs.GetInt("hints") == 0))
            return;
        if (debugFindHints)
        {
            TestHints();
            debugFindHints = false;
        }
        if (currentMarkers.Count == 0 && Grid.ptr.currentState == GameState.Move)
            hintDelaySeconds -= Time.deltaTime;
        if (hintDelaySeconds <= 0 && currentMarkers.Count == 0 && Grid.ptr.currentState == GameState.Move && !hintNotFound)
        {
            GenerateHint();
            hintDelaySeconds = hintDelay;
        }
        if (finished)
        {
            endDelaySeconds -= Time.deltaTime;
            if (endDelaySeconds <= 0 && currentMarkers.Count > 0)
            {
                for (int i = 0; i < currentMarkers.Count; ++i)
                {
                    currentMarkers[i].Play();
                }
                finished = false;
            }
        }
    }

    public void TestHints()
    {
        float time = Time.realtimeSinceStartup;
        FindHints(true);
        Debug.Log("Total time to search all hints: " + (Time.realtimeSinceStartup - time) + " hints found: " + hints.Count);
        hintText.text = "Hints Time: " + (Time.realtimeSinceStartup - time).ToString("N2");
    }

    public void BoardChanged()
    {
        DestroyMarkers();
        while (hints.Count > 0)
            hints.RemoveAt(0);
        //FindHints();
        hintNotFound = false;
        ResetTimer();
    }

    public void ResetTimer()
    {
        hintDelaySeconds = hintDelay;
    }

    //Select randomly from the hints found
    public void GenerateHint()
    {
        DestroyMarkers();
        if (hints.Count == 0)
            FindHints();
        if (hints.Count > 0)
        {
            //Get a random hint from the words we found and place the markers over the tiles in the scene.
            hint = hints[Random.Range(0, hints.Count - 1)];
            ParticleSystem.MainModule main;
            GameObject go;
            for (int i = 0; i < hint.Length; ++i)
            {
                go = (GameObject)Instantiate(HintParticle, new Vector2(hint[i].column, hint[i].row), Quaternion.identity);
                currentMarkers.Add(go.GetComponent<ParticleSystem>());
                main = currentMarkers[i].main;
                main.startDelay = 0.5f * i;
            }
            if (currentMarkers.Count > 0)
            {
                main = currentMarkers[currentMarkers.Count - 1].main;
                main.stopAction = ParticleSystemStopAction.Callback;
                currentMarkers[currentMarkers.Count - 1].GetComponent<HintParticle>().onParticleStopped += OnParticleStopped;
            }
        }
        else
        {
            hintNotFound = true;
        }
    }

    public void OnParticleStopped()
    {
        endDelaySeconds = endDelay;
        finished = true;
    }

    public void DestroyMarkers()
    {
        if (currentMarkers.Count > 0)
        {
            currentMarkers[currentMarkers.Count - 1].GetComponent<HintParticle>().onParticleStopped -= OnParticleStopped;
            for (int i = 0; i < currentMarkers.Count; ++i)
            {
                Destroy(currentMarkers[i].gameObject);
            }
            while (currentMarkers.Count > 0)
                currentMarkers.RemoveAt(0);
        }
        finished = false;
    }

    public bool InHint(Tile tile)
    {
        if (hint != null && hint.Length > 0)
            return System.Array.IndexOf(hint, tile) != -1;
        return true;
    }

    //Go through every tile on the board until at least 3 hints have been found
    //Will find every word possible if all = true
    private void FindHints(bool all = false)
    {
        if (!Grid.ptr.init || (PlayerPrefs.HasKey("hints") && PlayerPrefs.GetInt("hints") == 0))
            return;
        List<Tile> t = new List<Tile>();
        for (int i = 0; i < Grid.ptr.width; ++i)
        {
            for (int j = 0; j < Grid.ptr.height; ++j)
            {
                if (Grid.ptr.tiles[i, j] == null)
                    continue;
                if(hints.Count < 3 || all)
                    GenerateWord(Grid.ptr.tiles[i, j], t, all);
                while (t.Count > 0)
                    t.RemoveAt(0);
            }
        }
        //string words = "Words found: " + hints.Count + "\n";
        //string word;
        //for (int i = 0; i < hints.Count; ++i)
        //{
        //    word = "";
        //    for (int j = 0; j < hints[i].Length; ++j)
        //    {
        //        word += hints[i][j].letter.letter;
        //    }
        //    words += i + ": " + word + "\n";
        //}
        //Debug.Log("Find Hint Complete\n" + words);
        //yield return null;
    }

    void GenerateWord(Tile current, List<Tile> t, bool all = false)
    {
        if (t.Count > Grid.ptr.solveAmount || (hints.Count >= 3 && !all))
            return;
        //Add letter to the list and then build the string
        t.Add(current);
        //string word = "";
        stringBuilder.Clear();
        for (int i = 0; i < t.Count; ++i)
        {
            stringBuilder.Append(t[i].letter.letter);
            //word += t[i].letter.letter;
        }
        //If the string length is greater than the min word count then check if it is a word
        //If it's a word then add the tiles containing the letters to make up the discovered word
        if (stringBuilder.Length >= 3 && GameDictionary.dictionary.isWord(stringBuilder.ToString()))
        {
            hints.Add(new Tile[t.Count]);
            for (int i = 0; i < t.Count; ++i)
            {
                hints[hints.Count - 1][i] = t[i];
            }
        }
        if (t.Count >= Grid.ptr.solveAmount)
            return;
        //string check = "";
        //Get the adjacent tiles that we don't already have and recursively go through this with the adjacent tiles
        int index = t.Count;
        List<Tile> adj = current.GetAdjacentTiles();
        for (int i = 0; i < adj.Count; ++i)
        {
            if (t.Contains(adj[i]))
                continue;
            GenerateWord(adj[i], t, all);
            for (int j = 0; j < t.Count; ++j)
            {
                //check += t[j].letter.letter;
                stringBuilder.Append(t[j].letter.letter);
            }
            if (!GameDictionary.dictionary.isWord(stringBuilder.ToString()))
            {
                while (t.Count > index)
                    t.RemoveAt(t.Count - 1);
            }
            //check = "";
        }
    }
}
