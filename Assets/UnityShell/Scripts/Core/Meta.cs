using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace UnityShell
{

static public class Meta
{
	// --------------------------------------------------------------------------------
	// Components
	// --------------------------------------------------------------------------------

	static public Component[] GetAllComponents(GameObject instance)
	{
		return instance.GetComponents<Component>();
	}


	static public string[] GetAllComponentsNames(GameObject instance)
	{
		return GetAllComponents(instance).Select(x => x.GetType().Name).ToArray();
	}



	// --------------------------------------------------------------------------------
	// Fields
	// --------------------------------------------------------------------------------

	static public FieldInfo[] GetAllFields(
		Object instance,
		BindingFlags flags =
			BindingFlags.Public    |
			BindingFlags.NonPublic |
			BindingFlags.Instance  |
			BindingFlags.Static)
	{
		return instance.GetType().GetFields(flags);
	}

	static public string[] GetAllFieldsNames(Object instance)
	{
		return GetAllFields(instance).Select(x => x.Name).ToArray();
	}

	static public FieldInfo GetField(Object instance, string fieldName)
	{
		var fields = GetAllFields(instance).Where(x => x.Name == fieldName);

		if (fields.Count() == 0) {
			return null;
		}

		return fields.Count() > 0 ? fields.First() : null;
	}



	// --------------------------------------------------------------------------------
	// Methods
	// --------------------------------------------------------------------------------

	static public MethodInfo[] GetAllMethods(
		Object instance,
		BindingFlags flags =
			BindingFlags.Public    |
			BindingFlags.NonPublic |
			BindingFlags.Instance  |
			BindingFlags.Static)
	{
		return instance.GetType().GetMethods(flags);
	}

	static public string[] GetAllMethodsNames(Object instance)
	{
		return GetAllMethods(instance).Select(x => x.Name).ToArray();
	}

	static public MethodInfo GetMethod(Object instance, string methodName)
	{
		var methods = GetAllMethods(instance).Where(x => x.Name == methodName);

		if (methods.Count() == 0) {
			return null;
		}

		// TODO: find methods with arguments
		return methods.Count() > 0 ? methods.First() : null;
	}



	// --------------------------------------------------------------------------------
	// Properties
	// --------------------------------------------------------------------------------

	static public PropertyInfo[] GetAllProperties(
		Object instance,
		BindingFlags flags =
			BindingFlags.Public    |
			BindingFlags.NonPublic |
			BindingFlags.Instance  |
			BindingFlags.Static)
	{
		return instance.GetType().GetProperties(flags);
	}

	static public string[] GetAllPropertiesNames(Object instance)
	{
		return GetAllProperties(instance).Select(x => x.Name).ToArray();
	}

	static public PropertyInfo GetProperty(Object instance, string propertyName)
	{
		var properties = GetAllProperties(instance).Where(x => x.Name == propertyName);

		if (properties.Count() == 0) {
			return null;
		}

		return properties.Count() > 0 ? properties.First() : null;
	}

}

}
