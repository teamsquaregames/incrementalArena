using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lean.Pool
{
	[ExecuteInEditMode]
	[AddComponentMenu(LeanPool.ComponentPathPrefix + "Particle System Pool")]
	public class LeanParticleSystemPool : LeanComponentPool<ParticleSystem>
	{
	}


#if UNITY_EDITOR
	namespace Lean.Pool.Editor
	{
		using CW.Common;
		using UnityEditor;
		using TARGET = LeanParticleSystemPool;

		[CanEditMultipleObjects]
		[CustomEditor(typeof(TARGET))]
		public class LeanParticleSystemPool_Editor : CwEditor
		{
			private static List<ParticleSystem> tempClones = new List<ParticleSystem>();

			[System.NonSerialized] TARGET tgt; [System.NonSerialized] TARGET[] tgts;

			protected override void OnInspector()
			{
				GetTargets(out tgt, out tgts);

				BeginError(Any(tgts, t => t.Prefab == null));
				if (Draw("prefab", "The prefab this pool controls.") == true)
				{
					Each(tgts, t => { t.Prefab = (ParticleSystem)serializedObject.FindProperty("prefab").objectReferenceValue; }, true);
				}
				EndError();
				Draw("notification", "If you need to perform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed. None = If you use this then you must rely on the OnEnable and OnDisable messages. SendMessage = The prefab clone is sent the OnSpawn and OnDespawn messages. BroadcastMessage = The prefab clone and all its children are sent the OnSpawn and OnDespawn messages. IPoolable = The prefab clone's components implementing IPoolable are called. Broadcast IPoolable = The prefab clone and all its child components implementing IPoolable are called.");
				Draw("strategy", "This allows you to control how spawned/despawned ParticleSystem will be handled. The DeactivateViaHierarchy mode should be used if you need to maintain your prefab's de/activation state.\n\nActivateAndDeactivate = Despawned clones will be deactivated and placed under this ParticleSystem.\n\nDeactivateViaHierarchy = Despawned clones will be placed under a deactivated ParticleSystem and left alone.");
				Draw("preload", "Should this pool preload some clones?");
				Draw("capacity", "Should this pool have a maximum amount of spawnable clones?");
				Draw("recycle", "If the pool reaches capacity, should new spawns force older ones to despawn?");
				Draw("persist", "Should this pool be marked as DontDestroyOnLoad?");
				Draw("stamp", "Should the spawned clones have their clone index appended to their name?");
				Draw("warnings", "Should detected issues be output to the console?");

				Separator();

				BeginDisabled();
				DrawClones("Spawned", true, false, "notification");
				DrawClones("Despawned", false, true, "strategy");
				DrawClones("Total", true, true, "preload");
				EndDisabled();

				if (Application.isPlaying == false)
				{
					if (Any(tgts, t => t.DespawnedClonesMatch == false))
					{
						Warning("Your preloaded clones no longer match the Prefab.");
					}
				}
			}

			private void DrawClones(string title, bool spawned, bool despawned, string propertyName)
			{
				var property = serializedObject.FindProperty(propertyName);
				var rect = EditorGUILayout.BeginVertical(); EditorGUILayout.LabelField(string.Empty, GUILayout.Height(EditorGUI.GetPropertyHeight(property))); EditorGUILayout.EndVertical();
				var rectF = rect; rectF.height = 16;

				tgt.GetClones(tempClones, spawned, despawned);

				property.isExpanded = EditorGUI.Foldout(rectF, property.isExpanded, GUIContent.none);

				UnityEditor.EditorGUI.IntField(rect, title, tempClones.Count);

				if (property.isExpanded == true)
				{
					foreach (var clone in tempClones)
					{
						EditorGUILayout.ObjectField(GUIContent.none, clone, typeof(ParticleSystem), true);
					}
				}
			}

			[UnityEditor.MenuItem("GameObject/Lean/ParticleSystem Pool", false, 1)]
			private static void CreateLocalization()
			{
				var particleSystem = new GameObject(typeof(LeanParticleSystemPool).Name);

				UnityEditor.Undo.RegisterCreatedObjectUndo(particleSystem, "Create LeanGameObjectPool");

				particleSystem.AddComponent<LeanParticleSystemPool>();

				UnityEditor.Selection.activeGameObject = particleSystem;
			}
		}
	}
#endif
}