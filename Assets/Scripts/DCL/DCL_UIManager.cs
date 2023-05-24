using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

using static HardCoded.VRigUnity.FileDialogUtils;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity
{
    [System.Serializable]
    public struct VRMMesehesObjects
    {
        public SkinnedMeshRenderer vrm_hair;
        public SkinnedMeshRenderer vrm_lower_bod;
        public SkinnedMeshRenderer vrm_upper_body;
        public SkinnedMeshRenderer vrm_feet;
        public SkinnedMeshRenderer vrm_head;

        public SkinnedMeshRenderer vrm_facial_hair;

    }

    public class DCL_UIManager : MonoBehaviour
    {

        public VRMMesehesObjects meshesMale;
        public VRMMesehesObjects meshesFemale;


        public static DCL_UIManager Instance;

        public UIDocument UIpanel;

        VisualElement rootUI;

        public Animator anima;

        public VisualElement menuPanel;
        public VisualElement closeSettings;
        public VisualElement openSettings;


        public VisualElement WalletPANELDisconnected;
        public VisualElement WalletPANELConnected;


        /******/

        public VisualElement submenuSceneContent;
        public VisualElement submenuVideoContent;

        public VisualElement panelScene;
        public VisualElement panelVideo;


        public UnityEvent OnClickWalletConnect;

        public UnityEvent OnClickLogout;
        public UnityEvent OnClickRefresh;
        public UnityEvent OnClickExport;


        private HolisticSolution _solution;

         /**** FADER **/
        public CanvasGroup canvasGroup;
        public float fadeOutDuration = 3f;
        public float fadeInDuration = 3f;

        private float currentAlpha;
        private float fadeSpeed;


        WebCamSource imageSource;

        protected GUIMain guiMain;


        public Transform parentMeshesVRM;

        

        void Awake()
        {
            Instance = this;

          
            
        }

       


        public IEnumerator Start()
        {
            currentAlpha = 1f;
            fadeSpeed = 1f / fadeOutDuration;
            
            
            
            guiMain = GameObject.FindObjectOfType<GUIMain>();

            imageSource = SolutionUtils.GetImageSource();

            yield return null;


            _solution = SolutionUtils.GetSolution();

            rootUI = UIpanel.rootVisualElement;


            menuPanel = rootUI.Q<VisualElement>("Menu");

            /***/

            closeSettings = rootUI.Q<VisualElement>("btnCloseMenu");
            closeSettings.AddManipulator(new Clickable(evt => SeteSettingsEvent(false)));

            openSettings = rootUI.Q<VisualElement>("Closed");
            openSettings.AddManipulator(new Clickable(evt => SeteSettingsEvent(true)));


            SettingsWalletConnect();
            SettingsSubmenu();
            SetPanelAccount();

            SettingsCamera();

            SettingsFlippedImage();
            SettingsVirtualCamera();

            SettingsBackground();
            SettingsMaleFemale();


            UpdateSources();
            StartCoroutine(UpdateResolutions());

            yield return StartCoroutine(FadeCanvasOut());
            canvasGroup.blocksRaycasts = false;
            OnClickWalletConnect.Invoke();

        }

        private IEnumerator FadeCanvasOut()
        {
            while (currentAlpha > 0f)
            {
                currentAlpha -= fadeSpeed * Time.deltaTime;               
                canvasGroup.alpha = currentAlpha;
                yield return null;
            }
            canvasGroup.alpha = 0;
        }




        void SeteSettingsEvent(bool status)
        {
            menuPanel.style.display = (status == false) ? DisplayStyle.None : DisplayStyle.Flex;
            openSettings.style.display = (status == false) ? DisplayStyle.Flex : DisplayStyle.None;
        }



        void SettingsWalletConnect()
        {
            WalletPANELDisconnected = rootUI.Q<VisualElement>("WalletPANEL");
            WalletPANELConnected = rootUI.Q<VisualElement>("Wallet-UserConnect");

            VisualElement btnWalletConnect = rootUI.Q<VisualElement>("btnWallet");
            btnWalletConnect.AddManipulator(new Clickable(evt => btnWalletConnectEvent()));

            VisualElement btnUserRefresh = rootUI.Q<VisualElement>("btnWallet");
            btnWalletConnect.AddManipulator(new Clickable(evt => btnWalletConnectEvent()));
        }



        void SettingsSubmenu()
        {
            submenuSceneContent = rootUI.Q<VisualElement>("SubmenuScene");
            submenuVideoContent = rootUI.Q<VisualElement>("SubmenuVideo");

            panelScene = rootUI.Q<VisualElement>("SceneTab");
            panelVideo = rootUI.Q<VisualElement>("VideoTab");

            VisualElement btnScene = rootUI.Q<VisualElement>("btnScene");
            btnScene.AddManipulator(new Clickable(evt => ShowSceneArea()));

            VisualElement btnVideo = rootUI.Q<VisualElement>("btnVideo");
            btnVideo.AddManipulator(new Clickable(evt => ShowVideoArea()));
        }


        void ShowSceneArea()
        {
            submenuSceneContent.style.display = DisplayStyle.Flex;
            submenuVideoContent.style.display = DisplayStyle.None;
            panelScene.style.display = DisplayStyle.Flex;
            panelVideo.style.display = DisplayStyle.None;
        }


        void ShowVideoArea()
        {

            submenuSceneContent.style.display = DisplayStyle.None;
            submenuVideoContent.style.display = DisplayStyle.Flex;
            panelScene.style.display = DisplayStyle.None;
            panelVideo.style.display = DisplayStyle.Flex;
        }




        void btnWalletConnectEvent()
        {            
            OnClickWalletConnect.Invoke();
        }


        public void WalletOnConnected()
        {

            rootUI.Q<VisualElement>("btnAvatar").style.display = DisplayStyle.Flex;
            rootUI.Q<VisualElement>("selectorAvatar").style.display = DisplayStyle.Flex;
            rootUI.Q<VisualElement>("selectorMale").style.display = DisplayStyle.None;
            rootUI.Q<VisualElement>("selectorFemale").style.display = DisplayStyle.None;


            btnRefreshAvatar.style.display = DisplayStyle.Flex;

            WalletPANELDisconnected.style.display = DisplayStyle.None;
            WalletPANELConnected.style.display = DisplayStyle.Flex;
        }


        public void WalletOnDisconnected()
        {
            MaleOnCLick();

            btnRefreshAvatar.style.display = DisplayStyle.None;
            WalletPANELDisconnected.style.display = DisplayStyle.Flex;
            WalletPANELConnected.style.display = DisplayStyle.None;
        }

        VisualElement btnRefreshAvatar;
        VisualElement btnExport;

        public void SetPanelAccount()
        {

            btnRefreshAvatar = rootUI.Q<VisualElement>("CTArefresh");
            btnRefreshAvatar.AddManipulator(new Clickable(evt => RefreshOnClick()));

            VisualElement btnLogout = rootUI.Q<VisualElement>("LOG-Out");
            btnLogout.AddManipulator(new Clickable(evt => LogoutOnClick()));

            btnExport = rootUI.Q<VisualElement>("ExportCTA");
            btnExport.AddManipulator(new Clickable(evt => ExportOnClick()));
        }


        void RefreshOnClick()
        {
            rootUI.Q<VisualElement>("selectorAvatar").style.display = DisplayStyle.Flex;
            rootUI.Q<VisualElement>("selectorMale").style.display = DisplayStyle.None;
            rootUI.Q<VisualElement>("selectorFemale").style.display = DisplayStyle.None;
            OnClickRefresh.Invoke();
        }

        void LogoutOnClick()
        {

            rootUI.Q<VisualElement>("btnAvatar").style.display = DisplayStyle.None;

            WalletOnDisconnected();
            OnClickLogout.Invoke();
        }


        void ExportOnClick()
        {
            OnClickExport.Invoke();
        }





        public void SettingsHide()
        {
            anima.SetBool("show", false);
        }

        public void SettingsShow()
        {
            anima.SetBool("show", true);
        }


        private void UpdateSources()
        {
           // WebCamSource imageSource = SolutionUtils.GetImageSource();
            var sourceNames = imageSource.SourceCandidateNames;
            int sourceId = imageSource.SelectSourceFromName(Settings.CameraName);

            var options = new List<string>(sourceNames);
            DropdownField dropdownField = rootUI.Q<DropdownField>("DropdownCameras");           
            dropdownField.choices = options;

            dropdownField.RegisterValueChangedCallback(evt => {
                var selectedValue = evt.newValue;
                var selectedIndex = dropdownField.choices.IndexOf(selectedValue);

                imageSource.SelectSource(selectedIndex);
                if (!_solution.IsPaused)
                {
                    _solution.Play();
                }

                Debug.Log("Índice seleccionado: " + selectedIndex);
            });

            //dropdownField.RegisterCallback<ChangeEvent<string>>(OnDropdownValueChanged);



        }


        private IEnumerator UpdateResolutions()
        {
            //var imageSource = SolutionUtils.GetImageSource();
            //var sourceNames = imageSource.SourceCandidateNames;
            //int sourceId = imageSource.SelectSourceFromName(Settings.CameraName);


            var imageSource = SolutionUtils.GetImageSource();
            yield return imageSource.UpdateSources();
         
           var resolutions = imageSource.AvailableResolutions;
            int resolutionId = imageSource.SelectResolutionFromString(Settings.CameraResolution);

            var options = resolutions.ToList().Select(option => option.ToString()).ToList();



           // var options = new List<string>(sourceNames);
            DropdownField dropdownField = rootUI.Q<DropdownField>("DropdownResolution");
            dropdownField.choices = options;

            dropdownField.RegisterValueChangedCallback(evt => {
                var selectedValue = evt.newValue;
                var selectedIndex = dropdownField.choices.IndexOf(selectedValue);

                imageSource.SelectResolution(selectedIndex);
               // UpdateCustomResolution();
                if (!_solution.IsPaused)
                {
                    _solution.Play();
                }

                Debug.Log("resolucion  seleccionado: " + selectedIndex);
            });

            //dropdownField.RegisterCallback<ChangeEvent<string>>(OnDropdownValueChanged);



        }


        //private void UpdateCustomResolution(bool custom = false)
        //{
        //    var widthField = customResolutionField[1].InputField;
        //    var heightField = customResolutionField[2].InputField;
        //    var fpsField = customResolutionField[3].InputField;

        //    bool active = customResolutionField[0].Toggle.isOn;
        //    widthField.interactable = active;
        //    heightField.interactable = active;
        //    fpsField.interactable = active;
        //    resolutionField[0].Dropdown.interactable = !active;

        //    if (!active && custom)
        //    {
        //        UpdateResolutions();
        //    }

        //    var res = SettingsUtil.GetResolution(Settings.CameraResolution);
        //    widthField.SetTextWithoutNotify(res.width.ToString());
        //    heightField.SetTextWithoutNotify(res.height.ToString());
        //    fpsField.SetTextWithoutNotify(((int)res.frameRate).ToString());

        //    if (active)
        //    {
        //        // Values of zero means uninitialized
        //        UpdateCustomResolutionTest(0, 0, 0);
        //    }
        //}






        //void OnDropdownSourceValueChanged(int value)
        //{
        //    // Obtener el índice de la opción seleccionada
        //    int selectedIndex = dropdown.value;

        //    imageSource.SelectSource(value);
        //    if (!_solution.IsPaused)
        //    {
        //        _solution.Play();
        //    }
        //}








        void SettingsFlippedImage()
        {

            VisualElement btnFilppedCameraOff = rootUI.Q<VisualElement>("SwitchFlippOff");
            btnFilppedCameraOff.AddManipulator(new Clickable(evt => {

                VisualElement elementOn = rootUI.Q<VisualElement>("SwitchFlippOn");
                elementOn.style.display = DisplayStyle.Flex;
                btnFilppedCameraOff.style.display = DisplayStyle.None;

                Settings.CameraFlipped = true;
                imageSource.IsHorizontallyFlipped = true;
                if (!_solution.IsPaused)
                {
                    _solution.Play();
                }

            }));



            VisualElement btnFlippedCameraOn = rootUI.Q<VisualElement>("SwitchFlippOn");
            btnFlippedCameraOn.AddManipulator(new Clickable(evt => {

                VisualElement elementOn = rootUI.Q<VisualElement>("SwitchFlippOff");
                elementOn.style.display = DisplayStyle.Flex;
                btnFlippedCameraOn.style.display = DisplayStyle.None;

                Settings.CameraFlipped = false;
                imageSource.IsHorizontallyFlipped = false;
                if (!_solution.IsPaused)
                {
                    _solution.Play();
                }


            }));



        }


        /******---------  settings virtual camera  --------- **********/

        bool activeVirtualCamera = false;
        void SettingsVirtualCamera()
        {

            VisualElement btnVirtualCameraOff = rootUI.Q<VisualElement>("SwitchVCameraOff");
            btnVirtualCameraOff.AddManipulator(new Clickable(evt => {

                VisualElement elementOn = rootUI.Q<VisualElement>("SwitchVCameraOn");
                elementOn.style.display =  DisplayStyle.Flex;
                btnVirtualCameraOff.style.display = DisplayStyle.None;

                activeVirtualCamera = true;
                Settings.Temporary.VirtualCamera = activeVirtualCamera;
               
            }));



            VisualElement btnVirtualCameraOn = rootUI.Q<VisualElement>("SwitchVCameraOn");
            btnVirtualCameraOn.AddManipulator(new Clickable(evt => {

                VisualElement elementOn = rootUI.Q<VisualElement>("SwitchVCameraOff");
                elementOn.style.display = DisplayStyle.Flex;
                btnVirtualCameraOn.style.display = DisplayStyle.None;

                activeVirtualCamera = false;
                Settings.Temporary.VirtualCamera = activeVirtualCamera;               

            }));



            VisualElement btnInstallVcam = rootUI.Q<VisualElement>("btnInstallVCamera");
            btnInstallVcam.AddManipulator(new Clickable(evt => {
                CameraCapture.InstallVirtualCamera();
            }));


            VisualElement btnUninstallVcam = rootUI.Q<VisualElement>("btnUninstallVCamera");
            btnUninstallVcam.AddManipulator(new Clickable(evt => {
                CameraCapture.UninstallVirtualCamera();
            }));



        }



        /******Background******/


        public void SettingsBackground()
        {
            if(Settings.ShowCustomBackground)
            {
                rootUI.Q<VisualElement>("BackgroundActive").style.display = DisplayStyle.Flex;
                rootUI.Q<VisualElement>("BackgroundGreenActive").style.display = DisplayStyle.None;
            }
            else
            {
                rootUI.Q<VisualElement>("BackgroundActive").style.display = DisplayStyle.None;
                rootUI.Q<VisualElement>("BackgroundGreenActive").style.display = DisplayStyle.Flex;

            }

            VisualElement btnUploadImage = rootUI.Q<VisualElement>("BtnUploadImage");
            btnUploadImage.AddManipulator(new Clickable(evt => {
                var extensions = new[] {
                    new CustomExtensionFilter(Lang.DialogImageFiles.Get(), new string[] { "png", "jpg", "jpeg" }),
                    new CustomExtensionFilter(Lang.DialogAllFiles.Get(), "*"),
                };

                FileDialogUtils.OpenFilePanel(this, Lang.DialogOpenImage.Get(), Settings.ImageFile, extensions, false, (paths) => {
                    if (paths.Length > 0)
                    {
                        string filePath = paths[0];
                        guiMain.LoadCustomImage(filePath);
                    }
                });
            }));



            VisualElement btnBackgroundOn = rootUI.Q<VisualElement>("BtnBackground");
            btnBackgroundOn.AddManipulator(new Clickable(evt => {
                guiMain.SetShowBackgroundImage(true);

                rootUI.Q<VisualElement>("BackgroundActive").style.display = DisplayStyle.Flex;
                rootUI.Q<VisualElement>("BackgroundGreenActive").style.display = DisplayStyle.None;


            }));


            VisualElement btnBackgroundOff = rootUI.Q<VisualElement>("BtnBackgroundGreen");
            btnBackgroundOff.AddManipulator(new Clickable(evt => {
                guiMain.SetShowBackgroundImage(false);
                rootUI.Q<VisualElement>("BackgroundActive").style.display = DisplayStyle.None;
                rootUI.Q<VisualElement>("BackgroundGreenActive").style.display = DisplayStyle.Flex;

            }));


        }




        public void SettingsMaleFemale()
        {
            

            VisualElement btnMale = rootUI.Q<VisualElement>("btnMale");
            btnMale.AddManipulator(new Clickable(evt => {
                MaleOnCLick();
            }));


            VisualElement btnFemale = rootUI.Q<VisualElement>("btnFemale");
            btnFemale.AddManipulator(new Clickable(evt => {
                rootUI.Q<VisualElement>("selectorAvatar").style.display = DisplayStyle.None;
                rootUI.Q<VisualElement>("selectorMale").style.display = DisplayStyle.None;
                rootUI.Q<VisualElement>("selectorFemale").style.display = DisplayStyle.Flex;
                SetMaleFemaleVrm(false);
            }));


            VisualElement btnAvatar = rootUI.Q<VisualElement>("btnAvatar");
            btnAvatar.AddManipulator(new Clickable(evt => {
                RefreshOnClick();
            }));


            

        }


        private void MaleOnCLick()
        {
            rootUI.Q<VisualElement>("selectorAvatar").style.display = DisplayStyle.None;
            rootUI.Q<VisualElement>("selectorMale").style.display = DisplayStyle.Flex;
            rootUI.Q<VisualElement>("selectorFemale").style.display = DisplayStyle.None;
            SetMaleFemaleVrm(true);
        }





        /*****Start Camera Live******/


        bool isCameraShowing = false;

        private void SetCamera(bool enable)
        {
            
            VisualElement elementOn = rootUI.Q<VisualElement>("StartCameraOn");
            elementOn.style.display = (enable == true) ? DisplayStyle.Flex :  DisplayStyle.None;

            VisualElement elementOff = rootUI.Q<VisualElement>("StartCameraOff");
            elementOff.style.display = (enable == true) ? DisplayStyle.None : DisplayStyle.Flex; 

            isCameraShowing = enable;
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



        public void SettingsCamera()
        {            
            VisualElement btnStartCamOff = rootUI.Q<VisualElement>("StartCameraOff");
            btnStartCamOff.AddManipulator(new Clickable(evt =>  SetCamera(true)));

            VisualElement btnStartCamOn = rootUI.Q<VisualElement>("StartCameraOn");
            btnStartCamOn.AddManipulator(new Clickable(evt => SetCamera(false)));
        }


        void Update()
        {
        if (Input.GetKeyDown(KeyCode.Return))
            {             
                // Llama a la función que deseas ejecutar al presionar Enter
                SetCamera(!isCameraShowing);
            }
        }



        public void SetAccount(string name, string wallet)
        {   
            Label labelWallet = rootUI.Q<Label>("Wallet");
            Label labelUsername = rootUI.Q<Label>("Username");
            labelWallet.text = name;
            labelUsername.text = wallet;
        }



       


        public void SetMaleFemaleVrm(bool _showMale)
        {

            foreach (Transform hijo in parentMeshesVRM)
            {
                Destroy(hijo.gameObject);
            }


            bool showMale = (_showMale)? true: false;
            bool showFemale = (_showMale) ? false : true;
            meshesMale.vrm_hair.enabled = showMale;
            meshesMale.vrm_lower_bod.enabled = showMale;
            meshesMale.vrm_upper_body.enabled = showMale;
            meshesMale.vrm_feet.enabled = showMale;
            meshesMale.vrm_head.enabled = true;

            meshesMale.vrm_facial_hair.enabled = (showMale)?true:false;


            meshesFemale.vrm_hair.enabled = showFemale;
            meshesFemale.vrm_lower_bod.enabled = showFemale;
            meshesFemale.vrm_upper_body.enabled = showFemale;
            meshesFemale.vrm_feet.enabled = showFemale;
            meshesFemale.vrm_head.enabled = true;


        }


    }

   



}