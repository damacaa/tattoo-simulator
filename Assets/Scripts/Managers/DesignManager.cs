using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static NativeGallery;

public class DesignManager : MonoBehaviour
{
    static public DesignManager instance;


    [SerializeField]
    Transform content;
    [SerializeField]
    List<Sprite> sprites;
    [SerializeField]
    GameObject buttonPrefab;
    [SerializeField]
    bool loadFromFolder = true;

    string imageFolderPath;

    public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
    void Awake()
    {
        instance = this;

        imageFolderPath = Directory.GetParent(Application.dataPath).ToString() + "/Images/";

        for (int i = 0; i < sprites.Count; i++)
        {
            AddButton(sprites[i].texture, sprites[i].texture.name);
        }
        sprites.Clear();

#if !UNITY_WEBGL || UNITY_EDITOR
        if (loadFromFolder)
        {
            string[] files;
            files = System.IO.Directory.GetFiles(imageFolderPath, "*.png");

            for (int i = 0; i < files.Length; i++)
            {
                Texture2D tex = LoadPNG(files[i]);
                string name = Path.GetFileNameWithoutExtension(files[i]);
                AddButton(tex, name);
            }
            //Debug.Log("Loading done");
        }

        //TattooManager.instance.LoadData();

#endif
    }

    void AddButton(Texture2D tex, string name)
    {
        tex.name = name;
        textures[name] = tex;
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        GameObject g = GameObject.Instantiate(buttonPrefab, content);
        g.GetComponent<TattooSelectionButton>().SetSprite(s);
        g.transform.SetSiblingIndex(g.transform.GetSiblingIndex() - 1);

        //Resources.UnloadUnusedAssets();
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            //tex.alphaIsTransparency = true;
            //tex.anisoLevel = 8;
        }
        return tex;
    }

    public void RequestImage()
    {
#if UNITY_WEBGL
        GetImage.GetImageFromUserAsync(gameObject.name, "ReceiveImage");
        //Alternative:
        //https://forum.unity.com/threads/how-do-i-let-the-user-load-an-image-from-their-harddrive-into-a-webgl-app.380985/
#else
        PickImage(0);
#endif


    }


#if UNITY_WEBGL
    static string s_dataUrlPrefix = "data:image/png;base64,";
    public void ReceiveImage(string dataUrl)
    {
        if (dataUrl.StartsWith(s_dataUrlPrefix))
        {
            byte[] pngData = System.Convert.FromBase64String(dataUrl.Substring(s_dataUrlPrefix.Length));

            // Create a new Texture (or use some old one?)
            Texture2D tex = new Texture2D(1, 1); // does the size matter? hehe
            if (tex.LoadImage(pngData))
            {
                AddButton(tex, "a");
                print(dataUrl);
            }
            else
            {
                Debug.LogError("could not decode image");
            }
        }
        else
        {
            Debug.LogError("Error getting image:" + dataUrl);
        }
    }
#endif

    private void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);//Check all paramenters later
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                string name = Path.GetFileNameWithoutExtension(path);
                AddButton(texture, name);

#if UNITY_STANDALONE || UNITY_EDITOR
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(imageFolderPath + name + ".png", bytes);
#endif

                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        });

        Debug.Log("Permission result: " + permission);
    }
}