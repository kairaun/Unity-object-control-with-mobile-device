using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour, IPointerClickHandler
{
	public string filePath; // 存儲加載的文件路徑
	public GameObject loadedModel;

	// 初始化方法，當創建按鈕時調用
	public void Initialize(string path, GameObject model)
	{
		filePath = path;
		loadedModel = model;
		Debug.Log("PanelButton initialized with file: " + path);
	}

	// 右鍵點擊刪除
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			// 顯示刪除UI，這裡你可以實現右鍵選單
			UIController uiController = FindObjectOfType<UIController>();
			uiController.OnDeleteButtonClick(this.gameObject);
		}
	}
}
