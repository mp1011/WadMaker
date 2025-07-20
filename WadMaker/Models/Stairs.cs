namespace WadMaker.Models;

public record Stairs(TextureInfo StepTexture, int StartPosition, int EndPosition, int StepWidth, Room[] Rooms)
{
}
