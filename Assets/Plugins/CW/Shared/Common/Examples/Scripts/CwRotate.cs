using UnityEngine;

namespace CW.Common
{
	/// <summary>This component rotates the current <b>Transform</b>.</summary>
	[ExecuteAlways]
	[HelpURL(CwShared.HelpUrlPrefix + "CwRotate")]
	[AddComponentMenu(CwShared.ComponentMenuPrefix + "Rotate")]
	public class CwRotate : MonoBehaviour
	{
		/// <summary>The speed of the rotation in degrees per second.</summary>
		public Vector3 AngularVelocity { set { angularVelocity = value; } get { return angularVelocity; } } [SerializeField] private Vector3 angularVelocity = Vector3.up;

		/// <summary>The rotation space.</summary>
		public Space RelativeTo { set { relativeTo = value; } get { return relativeTo; } } [SerializeField] private Space relativeTo;

		[SerializeField, HideInInspector] private float lastRealtime;

		protected virtual void OnEnable()
		{
			lastRealtime = Time.realtimeSinceStartup;
		}

		protected virtual void Update()
		{
			var now   = Time.realtimeSinceStartup;
			var delta = Application.isPlaying ? Time.deltaTime : Mathf.Max(0.0f, now - lastRealtime);

			lastRealtime = now;

			if (delta > 0.0f)
			{
				transform.Rotate(angularVelocity * delta, relativeTo);
			}
		}
	}
}

#if UNITY_EDITOR
namespace CW.Common
{
	using UnityEditor;
	using TARGET = CwRotate;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwRotate_Editor : CwEditor
	{
		private const float EditStepSeconds = 0.1f;

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.AngularVelocity.magnitude == 0.0f));
				Draw("angularVelocity", "The speed of the rotation in degrees per second.");
			EndError();
			Draw("relativeTo", "The rotation space.");

			if (GUILayout.Button("Rotate In Edit Mode"))
			{
				Undo.RecordObjects(tgts, "Rotate In Edit Mode");

				for (var i = 0; i < tgts.Length; i++)
				{
					var current = tgts[i];
					if (current == null) continue;

					current.transform.Rotate(current.AngularVelocity * EditStepSeconds, current.RelativeTo);
					EditorUtility.SetDirty(current.transform);
				}
			}
		}
	}
}

	
#endif