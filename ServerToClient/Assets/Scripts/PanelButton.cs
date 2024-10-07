using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour, IPointerClickHandler
{
	public string filePath; // �s�x�[���������|
	public GameObject loadedModel;

	// ��l�Ƥ�k�A��Ыث��s�ɽե�
	public void Initialize(string path, GameObject model)
	{
		filePath = path;
		loadedModel = model;
		Debug.Log("PanelButton initialized with file: " + path);
	}

	// �k���I���R��
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			// ��ܧR��UI�A�o�̧A�i�H��{�k����
			UIController uiController = FindObjectOfType<UIController>();
			uiController.OnDeleteButtonClick(this.gameObject);
		}
	}
}
