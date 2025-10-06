namespace WadMaker.Services.StructureGenerators;

public interface IStructureGenerator<T> 
    where T: RoomBuildingBlock
{
    Room AddStructure(Room room, T structure);
}

public interface ISideStructureGenerator<T>
    where T : RoomBuildingBlock
{
    Room AddStructure(Room room, T structure, Side side);
}

public interface IMultiRoomStructureGenerator<T>
    where T : RoomBuildingBlock
{
    IEnumerable<Room> AddStructure(Room room, T structure, Side side);
}

public class StructureGenerator
{
    private readonly IServiceProvider _serviceProvider;

    public StructureGenerator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Room AddStructure<T>(Room room, T structure) where T : RoomBuildingBlock
    {
        var generator = _serviceProvider.GetRequiredService<IStructureGenerator<T>>();
        if (generator == null)
            throw new Exception($"There is no generator defined for type {nameof(T)}");

        return generator.AddStructure(room, structure);
    }

    public Room AddStructure<T>(Room room, T structure, Side side) where T : RoomBuildingBlock
    {
        var generator = _serviceProvider.GetRequiredService<ISideStructureGenerator<T>>();
        if (generator == null)
            throw new Exception($"There is no generator defined for type {nameof(T)}");

        return generator.AddStructure(room, structure, side);
    }


    public IEnumerable<Room> AddMultiRoomStructure<T>(Room room, T structure, Side side) where T : MultiRoomBuildingBlock
    {
        var generator = _serviceProvider.GetRequiredService<IMultiRoomStructureGenerator<T>>();
        if (generator == null)
            throw new Exception($"There is no generator defined for type {nameof(T)}");

        return generator.AddStructure(room, structure, side);
    }

    public void GenerateDoorColorBars(Room hall, Door door, Side hallSide) =>
        _serviceProvider.GetRequiredService<DoorColorBarGenerator>().GenerateDoorColorBars(hall, door, hallSide);

    //deprecated
    public Room GenerateHall(Hall hall) => AddStructure(hall.Room1, hall);
}
