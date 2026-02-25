using Il2Cpp;
using Il2CppBeautifyEffect;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[assembly: MelonInfo(typeof(BPMod_SimpleFreecam.Mod_SimpleFreecam), "BPMod_SimpleFreecam", "2.0.5", "Borealum", null)]
[assembly: MelonGame("Dogubomb", "BLUE PRINCE")]

namespace BPMod_SimpleFreecam
{
    public enum ActiveFreecamType
    {
        OFF = 0,
        FREECAM = 1,
        TOP_DOWN_PERSPECTIVE = 2,
        TOP_DOWN_ORTHOGRAPHIC = 3
    }

    public class Mod_SimpleFreecam : MelonMod
    {
        //configuration stuff
        String categoryID = "Borealum_SimpleFreecam";
        private static MelonPreferences_Category category;
        String autoResetID = "Borealum_SimpleFreecam_autoReset";
        private static MelonPreferences_Entry<Boolean> autoReset;
        String freecamKeyID = "Borealum_SimpleFreecam_freecamKey";
        private static MelonPreferences_Entry<KeyCode> freecamKey;
        String topDownPerspectiveKeyID = "Borealum_SimpleFreecam_topDownPerspectiveKey";
        private static MelonPreferences_Entry<KeyCode> topDownPerspectiveKey;
        String topDownOrthographicKeyID = "Borealum_SimpleFreecam_topDownOrthographicKey";
        private static MelonPreferences_Entry<KeyCode> topDownOrthographicKey;
        String doorsKeyID = "Borealum_SimpleFreecam_doorsKey";
        private static MelonPreferences_Entry<KeyCode> doorsKey;
        String roomsKeyID = "Borealum_SimpleFreecam_roomsKey";
        private static MelonPreferences_Entry<KeyCode> roomsKey;
        String hudKeyID = "Borealum_SimpleFreecam_hudKey";
        private static MelonPreferences_Entry<KeyCode> hudKey;
        String resetKeyID = "Borealum_SimpleFreecam_resetKey";
        private static MelonPreferences_Entry<KeyCode> resetKey;
        String rotateKeyID = "Borealum_SimpleFreecam_rotateKey";
        private static MelonPreferences_Entry<KeyCode> rotateKey;

        String freecamSpeedID = "Borealum_SimpleFreecam_freecamSpeed";
        private static MelonPreferences_Entry<float> freecamSpeed;
        String freecamStep0ID = "Borealum_SimpleFreecam_freecamStep0";
        private static MelonPreferences_Entry<float> freecamStep0;
        String freecamStep1ID = "Borealum_SimpleFreecam_freecamStep1";
        private static MelonPreferences_Entry<float> freecamStep1;
        String freecamStep2ID = "Borealum_SimpleFreecam_freecamStep2";
        private static MelonPreferences_Entry<float> freecamStep2;

        String freecamSpeedMultID = "Borealum_SimpleFreecam_freecamSpeedMult";
        private static MelonPreferences_Entry<float> freecamSpeedMult;

        float rotation_speed = 1.0f;
        String fpsControllerPath = "__SYSTEM/FPS Home/FPSController - Prince";
        private GameObject fpsControllerGO;
        private GameObject hudGO;
        private GameObject roomsGO;
        private GameObject entranceGO;
        private GameObject antechamberGO;
        private List<GameObject> roomsList = new List<GameObject>();
        private bool showInfo = false;
        private float invisibleOffset = 0.0001f;//fake "0" clip plane distance offset
        private float fakeNearClip = 0;
        private float fakeOrthoSize = 5;

        public override void OnInitializeMelon()
        {
            category = MelonPreferences.CreateCategory(categoryID);
            autoReset = MelonPreferences.CreateEntry<Boolean>(categoryID, autoResetID, true, autoResetID, "Auto-reset camera postition to player when entering freecams. Default = true");
            LoggerInstance.Msg($"{autoResetID} value = {autoReset.Value}");
            freecamKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, freecamKeyID, KeyCode.F5, freecamKeyID, "Freecam key. Default is \"F5\". Should handle Unity KeyCode values: https://docs.unity3d.com/ScriptReference/KeyCode.html");
            LoggerInstance.Msg($"{freecamKeyID} value = {freecamKey.Value}");
            topDownPerspectiveKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, topDownPerspectiveKeyID, KeyCode.F6, topDownPerspectiveKeyID, "Top-down perspective view key. Default is \"F6\".");
            LoggerInstance.Msg($"{topDownPerspectiveKeyID} value = {topDownPerspectiveKey.Value}");
            topDownOrthographicKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, topDownOrthographicKeyID, KeyCode.F8, topDownOrthographicKeyID, "Top-down orthographic view key. Default is \"F8\".");
            LoggerInstance.Msg($"{topDownOrthographicKeyID} value = {topDownOrthographicKey.Value}");

            resetKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, resetKeyID, KeyCode.U, resetKeyID, "Camera reset key. Default is \"U\".");
            LoggerInstance.Msg($"{resetKeyID} value = {resetKey.Value}");
            doorsKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, doorsKeyID, KeyCode.O, doorsKeyID, "Hide/show closest doors key. Default is \"O\".");
            LoggerInstance.Msg($"{doorsKeyID} value = {doorsKey.Value}");
            hudKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, hudKeyID, KeyCode.I, hudKeyID, "Hide/show HUD key. Default is \"I\".");
            LoggerInstance.Msg($"{hudKeyID} value = {hudKey.Value}");
            roomsKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, roomsKeyID, KeyCode.P, roomsKeyID, "Hide/show other rooms key. Default is \"P\".");
            LoggerInstance.Msg($"{roomsKeyID} value = {roomsKey.Value}");
            rotateKey = MelonPreferences.CreateEntry<KeyCode>(categoryID, rotateKeyID, KeyCode.E, rotateKeyID, "Rotate camera 90 degrees. Default is \"E\".");
            LoggerInstance.Msg($"{rotateKeyID} value = {rotateKey.Value}");

            freecamSpeed = MelonPreferences.CreateEntry<float>(categoryID, freecamSpeedID, 0.1f, freecamSpeedID, "Freecam speed. Default value = 0.05");
            LoggerInstance.Msg($"{freecamSpeedID} value = {freecamSpeed.Value}");
            freecamStep0 = MelonPreferences.CreateEntry<float>(categoryID, freecamStep0ID, 1.0f, freecamStep0ID, "Freecam Step0. For top-down camera WASD adjustment. Default value = 1.0");
            LoggerInstance.Msg($"{freecamStep0ID} value = {freecamStep0.Value}");
            freecamStep1 = MelonPreferences.CreateEntry<float>(categoryID, freecamStep1ID, 0.1f, freecamStep1ID, "Freecam Step1. For top-down camera height adjustment. Default value = 0.10");
            LoggerInstance.Msg($"{freecamStep1ID} value = {freecamStep1.Value}");
            freecamStep2 = MelonPreferences.CreateEntry<float>(categoryID, freecamStep2ID, 0.1f, freecamStep2ID, "Freecam Step2. For top-down camera special adjustments (near clip plane/orthographic size). Default value = 0.10");
            LoggerInstance.Msg($"{freecamStep2ID} value = {freecamStep2.Value}");
            freecamSpeedMult = MelonPreferences.CreateEntry<float>(categoryID, freecamSpeedMultID, 5.0f, freecamSpeedMultID, "Freecam speed multiplier when holding shift. Default value = 5.0");
            LoggerInstance.Msg($"{freecamSpeedMultID} value = {freecamSpeedMult.Value}");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != null && sceneName.Equals("Mount Holly Estate"))
            {
                fpsControllerGO = GameObject.Find(fpsControllerPath);
                hudGO = GameObject.Find("__SYSTEM/HUD");
                roomsGO = GameObject.Find("__SYSTEM/Room Spawn Pools");
                entranceGO = GameObject.Find("ROOMS/Entrance Hall");
                antechamberGO = GameObject.Find("ROOMS/Antechamber");
                showInfo = false;
            }
            roomsList.Clear();
            cameras.Clear();
        }

        private GameObject FindClosestWithName(string namePart)
        {
            GameObject closest = null;
            if (fpsControllerGO != null)
            {
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                float closestDistance = Mathf.Infinity;
                foreach (GameObject obj in allObjects)
                {
                    if (!obj.name.Contains(namePart))
                        continue;
                    float distance = Vector3.Distance(fpsControllerGO.transform.position, obj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = obj;
                    }
                }
            }
            return closest;
        }

        private Camera main_camera;
        private Dictionary<ActiveFreecamType, Camera> cameras = new Dictionary<ActiveFreecamType, Camera>();
        private ActiveFreecamType activeFreecamType = ActiveFreecamType.OFF;
        private Camera activeFreecam = null;

        private void switchFreecam(ActiveFreecamType type)
        {
            if (activeFreecamType != ActiveFreecamType.OFF)
            {
                disable_freecam();
                return;
            }

            main_camera = Camera.main;
            if (main_camera != null)
            {
                fpsControllerGO.GetComponent<PlayMakerFSM>()?.SendEvent("disable");
                main_camera.enabled = false;
                Camera freecam_camera = null;
                if (!cameras.TryGetValue(type, out freecam_camera))
                {
                    freecam_camera = buildCameraGO(type);
                    cameras[type] = freecam_camera;
                    resetActiveCameraPosition();
                }
                else
                {
                    freecam_camera = cameras[type];
                }
                freecam_camera.gameObject.SetActive(true);
                freecam_camera.enabled = true;
                activeFreecamType = type;
                activeFreecam = freecam_camera;
                if (autoReset.Value)
                {
                    resetActiveCameraPosition();
                }
            }
            else
            {
                MelonLogger.Error("error: no camera");
            }
        }

        private float gridSize = 10f;
        private float offset = 5f;

        private void resetActiveCameraPosition()
        {
            if (activeFreecamType == ActiveFreecamType.FREECAM)
            {
                activeFreecam.transform.position = main_camera.transform.position;
                activeFreecam.transform.rotation = main_camera.transform.rotation;
            } 
            else if (activeFreecamType == ActiveFreecamType.TOP_DOWN_PERSPECTIVE || activeFreecamType == ActiveFreecamType.TOP_DOWN_ORTHOGRAPHIC)
            {
                //snap to closest tile center, keep height
                Vector3 pos = main_camera.transform.position;
                pos.x = Mathf.Round((pos.x - offset) / gridSize) * gridSize + offset;
                pos.z = Mathf.Round((pos.z - offset) / gridSize) * gridSize + offset;
                pos.y = activeFreecam.transform.position.y;
                activeFreecam.transform.position = pos;
            }
        }

        private Camera buildCameraGO(ActiveFreecamType type)
        {
            GameObject freecamGO = new GameObject("freecam");
            Camera freecam_camera = freecamGO.AddComponent<Camera>();

            //freecam_camera.CopyFrom(main_camera); I don't trust this

            //freecam_camera.gameObject.tag = "MainCamera";
            freecam_camera.tag = main_camera.tag;
            freecam_camera.allowHDR = main_camera.allowHDR;
            freecam_camera.clearFlags = main_camera.clearFlags;
            //commandbuffercount = 4
            freecam_camera.cullingMask = main_camera.cullingMask;
            freecam_camera.forceIntoRenderTexture = main_camera.forceIntoRenderTexture;
            freecam_camera.nearClipPlane = main_camera.nearClipPlane;
            freecam_camera.farClipPlane = main_camera.farClipPlane;
            freecam_camera.renderingPath = main_camera.renderingPath;

            //post processing and effects stuff so the camera isn't dark
            var srcPP = main_camera.GetComponent<PostProcessLayer>();
            var dstPP = freecam_camera.gameObject.AddComponent<PostProcessLayer>();
            if (srcPP != null)
            {
                dstPP.volumeLayer = srcPP.volumeLayer;
                dstPP.volumeTrigger = freecam_camera.transform;
                dstPP.antialiasingMode = srcPP.antialiasingMode;
                dstPP.fastApproximateAntialiasing = srcPP.fastApproximateAntialiasing;
                dstPP.temporalAntialiasing = srcPP.temporalAntialiasing;
                dstPP.m_Resources = srcPP.m_Resources;
            }

            var srcVol = main_camera.GetComponent<PostProcessVolume>();
            var dstVol = freecam_camera.gameObject.AddComponent<PostProcessVolume>();
            if (srcVol != null)
            {
                dstVol.isGlobal = srcVol.isGlobal;
                dstVol.sharedProfile = srcVol.sharedProfile;
            }

            Beautify dstBeau = freecamGO.AddComponent<Beautify>();
            Beautify srcBeau = main_camera.gameObject.GetComponent<Beautify>();
            if (srcBeau != null)
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(srcBeau), dstBeau);//Json-based copy of attributes

            var srcSky = main_camera.GetComponent<SkyboxRenderer>();
            SkyboxRenderer dstSky = freecamGO.AddComponent<SkyboxRenderer>();
            if (srcSky != null)
            {
                dstSky.FaceMeshes = srcSky.FaceMeshes;
                dstSky.GradientColor = srcSky.GradientColor;
                dstSky.SkyScale = srcSky.SkyScale;
                dstSky.UseFogColor = srcSky.UseFogColor;
                dstSky._skyboxMatrix = srcSky._skyboxMatrix;
            }

            //top - down cameras extra stuff
            
            Quaternion rot = Quaternion.LookRotation(Vector3.down, Vector3.forward);
    
            if (type == ActiveFreecamType.TOP_DOWN_PERSPECTIVE)
            {
                Vector3 pos = new Vector3(0, 12.5f, 0);
                freecam_camera.transform.position = pos;
                freecam_camera.transform.rotation = rot;
            }
            if (type == ActiveFreecamType.TOP_DOWN_ORTHOGRAPHIC)
            {
                Vector3 pos = new Vector3(0, 10.0f, 0);
                freecam_camera.transform.position = pos;
                freecam_camera.transform.rotation = rot;
                freecam_camera.orthographic = true;
            }
            return freecam_camera;
        }

        private void disable_freecam()
        {
            Camera freecam_camera = cameras[activeFreecamType];
            if (freecam_camera != null)
            {
                freecam_camera.enabled = false;
            }
            activeFreecamType = ActiveFreecamType.OFF;
            activeFreecam = null;
            if (main_camera != null)
            {
                fpsControllerGO.GetComponent<PlayMakerFSM>()?.SendEvent("enable");
                main_camera.enabled = true;
            }
        }

        public override void OnUpdate()
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(freecamKey.Value))
            {
                switchFreecam(ActiveFreecamType.FREECAM);
            }
            else if (Input.GetKeyDown(topDownPerspectiveKey.Value))
            {
                switchFreecam(ActiveFreecamType.TOP_DOWN_PERSPECTIVE);
            }
            else if (Input.GetKeyDown(topDownOrthographicKey.Value))
            {
                switchFreecam(ActiveFreecamType.TOP_DOWN_ORTHOGRAPHIC);
            }

            if (Input.GetKeyDown(hudKey.Value))
            {
                if (shift)
                {
                    showInfo = !showInfo;
                }
                else
                {
                    hudGO?.SetActive(!hudGO.active);
                }
            }
            if (Input.GetKeyDown(doorsKey.Value))
            {
                GameObject gmply = FindClosestWithName("_GAMEPLAY");
                GameObject doors = null;
                for (int i = 0; i < gmply.transform.childCount; i++)
                {
                    Transform child = gmply.transform.GetChild(i);
                    if (child.name == "_DOORS")
                    {
                        doors = child.gameObject;
                        break;
                    }
                }
                doors?.SetActive(!doors.active);
            }
            if (Input.GetKeyDown(roomsKey.Value))
            {
                if (roomsGO != null)//to make sure we are in the right scene
                {
                    if (roomsList.Count!=0)//turn on last turned off rooms
                    {
                        foreach (GameObject go in roomsList)
                        {
                            go.SetActive(true);
                        }
                        roomsList.Clear();
                    }
                    else
                    {
                        GameObject currRoom = FindClosestWithName("_GAMEPLAY").transform.GetParent().gameObject;
                        for (int i = 0; i < roomsGO.transform.childCount; i++)
                        {
                            GameObject childObject = roomsGO.transform.GetChild(i).gameObject;
                            if (childObject != currRoom)
                            {
                                roomsList.Add(childObject);
                            }
                        }
                        if(currRoom != entranceGO)
                            roomsList.Add(entranceGO);
                        if (currRoom != antechamberGO)
                            roomsList.Add(antechamberGO);
                        foreach (GameObject go in roomsList)
                        {
                            go.SetActive(false);
                        }
                    }
                }
            }
            if (Input.GetKeyDown(resetKey.Value))
            {
                resetActiveCameraPosition();
            }
            if (Input.GetKeyDown(rotateKey.Value))
            {
                activeFreecam.transform.rotation = Quaternion.AngleAxis(-90f, Vector3.up) * activeFreecam.transform.rotation;
            }
            //movement controls
            if(activeFreecamType == ActiveFreecamType.OFF)
            {
                return;
            }
            //handle axis movement
            float speedMult = shift ? freecamSpeedMult.Value : 1.0f;
            Vector3 movDir;
            float movRight = 0f;
            float movFwd = 0f;
            float movUp = 0f;
            float movSpecial = 0f;

            if (activeFreecamType == ActiveFreecamType.FREECAM)
            {
                //this one moves continuously
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
                movDir = activeFreecam.transform.right * movRight + activeFreecam.transform.forward * movFwd + activeFreecam.transform.up * movUp;
                activeFreecam.transform.position += movDir * freecamSpeed.Value * speedMult;

                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    freecamSpeed.Value += 0.005f;
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
                    freecamSpeed.Value -= 0.005f;
                activeFreecam.transform.Rotate(new UnityEngine.Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * rotation_speed);
                // lock z rotation
                if (activeFreecam.transform.eulerAngles.z != 0)
                    activeFreecam.transform.eulerAngles = new UnityEngine.Vector3(activeFreecam.transform.eulerAngles.x, activeFreecam.transform.eulerAngles.y, 0);
            }
            else if(activeFreecamType == ActiveFreecamType.TOP_DOWN_PERSPECTIVE) 
            {
                //these move in steps, but like on a 2D plane (forward+backward and up+down are switched around and their positive/negative too)
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    movRight = -1f;
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    movRight = 1f;
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    movUp = -1f;
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    movUp = 1f;
                if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.PageDown))
                    movFwd = 1f;
                if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.PageUp))
                    movFwd = -1f;
                movDir = activeFreecam.transform.right * movRight * freecamStep0.Value + activeFreecam.transform.up * movUp * freecamStep0.Value;
                movDir += activeFreecam.transform.forward * movFwd * freecamStep1.Value;
                activeFreecam.transform.position += movDir * speedMult;

                //near clip plane
                if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Home))
                    movSpecial = 1f;
                if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.End))
                    movSpecial = -1f;
                fakeNearClip += movSpecial * freecamStep2.Value * speedMult;
                fakeNearClip = Math.Max(fakeNearClip, 0);
                activeFreecam.nearClipPlane = fakeNearClip + invisibleOffset;
            }
            else if(activeFreecamType == ActiveFreecamType.TOP_DOWN_ORTHOGRAPHIC)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    movRight = -1f;
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    movRight = 1f;
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    movUp = -1f;
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    movUp = 1f;
                if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.PageDown))
                    movFwd = 1f;
                if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.PageUp))
                    movFwd = -1f;
                movDir = activeFreecam.transform.right * movRight * freecamStep0.Value + activeFreecam.transform.up * movUp * freecamStep0.Value;
                movDir += activeFreecam.transform.forward * movFwd * freecamStep1.Value;
                activeFreecam.transform.position += movDir * speedMult;
        
                if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Home))
                    movSpecial = -1f;
                if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.End))
                    movSpecial = 1f;
                fakeOrthoSize += movSpecial * freecamStep2.Value * speedMult;
                fakeOrthoSize = Math.Max(fakeOrthoSize, 0);
                activeFreecam.orthographicSize = fakeOrthoSize;
            }
        }

        private Rect windowRect = new Rect(20, Screen.height - 200, 250, 150);
        public override void OnGUI()
        {
            if (!showInfo) return;
            windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)DrawWindow, "Freecam info");
        }

        private void DrawWindow(int windowID)
        {
            String type = "";
            String lineSpecial = "";
            if (activeFreecamType == ActiveFreecamType.OFF)
            {
                type = "Off";
            }
            else if (activeFreecamType == ActiveFreecamType.FREECAM)
            {
                type = "Freecam";
            }
            else if (activeFreecamType == ActiveFreecamType.TOP_DOWN_PERSPECTIVE)
            {
                type = "Perspective";
                lineSpecial = $"Near clip: {fakeNearClip:F3}";
            }
            else if (activeFreecamType == ActiveFreecamType.TOP_DOWN_ORTHOGRAPHIC)
            {
                type = "Orthographic"; 
                lineSpecial = $"Size: {fakeOrthoSize:F3}";
            }
            GUILayout.Label($"Type: {type}");
            if (activeFreecam != null)
            {
                Vector3 p = activeFreecam.transform.position;
                Vector3 q = activeFreecam.transform.eulerAngles;
                GUILayout.Label($"Pos: {p.x:F3}, {p.y:F3}, {p.z:F3}");
                GUILayout.Label($"Rot: {q.x:F3}, {q.y:F3}, {q.z:F3}");
            }
            GUILayout.Label(lineSpecial);
            GUI.DragWindow();
        }
    }
}