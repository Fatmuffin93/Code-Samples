using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class GameDictionary : MonoBehaviour
{
    public static GameDictionary dictionary;
    TextAsset textFile;
    Dictionary<int, List<string>> d;
    Dictionary<int, List<string>> currentBucket;
    Dictionary<string, Dictionary<int, List<string>>> e;
    bool loaded = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (dictionary == null)
        {
            DontDestroyOnLoad(this.gameObject);
            dictionary = this;
            StartCoroutine(Load());
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
   
    IEnumerator Load()
    {
        if (d != null)
            yield break;
        loaded = false;
        d = new Dictionary<int, List<string>>();
        e = new Dictionary<string, Dictionary<int, List<string>>>();
        textFile = (TextAsset)Resources.Load("words");
        string[] array = textFile.text.Split('\n');
        string s;
        int hash;
        int count = 0;
        foreach (string line in array)
        {
            s = line.ToUpper().Replace("\r", "");
            hash = s.GetHashCode();
            if (s.ToCharArray().Length >= 3 && s.ToCharArray().Length <= 9)
            {
                if (e.ContainsKey(s.Substring(0, 2)))
                {
                    if (e[s.Substring(0, 2)].ContainsKey(hash))
                    {
                        e[s.Substring(0, 2)][hash].Add(s);
                    }
                    else
                    {
                        e[s.Substring(0, 2)].Add(hash, new List<string>() { s });
                    }
                }
                else
                {
                    e.Add(s.Substring(0, 2), new Dictionary<int, List<string>>());
                    e[s.Substring(0, 2)].Add(hash, new List<string>() { s });
                }

                if (d.ContainsKey(hash))
                {
                    d[hash].Add(s);
                }
                else
                {
                    d.Add(hash, new List<string>() { s });
                }
            }
            count++;
            if (count > 10000)
            {
                count = 0;
                yield return null;
            }
        }

        textFile = null;
        loaded = true;
    }

    public bool isLoaded()
    {
        return loaded;
    }

    public bool isWord(string word)
    {
        if (word.Length < 3)
            return false;
        int hash = word.GetHashCode();
        if (d.ContainsKey(hash))
            return d[hash].Contains(word);
        return false;
    }

    public bool SetBucket(string key)
    {
        if (e.ContainsKey(key))
        {
            currentBucket = e[key];
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isWordBucket(string word)
    {
        if (word.Length < 3)
            return false;
        int hash = word.GetHashCode();
        if (currentBucket.ContainsKey(hash))
            return currentBucket[hash].Contains(word);
        return false;
    }
}
