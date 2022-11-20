#pragma warning disable CS8618
namespace ClientWeb.Models;

public class HomeIndexModel
{
    public int     Value      { get; set; }
    public double? Calculated { get; set; }
    public double? Acc        { get; set; }

    public string UserName { get; set; }

    public string State { get; set; }
}