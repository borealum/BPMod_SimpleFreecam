using Il2Cpp;
using Il2CppBeautifyEffect;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[assembly: MelonInfo(typeof(BPMod_SimpleFreecam.Mod_SimpleFreecam), "BPMod_SimpleFreecam", "1.0.0", "Borealum", null)]
[assembly: MelonGame("Dogubomb", "BLUE PRINCE")]

namespace BPMod_SimpleFreecam
{
    public class Mod_SimpleFreecam : MelonMod
    {
        //configuration stuff
        String categoryID = "Borealum_SimpleFreecam";
        private static MelonPreferences_Category category;
        String freecamKeyID = "Borealum_SimpleFreecam_freecamKey";
        private static MelonPreferences_Entry<KeyCode> freecamKey;
        String freecamSpeedID = "Borealum_SimpleFreecam_freecamSpeed";
        private static MelonPreferences_Entry<float> freecamSpeed;
        String freecamSpeedMultID = "Borealum_SimpleFreecam_freecamSpeedMult";
        private static MelonPreferences_Entry<float> freecamSpeedMult;

        float rotation_speed = 1.0f;
        float speedMult;
        String fpsCharPath = "__SYSTEM/FPS Home/FPSController - Prince";

        public override void OnInitializeMelon()
        {
            category = MelonPreferences.CreateCategory(categoryID);
            freecamKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, freecamKeyID, KeyCode.F5, freecamKeyID, "Freecam toggle key. Default key is \"F5\". Should handle Unity KeyCode values: https://docs.unity3d.com/ScriptReference/KeyCode.html");
            LoggerInstance.Msg($"{freecamKeyID} value = {freecamKey.Value}");
            freecamSpeed = MelonPreferences.CreateEntry<float>(categoryID, freecamSpeedID, 0.05f, freecamKeyID, "Freecam speed. Default value = 0.05");
            LoggerInstance.Msg($"{freecamSpeedID} value = {freecamSpeed.Value}");
            freecamSpeedMult = MelonPreferences.CreateEntry<float>(categoryID, freecamSpeedMultID, 5.0f, freecamKeyID, "Freecam speed multiplier when holding shift. Default value = 5.0");
            LoggerInstance.Msg($"{freecamSpeedMultID} value = {freecamSpeedMult.Value}");
        }

        private Camera main_camera;
        private Camera our_camera;
        private GameObject cameraGO;
        private bool using_freecam = false;

        private void enable_freecam()
        {
            using_freecam = true;
            main_camera = Camera.main;
            if (main_camera != null)
            {
                GameObject.Find(fpsCharPath)?.GetComponent<PlayMakerFSM>()?.SendEvent("disable");
                main_camera.enabled = false;
                if (our_camera == null)
                {
                    //maybe there's a better way to just copy the camera? but this feels safer and I'm lazy
                    cameraGO = new GameObject("freecam");
                    our_camera = cameraGO.AddComponent<Camera>();
                    our_camera.gameObject.tag = "MainCamera";
                    our_camera.tag = main_camera.tag;
                    our_camera.allowHDR = main_camera.allowHDR;
                    our_camera.clearFlags = main_camera.clearFlags;
                    //commandbuffercount = 4
                    our_camera.cullingMask = main_camera.cullingMask;
                    our_camera.forceIntoRenderTexture = main_camera.forceIntoRenderTexture;
                    our_camera.nearClipPlane = main_camera.nearClipPlane;
                    our_camera.farClipPlane = main_camera.farClipPlane;
                    our_camera.renderingPath = main_camera.renderingPath;
                    
                    //post processing and effects stuff so the camera isn't dark
                    var srcPP = main_camera.GetComponent<PostProcessLayer>();
                    var dstPP = our_camera.gameObject.AddComponent<PostProcessLayer>();
                    if (srcPP != null)
                    {
                        dstPP.volumeLayer = srcPP.volumeLayer;
                        dstPP.volumeTrigger = our_camera.transform;
                        dstPP.antialiasingMode = srcPP.antialiasingMode;
                        dstPP.fastApproximateAntialiasing = srcPP.fastApproximateAntialiasing;
                        dstPP.temporalAntialiasing = srcPP.temporalAntialiasing;
                        dstPP.m_Resources = srcPP.m_Resources;
                    }

                    var srcVol = main_camera.GetComponent<PostProcessVolume>();
                    var dstVol = our_camera.gameObject.AddComponent<PostProcessVolume>();
                    if (srcVol != null)
                    {
                        dstVol.isGlobal = srcVol.isGlobal;
                        dstVol.sharedProfile = srcVol.sharedProfile;
                    }

                    Beautify dstBeau = cameraGO.AddComponent<Beautify>();
                    Beautify srcBeau = main_camera.gameObject.GetComponent<Beautify>();
                    if (srcBeau != null)
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(srcBeau), dstBeau);//Json-based copy of attributes

                    var srcSky = main_camera.GetComponent<SkyboxRenderer>();
                    SkyboxRenderer dstSky = cameraGO.AddComponent<SkyboxRenderer>();
                    if(srcSky != null)
                    {
                        dstSky.FaceMeshes = srcSky.FaceMeshes;
                        dstSky.GradientColor = srcSky.GradientColor;
                        dstSky.SkyScale = srcSky.SkyScale;
                        dstSky.UseFogColor = srcSky.UseFogColor;
                        dstSky._skyboxMatrix = srcSky._skyboxMatrix;
                    }
                }
                our_camera.transform.position = main_camera.transform.position;
                our_camera.transform.rotation = main_camera.transform.rotation;
                our_camera.gameObject.SetActive(true);
                our_camera.enabled = true;
            }
            else
            {
                MelonLogger.Error("error: no camera");
            }
        }

        private void disable_freecam()
        {
            using_freecam = false;
            if (main_camera != null)
            {
                GameObject.Find(fpsCharPath)?.GetComponent<PlayMakerFSM>()?.SendEvent("enable");
                main_camera.enabled = true;
            }

            if (our_camera != null)
            {
                UnityEngine.Object.Destroy(cameraGO.gameObject);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(freecamKey.Value))
            {
                if (using_freecam)
                    disable_freecam();
                else
                    enable_freecam();
            }

            if (using_freecam)
            {
                speedMult = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? freecamSpeedMult.Value : 1.0f;
                Vector3 movDir;
                float movRight = 0f;
                float movFwd = 0f;
                float movUp = 0f;
                
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    movRight = -1f;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    movRight = 1f;
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                    movFwd = -1f;
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                    movFwd = 1f;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.PageDown))
                    movUp = -1f;
                if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.PageUp))
                    movUp = 1f;

                movDir = our_camera.transform.right * movRight + our_camera.transform.forward * movFwd + our_camera.transform.up * movUp;
                our_camera.transform.position += movDir * freecamSpeed.Value * speedMult;

                /* mouse movement */
                our_camera.transform.Rotate(new UnityEngine.Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * rotation_speed);

                /* lock z rotation */
                if (our_camera.transform.eulerAngles.z != 0)
                    our_camera.transform.eulerAngles = new UnityEngine.Vector3(our_camera.transform.eulerAngles.x, our_camera.transform.eulerAngles.y, 0);
            }
        }
    }
}