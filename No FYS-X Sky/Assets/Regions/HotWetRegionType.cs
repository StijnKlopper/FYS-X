using System.Collections.Generic;

public class HotWetRegionType : RegionType
{

    public HotWetRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new ForestBiomeType());
        this.AvailableBiomes.Add(new PlainsBiomeType());
    }
}
