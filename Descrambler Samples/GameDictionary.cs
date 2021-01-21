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
                if (d.ContainsKey(hash)/*d.ContainsKey(s.Substring(0, 2))*/)
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
            //Debug.Log(line);
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
        //Debug.Log(word + " in dictionary: " + dictionary.ContainsKey(word));
        if (word.Length < 3)
            return false;
        int hash = word.GetHashCode();
        if (d.ContainsKey(hash))
            return d[hash].Contains(word);
        return false;
    }
}
