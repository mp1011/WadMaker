
namespace WadMaker.Services.StructureGenerators;

internal class LiftGenerator : IMultiRoomStructureGenerator<HallwayLift>, ISideStructureGenerator<Lift>
{
    public Room AddStructure(Room hallRoom, Lift lift, Side activationSide)
    {
        var liftRoom = hallRoom.AddInnerStructure(size: lift.Size);
        liftRoom.WallTexture = lift.SideTexture;
        liftRoom.Floor = 0;
        liftRoom.Ceiling = 0;
        lift.SetOn(liftRoom);

        var activation = lift.AddWalkTrigger ? Activation.PlayerCross | Activation.PlayerUse | Activation.Repeating
                                             : Activation.PlayerUse | Activation.Repeating;

        liftRoom.LineSpecials[activationSide.Opposite()] = new Plat_DownWaitUpStay(0, Speed.StandardLift, Activation: activation);
        liftRoom.LineSpecials[activationSide] = new Plat_DownWaitUpStay(0, Speed.StandardLift, Activation: activation);

        return liftRoom;       
    }

    public IEnumerable<Room> AddStructure(Room hallRoom, HallwayLift lift, Side hallSide)
    {
        var liftPoints = hallRoom.Bounds.GetSegment(hallSide, lift.PositionInHall, lift.Width);

        var startFloor = lift.StartRoom.Floor;
        var endFloor = lift.EndRoom.Floor;

        int lowerFloor = Math.Min(startFloor, endFloor);
        int upperFloor = Math.Max(startFloor, endFloor);

        var liftRoom = new Room
        {
            UpperLeft = liftPoints.Item1,
            BottomRight = liftPoints.Item2,
            WallTexture = lift.SideTexture,
            Floor = upperFloor - hallRoom.Floor
        };
        lift.SetOn(liftRoom);

        var activation = lift.AddWalkTrigger ? Activation.PlayerCross | Activation.PlayerUse | Activation.Repeating
                                             : Activation.PlayerUse | Activation.Repeating;
        liftRoom.LineSpecials[hallSide.Opposite()] = new Plat_DownWaitUpStay(0, Speed.StandardLift, Activation: activation);
        liftRoom.LineSpecials[hallSide] = new Plat_DownWaitUpStay(0, Speed.StandardLift, Activation: activation);

        yield return liftRoom;

        if (hallRoom.Floor == startFloor)
        {
            int upperSectionWidth = hallRoom.Bounds.AxisLength(hallSide) - lift.PositionInHall - lift.Width;
            var upperSectionPoints = hallRoom.Bounds.GetSegment(hallSide, lift.PositionInHall + lift.Width, upperSectionWidth);

            var upperSection = new Room
            {
                UpperLeft = upperSectionPoints.Item1,
                BottomRight = upperSectionPoints.Item2,
                Floor = endFloor - hallRoom.Floor,
                WallTexture = hallRoom.WallTexture
            };
            yield return upperSection;
        }
        else
        {
            throw new NotImplementedException("implement me");
        }
    }

}
