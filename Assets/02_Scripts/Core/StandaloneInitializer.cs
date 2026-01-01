using UnityEngine;

public interface IStandaloneInitializable
{
    void SetupAsStandalone();
}

[DefaultExecutionOrder(-1000)]
public class StandaloneInitializer : MonoBehaviour
{
    private void Awake()
    {
        foreach (var initializable in GetComponentsInChildren<IStandaloneInitializable>())
        {
            initializable.SetupAsStandalone();
        }
    }
}
