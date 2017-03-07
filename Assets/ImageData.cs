using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageData : MonoBehaviour {
    public string target_category = "emu";
    public string images_path = "";

    public int image_size = 32;

    public List<float[]> images = new List<float[]>();
    public List<float[]> target_images = new List<float[]>();
    public List<float[]> other_images = new List<float[]>();

    public List<string> image_paths = new List<string>();
    public List<string> target_image_paths = new List<string>();
    public List<string> other_image_paths = new List<string>();

    public float[] GetRandomTargetImage()
    {
        System.Random rnd = new System.Random();
        int rand = rnd.Next(target_images.Count);

        return target_images[rand];
    }

    public float[] GetRandomOtherImage()
    {
        System.Random rnd = new System.Random();
        int rand = rnd.Next(target_images.Count);

        return other_images[rand];
    }

    void Awake()
    {
        images_path = Application.dataPath + "/../" + images_path;

        try
        {
            foreach (string d in Directory.GetDirectories(images_path))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    var file_data = File.ReadAllBytes(f);
                    Texture2D temp_tex = new Texture2D(2, 2);
                    temp_tex.LoadImage(file_data, false);

                    //TextureScale.Bilinear(temp_tex, image_size, image_size);
                    //Directory.CreateDirectory(d.Replace("original", "64x64"));
                    //File.WriteAllBytes(f.Replace("original", "64x64"), temp_tex.EncodeToPNG());

                    var image_data = new float[image_size * image_size];
                    for (int y = 0; y < image_size; y++)
                    {
                        for (int x = 0; x < image_size; x++)
                        {
                            float color = temp_tex.GetPixel(x, y).grayscale;
                            //color = (color - 0.5f) * 2.0f;
                            image_data[x + y * image_size] = color;
                        }
                    }

                    images.Add(image_data);
                    image_paths.Add(f);

                    if (new DirectoryInfo(d).Name == target_category)
                    {
                        target_images.Add(image_data);
                        target_image_paths.Add(f);
                    }
                    else
                    {
                        other_images.Add(image_data);
                        other_image_paths.Add(f);
                    }
                }
            }
        }
        catch (System.Exception excpt)
        {
            print(excpt.Message);
        }

        GC.Collect();
    }

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
