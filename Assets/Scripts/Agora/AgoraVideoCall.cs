using UnityEngine;
using Agora.Rtc;

public class AgoraVideoCall : MonoBehaviour
{
    private IRtcEngine mRtcEngine;

    [Header("Configuración de Agora")]
    public string AppID = "83ce9e0760c6460c9a8ca139b412cc5a"; // ← Reemplaza con tu App ID real
    public string ChannelName = "MetaverseChannel";

    [Header("Plano donde se mostrará el video remoto (usa VideoSurface)")]
    public GameObject videoPlane;

    void Start()
    {
        if (!InitializeAgoraEngine())
        {
            Debug.LogError("Fallo al inicializar Agora Engine");
            return;
        }

        // Activar video
        mRtcEngine.EnableVideo();

        // Opciones para unirse al canal
        ChannelMediaOptions options = new ChannelMediaOptions();

        // Activar suscripción automática a audio/video del remoto
        options.autoSubscribeAudio = new Optional<bool>();
        options.autoSubscribeAudio.SetValue(true);

        options.autoSubscribeVideo = new Optional<bool>();
        options.autoSubscribeVideo.SetValue(true);

        // Unirse al canal (sin token si está deshabilitado en la consola de Agora)
        mRtcEngine.JoinChannel("", ChannelName, 0, options);
    }

    private bool InitializeAgoraEngine()
    {
        try
        {
            mRtcEngine = RtcEngine.CreateAgoraRtcEngine();

            var context = new RtcEngineContext(
                AppID,
                0, // context
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                null, // licencia
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
                AREA_CODE.AREA_CODE_GLOB,
                null, // logConfig opcional
                null, // prioridad de hilo
                false, // EGL externo
                false, // multiproceso
                false  // domainLimit
            );

            int result = mRtcEngine.Initialize(context);

            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            // Asignar manejador de eventos
            mRtcEngine.InitEventHandler(new UserEventHandler(this));

            return result == 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al inicializar Agora: " + e.Message);
            return false;
        }
    }

    void OnDestroy()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine.Dispose();
            mRtcEngine = null;
        }
    }

    // Manejador de eventos de Agora
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private AgoraVideoCall caller;

        public UserEventHandler(AgoraVideoCall caller)
        {
            this.caller = caller;
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log("[Agora] Usuario remoto conectado: " + uid);

            if (caller.videoPlane != null)
            {
                var surface = caller.videoPlane.GetComponent<VideoSurface>();
                if (surface != null)
                {
                    // Asignar el video del usuario remoto al plano
                    surface.SetForUser(uid, caller.ChannelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
                    surface.SetEnable(true);
                }
                else
                {
                    Debug.LogError("No se encontró componente VideoSurface en el GameObject.");
                }
            }
        }
    }
}
