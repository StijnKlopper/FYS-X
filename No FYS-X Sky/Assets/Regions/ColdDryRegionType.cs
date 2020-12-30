using System.Collections.Generic;

public class ColdDryRegionType : RegionType
{

    public ColdDryRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new SnowBiomeType());
        this.AvailableBiomes.Add(new TundraBiomeType());
    }
}
