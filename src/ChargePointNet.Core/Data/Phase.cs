namespace ChargePointNet.Core.Data;

public class Phase
{
    public Phase(int voltage, double current, double powerFactor)
    {
        Voltage = voltage;
        Current = current;
        PowerFactor = powerFactor;
    }

    public int Voltage { get; set; }
    public double Current { get; set; }
    public double? CurrentLimit { get; set; }
    public double PowerFactor { get; set; }
}