namespace Jumoo.Json;
public static class JsonComparisons
{
    /// <summary>
    /// Compares two objects to see if they are equal in JSON format.
    /// </summary>
    public static bool IsJsonEqual(this object currentObject, object newObject)
    {
        var currentString = currentObject.SerializeJsonString(false);
        var newObjectString = newObject.SerializeJsonString(false);
        return currentString == newObjectString;
    }
}
