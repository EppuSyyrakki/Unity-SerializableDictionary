﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class SerializableDictionaryPropertyDrawer<TKey, TValue> : PropertyDrawer
{
	GUIContent m_iconPlus = EditorGUIUtility.IconContent ("Toolbar Plus", "|Add");
	GUIContent m_iconMinus = EditorGUIUtility.IconContent ("Toolbar Minus", "|Remove");
	GUIStyle m_buttonStyle = GUIStyle.none;

	enum Action
	{
		None,
		Add,
		Remove
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		label = EditorGUI.BeginProperty(position, label, property);

		Action buttonAction = Action.None;
		int buttonActionIndex = 0;

		UnityEngine.Object scriptInstance = property.serializedObject.targetObject;
		Type scriptType = scriptInstance.GetType();
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		FieldInfo dictionaryField = scriptType.GetField(property.propertyPath, flags);
		IDictionary<TKey, TValue> dictionaryInstance = (IDictionary<TKey, TValue>) dictionaryField.GetValue(scriptInstance);
		Type dictionaryType = dictionaryField.FieldType.BaseType;
		FieldInfo keysField = dictionaryType.GetField("m_keys", flags);
		FieldInfo valuesField = dictionaryType.GetField("m_values", flags);

		var keysProperty = property.FindPropertyRelative("m_keys");
		var valuesProperty = property.FindPropertyRelative("m_values");

		var buttonWidth = m_buttonStyle.CalcSize(m_iconPlus).x;

		var labelPosition = position;
		labelPosition.height = EditorGUIUtility.singleLineHeight;
		if (property.isExpanded) 
			labelPosition.xMax -= m_buttonStyle.CalcSize(m_iconPlus).x;

		EditorGUI.PropertyField(labelPosition, property, label, false);
		// property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
		if (property.isExpanded)
		{
			int dictSize = keysProperty.arraySize;

			var buttonPosition = position;
			buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
			buttonPosition.height = EditorGUIUtility.singleLineHeight;
			if(GUI.Button(buttonPosition, m_iconPlus, m_buttonStyle))
			{			
				buttonAction = Action.Add;
				buttonActionIndex = dictSize;
			}

			EditorGUI.indentLevel++;
			var linePosition = EditorGUI.IndentedRect(position);
			linePosition.y += EditorGUIUtility.singleLineHeight;

			for(int i = 0; i < dictSize; ++i)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);

				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				linePosition.height = lineHeight;

				var keyPosition = linePosition;
				keyPosition.xMax = EditorGUIUtility.labelWidth;
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, false);
				if(EditorGUI.EndChangeCheck())
				{
					for(int j = 0; j < dictSize; j ++)
					{
						if (j != i)
						{
							var keyProperty2 = keysProperty.GetArrayElementAtIndex(j);
							if(EqualsValue(keyProperty2, keyProperty))
							{
								Debug.Log("key[" + i + "] == key[" + j + "]");
							}
						}
					}
				}

				var valuePosition = linePosition;
				valuePosition.xMin = EditorGUIUtility.labelWidth;
				valuePosition.xMax -= buttonWidth;
				EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, false);

				buttonPosition = linePosition;
				buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
				buttonPosition.height = EditorGUIUtility.singleLineHeight;
				if(GUI.Button(buttonPosition, m_iconMinus, m_buttonStyle))
				{
					buttonAction = Action.Remove;
					buttonActionIndex = i;
				}

				linePosition.y += lineHeight;
			}

			EditorGUI.indentLevel--;
		}

		if(buttonAction == Action.Add)
		{
			keysProperty.InsertArrayElementAtIndex(buttonActionIndex);
			valuesProperty.InsertArrayElementAtIndex(buttonActionIndex);
		}
		else if(buttonAction == Action.Remove)
		{
			keysProperty.DeleteArrayElementAtIndex(buttonActionIndex);
			valuesProperty.DeleteArrayElementAtIndex(buttonActionIndex);
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float propertyHeight = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			var keysProperty = property.FindPropertyRelative("m_keys");
			var valuesProperty = property.FindPropertyRelative("m_values");
			int n = keysProperty.arraySize;
			for(int i = 0; i < n; ++i)
			{
				var keyProperty = keysProperty.GetArrayElementAtIndex(i);
				var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
				float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
				float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
				float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
				propertyHeight += lineHeight;
			}
		}

		return propertyHeight;
	}

	static Dictionary<SerializedPropertyType, PropertyInfo> ms_serializedPropertyValueAccessorsDict;

	static SerializableDictionaryPropertyDrawer()
	{
		Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
			{ SerializedPropertyType.Integer, "intValue" },
			{ SerializedPropertyType.Boolean, "boolValue" },
			{ SerializedPropertyType.Float, "floatValue" },
			{ SerializedPropertyType.String, "stringValue" },
			{ SerializedPropertyType.Color, "colorValue" },
			{ SerializedPropertyType.ObjectReference, "objectReferenceValue" },
			{ SerializedPropertyType.LayerMask, "intValue" },
			{ SerializedPropertyType.Enum, "intValue" },
			{ SerializedPropertyType.Vector2, "vector2Value" },
			{ SerializedPropertyType.Vector3, "vector3Value" },
			{ SerializedPropertyType.Vector4, "vector4Value" },
			{ SerializedPropertyType.Rect, "rectValue" },
			{ SerializedPropertyType.ArraySize, "intValue" },
			{ SerializedPropertyType.Character, "intValue" },
			{ SerializedPropertyType.AnimationCurve, "animationCurveValue" },
			{ SerializedPropertyType.Bounds, "boundsValue" },
			{ SerializedPropertyType.Quaternion, "quaternionValue" },
		};
		Type serializedPropertyType = typeof(SerializedProperty);

		ms_serializedPropertyValueAccessorsDict	= new Dictionary<SerializedPropertyType, PropertyInfo>();
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

		foreach(var kvp in serializedPropertyValueAccessorsNameDict)
		{
			PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
			ms_serializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
		}
	}

	static bool EqualsValue(SerializedProperty p1, SerializedProperty p2)
	{
		if(p1.propertyType != p2.propertyType)
			return false;

		PropertyInfo propertyInfo = ms_serializedPropertyValueAccessorsDict[p1.propertyType];
		return object.Equals(propertyInfo.GetValue(p1, null), propertyInfo.GetValue(p2, null));
	}
}

[CustomPropertyDrawer(typeof(DictionaryTest.StringStringDictionary))]
public class StringStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.ColorStringDictionary))]
public class ColorStringDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<Color, string> {}

[CustomPropertyDrawer(typeof(DictionaryTest.StringColorDictionary))]
public class StringColorDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer<string, Color> {}