using HardCoded.VRigUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
 
using UnityEngine.UI;


public class DCL_Start : MonoBehaviour
{
	private TMP_Text text;
	private Button toggleButton;
	private Image buttonImage;
	private bool isCameraShowing;

	[SerializeField] string TextOnButton = "Start Camera";
	[SerializeField] string TextOffButton = "Stop Camera";

	[SerializeField] Image IconOnButton;
	[SerializeField] Image IconOffButton;

	[SerializeField] Color toggleOnColor = new(0.08009967f, 0.6792453f, 0.3454931f); // 0x14AD58
	[SerializeField] Color toggleOffColor = new(0.6981132f, 0, 0.03523935f); // 0xB30009

	void Start()
	{
		text = GetComponentInChildren<TMP_Text>();
		buttonImage = GetComponent<Image>();
		toggleButton = GetComponent<Button>();
		InitializeContents();

		//Localization.OnLocalizationChangeEvent += UpdateLanguage;
	}

	private void InitializeContents()
	{
		buttonImage.color = toggleOnColor;
		isCameraShowing = false;

		toggleButton.onClick.RemoveAllListeners();
		toggleButton.onClick.AddListener(delegate {
			SetCamera(!isCameraShowing);
		});
	}

	private void SetCamera(bool enable)
	{
		buttonImage.color = enable ? toggleOffColor : toggleOnColor;
		isCameraShowing = enable;
		UpdateButton();

		if (enable)
		{
			SolutionUtils.GetSolution().Play((_, _) => {
				// Error handling
				SetCamera(false);
			});
		}
		else
		{
			SolutionUtils.GetSolution().Model.ResetVRMAnimator();
			SolutionUtils.GetSolution().Stop();
		}
	}

	private void UpdateButton()
	{
		text.text = isCameraShowing ? TextOffButton : TextOnButton;
		
	}
}
