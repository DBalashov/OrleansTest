using Orleans.Placement;

namespace GrainInterfaces;

public interface IUnitCalculator : Orleans.IGrainWithIntegerKey
{
    Task<double> Calculate(int value);
}