using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
  public ParticleSystem[] allParticles;

  [SerializeField] float lifeTime = 1f;

  void Awake()
  {
    //allParticles = GetComponentsInChildren<ParticleSystem>();

    Destroy(gameObject, lifeTime);
  }

  public void Play(Color color)
  {
    AdjustParticlesStartColor(color);
    foreach (ParticleSystem ps in allParticles)
    {
      ps.Stop();
      ps.Play();
    }
  }

  void AdjustParticlesStartColor(Color color)
  {
    foreach (ParticleSystem ps in allParticles)
    {
      //var main = ps.main;
      ps.startColor = new Color(color.r, color.g, color.b, ps.startColor.a);
    }
  }
}
