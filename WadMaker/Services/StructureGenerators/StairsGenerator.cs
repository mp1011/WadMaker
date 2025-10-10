namespace WadMaker.Services.StructureGenerators;

internal class StairsGenerator : IMultiRoomStructureGenerator<Stairs>
{
    public IEnumerable<Room> AddStructure(Room hallRoom, Stairs stairs, Side hallSide)
    {
        int totalWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - stairs.EndPosition;
        int numSteps = totalWidth / stairs.StepWidth;
        if (numSteps == 0)
            yield break;

        int floorChangePerStep = (stairs.EndRoom.Floor - stairs.StartRoom.Floor) / numSteps;
        int ceilingChangePerStep = (stairs.EndRoom.Ceiling - stairs.StartRoom.Ceiling) / numSteps;

        int currentFloor = 0;
        int currentCeiling = 0;

        var nextStepPoints = hallRoom.Bounds.GetSegment(hallSide, stairs.StartPosition, stairs.StepWidth);
        int stepNum = 0;
        while (stepNum++ < numSteps)
        {
            currentFloor += floorChangePerStep;
            currentCeiling += ceilingChangePerStep;
            yield return CreateStep(stairs, hallRoom, nextStepPoints.Item1, nextStepPoints.Item2, currentFloor, currentCeiling);

            nextStepPoints = (nextStepPoints.Item1.Move(hallSide, stairs.StepWidth),
                              nextStepPoints.Item2.Move(hallSide, stairs.StepWidth));
        }

        var remainingWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - numSteps * stairs.StepWidth;

        if (remainingWidth > 0)
        {
            var endSegment = hallRoom.Bounds.GetSegment(hallSide.Opposite(), 0, remainingWidth);
            yield return CreateStep(stairs, hallRoom, endSegment.Item1, endSegment.Item2, 
                stairs.EndRoom.Floor - stairs.StartRoom.Floor,
                stairs.EndRoom.Ceiling - stairs.StartRoom.Ceiling);
        }
    }

    private Room CreateStep(Stairs stairs, Room hallRoom, Point stepUpperLeft, Point stepBottomRight, int stepHeight, int ceilingHeight)
    {
        var stepRoom = stairs.StepTemplate?.Copy(hallRoom) ?? new Room(hallRoom);
        stepRoom.Floor = stepHeight;
        if (stairs.FixedCeiling.HasValue)
            stepRoom.Ceiling = stairs.FixedCeiling.Value;
        else
            stepRoom.Ceiling = ceilingHeight;
        stepRoom.UpperLeft = stepUpperLeft;
        stepRoom.BottomRight = stepBottomRight;
        stepRoom.WallTexture = stairs.StepTexture;

        return stairs.SetOn(stepRoom);
    }


}
