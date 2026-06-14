using Source.Core.Contracts;

namespace Source.Core.Interfaces;

public interface IGameView
{
    void Render(in CharacterSheetDto dto);
}
