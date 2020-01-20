using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Ludiq.PeekCore;

[assembly: MapToPlugin(typeof(LudiqCoreConfiguration), LudiqCore.ID)]

namespace Ludiq.PeekCore
{
	public sealed class LudiqCoreConfiguration : PluginConfiguration
	{
		private LudiqCoreConfiguration(LudiqCore plugin) : base(plugin) { }

		public override void LateInitialize()
		{
			base.LateInitialize();
		}

		#region Editor Prefs

		private bool _humanNaming = true;

		private LanguageIconsSkin _languageIconsSkin = LanguageIconsSkin.VisualStudioMonochrome;
		
		public event Action namingSchemeChanged;

		public override string header => "Core";

		/// <summary>
		/// Whether programming names should be converted into a more human-readable format.
		/// </summary>
		[EditorPref(visible = true, resettable = true)]
		public bool humanNaming
		{
			get => _humanNaming;
			set
			{
				_humanNaming = value;
				namingSchemeChanged?.Invoke();
			}
		}
		
		/// <summary>
		/// Whether the Unity object fields should use the fuzzy finder instead
		/// of the default object picker window.
		/// </summary>
		[EditorPref]
		public bool fuzzyObjectPicker { get; set; } = true;

		/// <summary>
		/// The maximum amount of search results to display.
		/// </summary>
		[EditorPref(visible = true, resettable = true)]
		public int maxSearchResults { get; set; } = 50;
		
		/// <summary>
		/// Whether inherited below should be grouped at the bottom of the options list.
		/// </summary>
		[EditorPref]
		public bool groupInheritedMembers { get; set; } = true;
		
		/// <summary>
		/// Whether the fuzzy finder should display options that are obsolete.
		/// </summary>
		[EditorPref]
		public bool obsoleteOptions { get; set; } = false;
		
		/// <summary>
		/// The skin to use for language related (C# / VB) icons.
		/// </summary>
		[EditorPref]
		public LanguageIconsSkin LanguageIconsSkin
		{
			get => _languageIconsSkin;
			set
			{
				_languageIconsSkin = value;
				Icons.Language.skin = value;
			}
		}
		
		/// <summary>
		/// Whether the height of the fuzzy finder should be limited to the
		/// main editor window height. This is meant to fix Y offset issues on OSX,
		/// but will cut the fuzzy finder if this window is not maximized to the screen size.
		/// </summary>
		[EditorPref(visibleCondition = nameof(isEditorOSX))]
		public bool limitFuzzyFinderHeight { get; set; } = true;
		
		/// <summary>
		/// Enables additional options and logging for debugging purposes.
		/// </summary>
		[EditorPref(resettable = false)]
		public new bool developerMode { get; set; } = false;
		
		[EditorPref(visibleCondition = nameof(developerMode))]
		public bool developerEditorMenu { get; set; } = false;

		/// <summary>
		/// Whether the log should track accessor state.
		/// </summary>
		[EditorPref(visibleCondition = nameof(developerMode))]
		public bool trackAccessorState { get; set; } = false;

		/// <summary>
		/// Whether additional helpers should be shown in the inspector for debugging and profiling.
		/// </summary>
		[EditorPref(visibleCondition = nameof(developerMode))]
		public bool debugInspectorGUI { get; set; } = false;

		// Needs to be proptected to avoid stripping
		private bool isEditorOSX => Application.platform == RuntimePlatform.OSXEditor;

		#endregion

		#region Project Settings
		
		/// <summary>
		/// Whether the project was updated from Bolt 1.
		/// </summary>
		[ProjectSetting]
		public bool legacyProject { get; set; } = false;
		
		[ProjectSetting(visible = false, resettable = false)]
		public HashSet<Member> favoriteMembers { get; set; } = new HashSet<Member>();

		/// <summary>
		/// The assemblies available for reflection.
		/// </summary>
		[ProjectSetting("assemblyOptions", visible = false, resettable = false)]
		public List<LooseAssemblyName> legacyAssemblyOptions { get; private set; } = new List<LooseAssemblyName>()
		{
			// .NET
			"mscorlib",

			// User
			"Assembly-CSharp-firstpass",
			"Assembly-CSharp",
			
			// Core
			"UnityEngine",
			"UnityEngine.CoreModule",

			// Input
			"UnityEngine.InputModule",
			"UnityEngine.ClusterInputModule",

			// Physics
			"UnityEngine.PhysicsModule",
			"UnityEngine.Physics2DModule",
			"UnityEngine.TerrainPhysicsModule",
			"UnityEngine.VehiclesModule",

			// Audio
			"UnityEngine.AudioModule",

			// Animation
			"UnityEngine.AnimationModule",
			"UnityEngine.VideoModule",
			"UnityEngine.DirectorModule",
			"UnityEngine.Timeline",
			
			// Effects
			"UnityEngine.ParticleSystemModule",
			"UnityEngine.ParticlesLegacyModule",
			"UnityEngine.WindModule",
			"UnityEngine.ClothModule",

			// 2D
			"UnityEngine.TilemapModule",
			"UnityEngine.SpriteMaskModule",

			// Rendering
			"UnityEngine.TerrainModule",
			"UnityEngine.ImageConversionModule",
			"UnityEngine.TextRenderingModule",
			"UnityEngine.ClusterRendererModule",
			"UnityEngine.ScreenCaptureModule",

			// AI
			"UnityEngine.AIModule",
			
			// UI
			"UnityEngine.UI",
			"UnityEngine.UIModule",
			"UnityEngine.IMGUIModule",
			"UnityEngine.UIElementsModule",
			"UnityEngine.StyleSheetsModule",
			
			// XR
			"UnityEngine.VR",
			"UnityEngine.VRModule",
			"UnityEngine.ARModule",
			"UnityEngine.HoloLens",
			"UnityEngine.SpatialTracking",
			"UnityEngine.GoogleAudioSpatializer",

			// Networking
			"UnityEngine.Networking",

			// Services
			"UnityEngine.Analytics",
			"UnityEngine.Advertisements",
			"UnityEngine.Purchasing",
			"UnityEngine.UnityConnectModule",
			"UnityEngine.UnityAnalyticsModule",
			"UnityEngine.GameCenterModule",
			"UnityEngine.AccessibilityModule",
		};

		/// <summary>
		/// The list of types available in the inspector.
		/// </summary>
		[ProjectSetting("typeOptions", visible = false, resettable = false)]
		public List<Type> legacyTypeOptions { get; private set; } = new List<Type>()
		{
			typeof(object),
			typeof(bool),
			typeof(int),
			typeof(float),
			typeof(string),
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Quaternion),
			typeof(Matrix4x4),
			typeof(Rect),
			typeof(Bounds),
			typeof(Color),
			typeof(AnimationCurve),
			typeof(LayerMask),
			typeof(Ray),
			typeof(Ray2D),
			typeof(RaycastHit),
			typeof(RaycastHit2D),
			typeof(ContactPoint),
			typeof(ContactPoint2D),
			typeof(ParticleCollisionEvent),
			typeof(Scene),
			
			typeof(Application),
			typeof(UnityEngine.Resources),
			typeof(Mathf),
			typeof(Debug),
			typeof(Input),
			typeof(Touch),
			typeof(Screen),
			typeof(Cursor),
			typeof(Time),
			typeof(Random),
			typeof(Physics),
			typeof(Physics2D),
			typeof(SceneManager),
			typeof(GUI),
			typeof(GUILayout),
			typeof(GUIUtility),
			typeof(AudioMixerGroup),
			typeof(NavMesh),
			typeof(Gizmos),
			typeof(AnimatorStateInfo),
			typeof(BaseEventData),
			typeof(PointerEventData),
			typeof(AxisEventData),

			typeof(IList),
			typeof(IDictionary),
			#pragma warning disable 618
			typeof(AotList),
			typeof(AotDictionary),
			#pragma warning restore 618

			typeof(Exception),
		};

		#endregion
	}
}