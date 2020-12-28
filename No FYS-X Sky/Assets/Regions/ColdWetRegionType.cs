using System.Collections.Generic;

public class ColdWetRegionType : RegionType
{

    public ColdWetRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new SnowBiomeType());
        this.AvailableBiomes.Add(new TaigaBiomeType());
    }
}
