namespace WadMaker.Models.BuildingBlocks;

public record Hall(int Width, 
    Room Room1, 
    Room Room2, 
    Room? HallTemplate=null, 
    Door? Door=null, 
    Stairs? Stairs=null,
    Lift? Lift=null);
