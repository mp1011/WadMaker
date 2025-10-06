namespace WadMaker.Services.StructureGenerators;

internal class StairsGenerator : IMultiRoomStructureGenerator<Stairs>
{
    public IEnumerable<Room> AddStructure(Room hallRoom, Stairs stairs, Side hallSide)
    {
        int totalWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - stairs.EndPosition;
        int numSteps = totalWidth / stairs.StepWidth;
        if (numSteps == 0)
            yield break;

        int heightPerStep = (stairs.EndRoom.Floor - stairs.StartRoom.Floor) / numSteps;
        int currentHeight = stairs.StartRoom.Floor;

        var nextStepPoints = hallRoom.Bounds.GetSegment(hallSide, stairs.StartPosition, stairs.StepWidth);
        int stepNum = 0;
        while (stepNum++ < numSteps)
        {
            currentHeight += heightPerStep;
            yield return CreateStep(stairs, hallRoom, nextStepPoints.Item1, nextStepPoints.Item2, currentHeight);

            nextStepPoints = (nextStepPoints.Item1.Move(hallSide, stairs.StepWidth),
                              nextStepPoints.Item2.Move(hallSide, stairs.StepWidth));
        }

        var remainingWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - numSteps * stairs.StepWidth;

        if (remainingWidth > 0)
        {
            var endSegment = hallRoom.Bounds.GetSegment(hallSide.Opposite(), 0, remainingWidth);
            yield return CreateStep(stairs, hallRoom, endSegment.Item1, endSegment.Item2, stairs.EndRoom.Floor);
        }
    }

    private Room CreateStep(Stairs stairs, Room hallRoom, Point stepUpperLeft, Point stepBottomRight, int stepHeight)
    {
        return stairs.SetOn(new Room
        {
            Floor = stepHeight - hallRoom.Floor,
            UpperLeft = stepUpperLeft,
            BottomRight = stepBottomRight,
            WallTexture = stairs.StepTexture
        });
    }


}
