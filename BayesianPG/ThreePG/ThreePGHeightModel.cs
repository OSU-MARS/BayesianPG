namespace BayesianPG.ThreePG
{
    /// <summary>
    /// Specifies equation form used for predicting height and crown length.
    /// </summary>
    public enum ThreePGHeightModel
    {
        // specify r3PG compatible values since Enum.Parse<>() supports integers as well as names
        Power = 1, // Forrester and Tang 2016
        Exponent = 2 // Michajlow function (Forrester et al. 2021, Equation 1)
    }
}
