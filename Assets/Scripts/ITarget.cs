using UnityEngine;

public interface ITarget
{
    public void GetHit();
    public void GetSpiked(Transform spike);

    public void DisableAllColliders(GameObject obj);

    public Transform Transform { get; }
}