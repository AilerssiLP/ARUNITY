using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ARFeatheredPlaneMeshVisualizerCompanion : MonoBehaviour
    {
        [SerializeField]
        Renderer m_PlaneRenderer;

        public Renderer planeRenderer
        {
            get => m_PlaneRenderer;
            set => m_PlaneRenderer = value;
        }

        [Range(0.1f, 1.0f)]
        [SerializeField]
        float m_FadeSpeed = 1f;

        public float fadeSpeed
        {
            get => m_FadeSpeed;
            set => m_FadeSpeed = value;
        }

        int m_ShaderAlphaPropertyID;
        float m_SurfaceVisualAlpha = 1f;
        float m_TweenProgress;
        Material m_PlaneMaterial;

#pragma warning disable CS0618 
        readonly FloatTweenableVariable m_AlphaTweenableVariable = new FloatTweenableVariable();
#pragma warning restore CS0618

        void Awake()
        {
            m_ShaderAlphaPropertyID = Shader.PropertyToID("_PlaneAlpha");
            m_PlaneMaterial = m_PlaneRenderer.material;
            visualizeSurfaces = true;
        }

        void OnDestroy()
        {
            m_AlphaTweenableVariable.Dispose();
        }

        void Update()
        {
            m_AlphaTweenableVariable.HandleTween(m_TweenProgress);
            m_TweenProgress += Time.unscaledDeltaTime * m_FadeSpeed;
            m_SurfaceVisualAlpha = m_AlphaTweenableVariable.Value;
            m_PlaneMaterial.SetFloat(m_ShaderAlphaPropertyID, m_SurfaceVisualAlpha);
        }

        public bool visualizeSurfaces
        {
            set
            {
                m_TweenProgress = 0f;
                m_AlphaTweenableVariable.target = value ? 1f : 0f;
                m_AlphaTweenableVariable.HandleTween(0f);
            }
        }
    }
}
