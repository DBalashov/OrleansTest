namespace GrainInterfaces;

public interface IStreamingGrain : IGrainWithIntegerKey
{
    Task<string> Execute(int value);
}