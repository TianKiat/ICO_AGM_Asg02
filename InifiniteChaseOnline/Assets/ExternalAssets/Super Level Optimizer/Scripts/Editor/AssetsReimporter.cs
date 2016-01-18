using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


public class AssetsReimporter : MonoBehaviour
{
	public void Reimport(Texture[] textures)
	{
		foreach(var tex in textures)
		{
			if(tex !=null)
			{
			    string path = AssetDatabase.GetAssetPath(tex);
				TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;

				if(ti !=null)
					if(ti.isReadable == false)
				{
					ti.isReadable = true;
					AssetDatabase.ImportAsset(path);
				}
			}
		}
	}

    public void CreateMesh(GameObject mesh,string folderPatch)
    {
        folderPatch = folderPatch + "SLOMeshes/";

        if (!Directory.Exists(folderPatch))
            Directory.CreateDirectory(folderPatch);

        if (mesh.GetComponent<MeshFilter>() != null)
            if (mesh.GetComponent<MeshFilter>().sharedMesh != null)
                if (AssetDatabase.GetAssetPath(mesh.GetComponent<MeshFilter>().sharedMesh) == "")
                    AssetDatabase.CreateAsset(mesh.GetComponent<MeshFilter>().sharedMesh, folderPatch + mesh.GetComponent<MeshFilter>().sharedMesh.name + mesh.GetComponent<MeshFilter>().sharedMesh.GetInstanceID() + ".asset");

        if (mesh.GetComponent<MeshRenderer>() != null)
            if (mesh.GetComponent<MeshRenderer>().sharedMaterials.Length > 0)
                for (int i = 0; i < mesh.GetComponent<MeshRenderer>().sharedMaterials.Length;i++)
                {
                    if (mesh.GetComponent<MeshRenderer>().sharedMaterials[i] != null)
                    {
                        if (mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture != null)
                            if (AssetDatabase.GetAssetPath(mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture) == "")
                            {
                                Texture2D texture = (Texture2D)mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture;

                                FileStream fStream = new FileStream(folderPatch + "Texture" + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.name + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.GetInstanceID() + ".png", FileMode.Create, FileAccess.Write);

                                BinaryWriter writer = new BinaryWriter(fStream);

                                Color[] colors = texture.GetPixels();

                                Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, true);

                                newTexture.SetPixels(colors);

                                byte[] bytes = newTexture.EncodeToPNG();

                                writer.Write(bytes);

                                writer.Close();
                                fStream.Close();

                                AssetDatabase.ImportAsset(folderPatch + "Texture" + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.name + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.GetInstanceID() + ".png");

                                AssetDatabase.SaveAssets();

                                mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture = (Texture)AssetDatabase.LoadAssetAtPath(folderPatch + "Texture" + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.name + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].mainTexture.GetInstanceID() + ".png", typeof(Texture));
                            }

                        if (AssetDatabase.GetAssetPath(mesh.GetComponent<MeshRenderer>().sharedMaterials[i]) == "")
                        {
                            AssetDatabase.CreateAsset(mesh.GetComponent<MeshRenderer>().sharedMaterials[i], folderPatch + "Material" + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].name + mesh.GetComponent<MeshRenderer>().sharedMaterials[i].GetInstanceID() + ".asset");
                        }
                    }
                }

        PrefabUtility.CreatePrefab(folderPatch + mesh.GetComponent<MeshFilter>().sharedMesh.name + ".prefab", mesh);
    }
}
