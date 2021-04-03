using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

[System.Serializable]
public class InterfaceComponent<TInterface> : ISerializationCallbackReceiver where TInterface : class
{
	[SerializeField] private Component component;
	private TInterface interfacedComponent;

	public TInterface Interface {
		get {
			return interfacedComponent;
		}
		set {
			Component valueAsComponent = value as Component;
			if (System.Object.ReferenceEquals(valueAsComponent, null)) {
				component = null;
				interfacedComponent = null;
				return;
			}
			component = valueAsComponent;
			interfacedComponent = value;
		}
	}
	public Component Component {
		get {
			return component;
		}
		set {
			TInterface valueAsInterface = value as TInterface;
			if (valueAsInterface == null) {
				component = null;
				interfacedComponent = null;
				return;
			}
			component = value;
			interfacedComponent = valueAsInterface;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		interfacedComponent = component as TInterface;
		if (interfacedComponent == null) {
			component = null;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InterfaceComponent<>), true)]
public class InterfaceComponentContainerPropertyDrawer : PropertyDrawer
{
	bool isInitialized = false;
	SerializedProperty componentProperty;
	Type targetInterfaceType;
	Type transformType;
	GUIContent displayLabel;

	private void Initialize(SerializedProperty classProperty, GUIContent label)
	{
		if (isInitialized) return;
		componentProperty = classProperty.FindPropertyRelative("component");
		targetInterfaceType = fieldInfo.FieldType.GetGenericArguments()[0];
		transformType = typeof(Transform);
		displayLabel = new GUIContent(label.text + " (" + targetInterfaceType.ToString() + ")");
		isInitialized = true;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		Initialize(property, label);
		EditorGUI.PropertyField(position, componentProperty, displayLabel);
		var component = componentProperty.objectReferenceValue as Component;
		if (component == null) {
			componentProperty.objectReferenceValue = null;
			return;
		}

		Type currentType = component.GetType();
		if (!targetInterfaceType.IsAssignableFrom(currentType)) {
			// 他の GameObject を引っ張って入れた場合、component に Transform が設定されるので
			// 該当 GameObject から TInterface が無いか検索する
			// If you grab an GameObject and put it on InterfaceCOmponent's inspector,
			// Transform (which is the first "Component") will be serialized (but not you really want to),
			// so search by GetComponent<TInterface>() again
			if (transformType.IsAssignableFrom(currentType)) {
				componentProperty.objectReferenceValue = component.GetComponent(targetInterfaceType);
			} else {
				componentProperty.objectReferenceValue = null;
			}
		}
	}
}
#endif