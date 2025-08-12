using UnityEngine;

public interface IProjectileMover
{
    // owner: ProjectileController, aimPoint: fixed ground target pos, target: optional dynamic target
    void Init(ProjectileController owner, Vector2 aimPoint, Transform target = null);
    void Tick(float deltaTime);
}
