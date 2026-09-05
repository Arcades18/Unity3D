using System;

[Serializable]
// Update DentalData class to include accuracy estimate
public class DentalData
{
    public float[] teethWidthsPixels;
    public float totalWidthPixels;
    public float totalWidthCM;
    public int teethDetected;
    public string errorMessage;
    public bool hasCoinReference;
    public string accuracyEstimate;
    public bool usedAutomaticDetection; // Add this
        // What mode produced the result
       public string analysisMode;

        // Normalized value (0–1 range approx)
        public float normalizedWidthScore;

        // Human-readable result
        public string widthCategory;


    public DentalData()
    {
        teethWidthsPixels = new float[4];
        teethDetected = 0;
        errorMessage = "";
        hasCoinReference = false;
        accuracyEstimate = "Unknown";
        usedAutomaticDetection = true;
    }
}