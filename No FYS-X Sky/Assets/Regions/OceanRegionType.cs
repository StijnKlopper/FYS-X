using System.Collections.Generic;

public class OceanRegionType : RegionType
{

    public OceanRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new OceanBiomeType());
    }
}
