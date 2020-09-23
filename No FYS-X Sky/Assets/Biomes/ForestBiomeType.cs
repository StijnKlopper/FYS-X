using UnityEngine;
public class ForestBiomeType : BiomeType
{
    public ForestBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
        this.heightMultiplier = 10f;
    }
}
