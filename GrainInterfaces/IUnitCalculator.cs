using Orleans.Placement;

namespace GrainInterfaces;

public interface IUnitCalculator : Orleans.IGrainWithIntegerKey
{
    Task<double> Calculate(int value);
    
    Task<double> Multi(Parms p);
}

[GenerateSerializer]
public record Parms(double X, double Y);