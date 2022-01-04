using System;
using System.Security.AccessControl;

namespace TopTrailExtractor.Models;

public class Ride
{
    public int RideId { get; set; }
    public int UserId { get; set; }
    public String RideDate { get; set; }
    public String UploadDate { get; set; }
    public double Distance { get; set; }
    public String Unused { get; set; } //This is the N/A Column...
    public String Duration { get; set; }
    public String RideName { get; set; }
    public String Difficulty { get; set; }
    public String Privacy { get; set; }
    public String PublicAccess { get; set; }
    public String Journal { get; set; }
    public String Horse { get; set; }
}