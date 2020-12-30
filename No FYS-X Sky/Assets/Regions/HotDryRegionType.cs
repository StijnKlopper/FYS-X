using System.Collections.Generic;

public class HotDryRegionType : RegionType
{

    public HotDryRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new DesertBiomeType());
        this.AvailableBiomes.Add(new ShrublandBiomeType());
    }
}
