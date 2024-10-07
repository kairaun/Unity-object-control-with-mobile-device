using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using Dummiesman;
using UniGLTF;
using VRM;
using SFB;
using System.Text;
using TMPro;

public class UIController : MonoBehaviour
{
	public GameObject addButtonPrefab; 
	public Transform addButtonTransform;
	public GameObject panelButtonPrefab;
	public Transform panelList; 
	private GameObject loadedModel = null;
	public List<GameObject> savedModel = new List<GameObject>();
	public bool loadingStart = false;
	public string message = null;
	public byte[] data = null;
	private List<GameObject> panelButtons = new List<GameObject>();
	[System.Serializable]
	public class SavedObjectData
	{
		public string filePath; 
		public Vector3 position; 
	}
	[System.Serializable]
	public class SavedObjectList
	{
		public List<SavedObjectData> savedObjects = new List<SavedObjectData>(); 
	}

	void Start()
	{
		LoadData();
	}

	public void OnAddButtonClick()
	{
		string filePath = ShowFileDialog();
		if (string.IsNullOrEmpty(filePath)) return;

		GameObject loadedModel = LoadModel(filePath); 
		CreatePanelButton(filePath, loadedModel); 
		SaveData();
	}

	void CreatePanelButton(string filePath, GameObject loadedModel)
	{
		GameObject panelButton = Instantiate(panelButtonPrefab, panelList);
		if (panelButton == null)
		{
			Debug.LogError("PanelButton 生成失敗");
			return;
		}
		panelButton.SetActive(true);

		panelButton.GetComponent<PanelButton>().Initialize(filePath, loadedModel);

		string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
		TextMeshProUGUI textComponent = panelButton.GetComponentInChildren<TextMeshProUGUI>();
		if (textComponent != null)
		{
			textComponent.text = fileName;
			textComponent.enableAutoSizing = true;
			//textComponent.fontSizeMin = 10; 
			//textComponent.fontSizeMax = 20;
			textComponent.alignment = TextAlignmentOptions.Midline;
			textComponent.overflowMode = TextOverflowModes.Ellipsis;
		}
		else
		{
			Debug.LogError("Text UI未找到");
		}

		panelButtons.Add(panelButton);
	}

	GameObject LoadModel(string filePath)
	{
		string extension = Path.GetExtension(filePath).ToLower();
		if (extension == ".obj")
		{
			Debug.Log("Trying to load OBJ file from path: " + filePath);
			if (File.Exists(filePath))
			{
				Debug.Log("OBJ file found, attempting to load.");
				try
				{
					loadedModel = new OBJLoader().Load(filePath, Path.ChangeExtension(filePath, ".mtl"));
					Debug.Log("OBJ file loaded successfully.");
					SetMaterialsToURPLit(loadedModel);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Failed to load OBJ file: " + e.Message + "\n" + e.StackTrace);
				}
			}
			else
			{
				Debug.LogError("OBJ file not found at path: " + filePath);
			}
		}
		else if (extension == ".glb")
		{
			LoadGlb(filePath);
		}
		else if (extension == ".gltf")
		{
			LoadGltf(filePath);
		}
		savedModel.Add(loadedModel);
		loadingStart = true;
		return loadedModel;
	}
	public async void LoadGltf(string filePath)
	{
		var gltfInstance = await GltfUtility.LoadAsync(filePath);
		string fileName = Path.GetFileNameWithoutExtension(filePath);
		gltfInstance.name= fileName;
		gltfInstance.EnableUpdateWhenOffscreen();
		gltfInstance.ShowMeshes();
		loadedModel = gltfInstance.gameObject;
		SetGLBMaterialsToURPLit(loadedModel);
		loadedModel.GetComponent<Animation>().Play();
	}
	public async void LoadGlb(string filePath)
	{
		var glbBytes = File.ReadAllBytes(filePath);
		var glbInstance = await GltfUtility.LoadBytesAsync(filePath, glbBytes);
		string fileName = Path.GetFileNameWithoutExtension(filePath);
		glbInstance.name= fileName;
		glbInstance.EnableUpdateWhenOffscreen();
		glbInstance.ShowMeshes();
		loadedModel = glbInstance.gameObject;
		SetGLBMaterialsToURPLit(loadedModel);
		loadedModel.GetComponent<Animation>().Play();
	}

	private void SetMaterialsToURPLit(GameObject obj)
	{

		foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
		{
			foreach (Material mat in renderer.materials)
			{
				if (mat.shader == null)
				{
					Debug.LogWarning("Material has no shader, assigning default shader.");
					mat.shader = Shader.Find("Standard");
				}
				if (mat.shader.name == "Standard (Specular setup)")
				{
					mat.shader = Shader.Find("Standard");
				}

				Texture albedoTexture = null;
				if (mat.HasProperty("_MainTex"))
				{
					albedoTexture = mat.GetTexture("_MainTex");
				}


				mat.shader = Shader.Find("Universal Render Pipeline/Lit");


				if (albedoTexture != null && mat.HasProperty("_BaseMap"))
				{
					mat.SetTexture("_BaseMap", albedoTexture);
				}


				if (mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") != null)
				{
					mat.SetTexture("_BumpMap", null);
					mat.DisableKeyword("_NORMALMAP");
				}
			}
		}
	}
	private void SetGLBMaterialsToURPLit(GameObject obj)
	{
		// 遞迴遍歷GameObject及其所有子物件
		TraverseAndSetMaterial(obj);
	}
	private void TraverseAndSetMaterial(GameObject obj)
	{
		if (obj == null) return;
		MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
		if (renderer != null)
		{
			foreach (Material mat in renderer.materials)
			{
				if (mat != null && mat.shader.name == "Standard")
				{
					mat.shader = Shader.Find("Universal Render Pipeline/Lit");
				}
			}
		}
		SkinnedMeshRenderer skinnedMeshRenderer= obj.GetComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer != null)
		{
			foreach (Material mat in skinnedMeshRenderer.materials)
			{
				if (mat != null && mat.shader.name == "Standard")
				{
					mat.shader = Shader.Find("UniGLTF/UniUnlit");
				}
			}
		}
		foreach (Transform child in obj.transform)
		{
			TraverseAndSetMaterial(child.gameObject);
		}
	}
	string ShowFileDialog()
	{
		var extensions = new[]
		{
			new ExtensionFilter("Model Files", "obj", "glb", "gltf")
		};

		string[] paths = StandaloneFileBrowser.OpenFilePanel("選擇模型檔案", "", extensions, false);

		if (paths.Length > 0)
		{
			return paths[0];
		}

		return null;
	}
	public void OnDeleteButtonClick(GameObject panelButton)
	{
		PanelButton buttonComponent = panelButton.GetComponent<PanelButton>();
		if (buttonComponent != null && buttonComponent.loadedModel != null)
		{
			Destroy(buttonComponent.loadedModel);
			for (int i = savedModel.Count- 1; i >= 0;i--)
			{
				GameObject obj = savedModel[i];
				if (obj == null)
				{
					savedModel.RemoveAt(i);
					continue;
				}
			}
			message = buttonComponent.loadedModel.name + ",REMOVE";
			data = Encoding.UTF8.GetBytes(message);
		}
		panelButtons.Remove(panelButton); 
		Destroy(panelButton); 
		SaveData();
	}
	void SaveData()
	{
		SavedObjectList savedObjectList = new SavedObjectList();

		foreach (var panelButton in panelButtons)
		{
			var buttonComponent = panelButton.GetComponent<PanelButton>();
			if (buttonComponent != null && buttonComponent.loadedModel != null)
			{
				SavedObjectData data = new SavedObjectData
				{
					filePath = buttonComponent.filePath,  
					position = buttonComponent.loadedModel.transform.position 
				};
				savedObjectList.savedObjects.Add(data);
			}
		}

		string json = JsonUtility.ToJson(savedObjectList);
		File.WriteAllText(Application.persistentDataPath + "/savedObjects.json", json);
		Debug.Log(Application.persistentDataPath);
	}
	void LoadData()
	{
		string filePath = Application.persistentDataPath + "/savedObjects.json";
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			SavedObjectList savedObjectList = JsonUtility.FromJson<SavedObjectList>(json);

			foreach (var savedObject in savedObjectList.savedObjects)
			{
				GameObject loadedModel = LoadModel(savedObject.filePath);
				if (loadedModel != null)
				{
					CreatePanelButton(savedObject.filePath, loadedModel);
					loadedModel.transform.position = savedObject.position;
				}
			}
		}
	}

}
