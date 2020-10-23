using UnityEngine;

public class LockRotationScsript : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
