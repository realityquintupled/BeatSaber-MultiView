using IllusionPlugin;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MultiView
{
    public class Plugin : IPlugin
    {
        public string Name => "MultiView";
        public string Version => "v1.2";
        private Config config = new Config(Path.Combine(Environment.CurrentDirectory, "MultiView.cfg"));
        private Camera multi = null;
        private bool isMulti;
        private bool IsMulti
        {
            get { return isMulti; }
            set
            {
                isMulti = value;
                multi.enabled = value;
                multi.depth = value ? 1023 : 0;
            }
        }
        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            isMulti = false;
            config.ConfigChangedEvent += (config) => { this.config = config; ReadConfig(); };
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.buildIndex > 1 && Camera.main != null && multi == null)
            {
                GameObject gameObj = Object.Instantiate(Camera.main.gameObject);
                gameObj.name = "Multi Camera";
                gameObj.tag = "Untagged";
                while (gameObj.transform.childCount > 0) Object.DestroyImmediate(gameObj.transform.GetChild(0).gameObject);
                Object.DestroyImmediate(gameObj.GetComponent("CameraRenderCallbacksManager"));
                Object.DestroyImmediate(gameObj.GetComponent("AudioListener"));
                Object.DestroyImmediate(gameObj.GetComponent("MeshCollider"));
                multi = gameObj.GetComponent<Camera>();
                multi.stereoTargetEye = StereoTargetEyeMask.None;
                multi.depth = 1023;
                multi.cullingMask &= ~(1 << 3);
                ReadConfig();
                Object.DontDestroyOnLoad(multi.gameObject);
            }
        }

        private void ReadConfig()
        {
            if (multi == null)
                return;
            multi.rect = new Rect(config.multiViewPosX, config.multiViewPosY, config.multiViewWidth, config.multiViewHeight);
            SetFOV();
            IsMulti = config.multiView;
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                IsMulti = !IsMulti;
            }
            if (multi != null)
            {
                multi.transform.position = Vector3.Lerp(multi.transform.position, Camera.main.transform.position, config.positionSmooth * Time.deltaTime);
                multi.transform.rotation = Quaternion.Slerp(multi.transform.rotation, Camera.main.transform.rotation, config.rotationSmooth * Time.deltaTime);
            }
        }

        public void OnFixedUpdate()
        {
        }

        protected virtual void SetFOV()
        {
            if (multi == null)
                return;
            var fov = (float)(57.2957801818848 * (2.0 * Mathf.Atan(Mathf.Tan((float)(config.fov * (Math.PI / 180.0) * 0.5)) / Camera.main.aspect)));
            multi.fieldOfView = fov;
        }
    }
}
