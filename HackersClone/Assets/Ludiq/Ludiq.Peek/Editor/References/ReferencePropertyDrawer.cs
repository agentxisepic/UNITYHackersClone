using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;
	
	[CustomPropertyDrawer(typeof(UnityObject), true)]
	public class ReferencePropertyDrawer : PropertyDrawer
	{
		private static Event e => Event.current;

		public static EditorWindow lastPopup;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UnityEditorDynamic.EditorGUI.DefaultPropertyField(position, property, label);

			if (PeekPlugin.Configuration.enableReferenceInspector && !property.hasMultipleDifferentValues && property.objectReferenceValue != null && label != GUIContent.none)
			{
				var buttonPosition = new Rect
				(
					position.x + EditorGUIUtility.labelWidth - IconSize.Small - 1,
					position.y,
					IconSize.Small,
					IconSize.Small
				);

				var isActive = PopupWatcher.IsOpenOrJustClosed(lastPopup);
				
				var activatedButton = LudiqGUI.DropdownToggle(buttonPosition, isActive, LudiqGUIUtility.TempContent(PeekPlugin.Icons.propertyDrawer?[IconSize.Small]), GUIStyle.none);
				
				if (activatedButton && !isActive)
				{
					PopupWatcher.Release(lastPopup);
					lastPopup = null;

					var targets = new[] {property.objectReferenceValue};
					var activatorGuiPosition = buttonPosition;
					var activatorScreenPosition = LudiqGUIUtility.GUIToScreenRect(activatorGuiPosition);

					if (e.IsContextMouseButton())
					{
						if (property.objectReferenceValue is GameObject go)
						{
							GameObjectContextMenu.Open(new[] {go}, activatorScreenPosition);
						}
						else
						{
							UnityObjectContextMenu.Open(targets, activatorGuiPosition);
						}
					}
					else
					{
						lastPopup = EditorPopup.Open(targets, activatorScreenPosition);
						PopupWatcher.Watch(lastPopup);
					}
				}
			}
		}
	}
}