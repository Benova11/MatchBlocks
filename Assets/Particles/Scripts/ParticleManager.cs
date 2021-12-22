using UnityEngine;

public class ParticleManager : MonoBehaviour
{
  public GameObject clearFXPrefab;

  public void ClearPieceFXAt(Color color,int x, int y, int z = 0)
  {
    if (clearFXPrefab != null)
    {
      GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity);
      ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();
      if (particlePlayer != null)
        particlePlayer.Play(color);
    }
  }

}
